﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
	// Controller for sync actions
	[Route("api/[controller]")]
	[ApiController]
	public class TodoController : ControllerBase
	{
		private readonly TodoDbContext _dbContext;
		public TodoController(TodoDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		// GET api/todo
		[HttpGet]
		public ActionResult<string> Welcome()
		{
			int maxWorkerThreads, maxPortThreads, availableWorkerThreads, availablePortThreads;
			ThreadPool.SetMaxThreads(50, 50);
			ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);
			ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availablePortThreads);

			Task.Delay(100).Wait();

			return $@"Welcome to the Todo API (worker threads: { maxWorkerThreads - availableWorkerThreads } / { maxWorkerThreads }; I/O threads: { maxPortThreads - availablePortThreads } / { maxPortThreads }; Time: { DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") }).
Operations:
- GET /todo/list (list all todo's)
- GET /todo/id (get a todo item)
- POST /todo (add a new todo item)
- PUT /todo/id (replace a todo item)
- PATCH /todo/id (update a todo item)
- DELETE /todo/id (delete a todo item)";
		}

		// GET api/todo/list
		[HttpGet("list")]
		public ActionResult<IEnumerable<TodoItem>> GetTodoList()
		{
			return _dbContext.TodoItems.ToList();
		}

		// GET api/todo/{id}
		[HttpGet("{id}")]
		public ActionResult<TodoItem> GetTodoItem(long id)
		{
			var todoItem = _dbContext.TodoItems.Find(id);

			if (todoItem == null)
				return NotFound();

			return todoItem;
		}

		// POST api/todo
		[HttpPost]
		public ActionResult<TodoItem> AddTodoItem(TodoItem item)
		{
			_dbContext.TodoItems.Add(item);
			_dbContext.SaveChanges();

			return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, item);
		}

		// PUT api/todo/{id}
		[HttpPut("{id}")]
		public ActionResult UpdateTodoItem(long id, TodoItem item)
		{
			if (id != item.Id)
				return BadRequest();

			_dbContext.Entry(item).State = EntityState.Modified;
			_dbContext.SaveChanges();

			return NoContent();
		}

		// PATCH api/todo/{id}
		[HttpPatch("{id}")]
		public ActionResult<TodoItem> PatchTodoItem(long id, JsonPatchDocument<TodoItem> patchDocument)
		{
			if (patchDocument == null)
				return BadRequest(ModelState);

			var todoItem = _dbContext.TodoItems.Find(id);

			if (todoItem == null)
				return NotFound();

			patchDocument.ApplyTo<TodoItem>(todoItem, ModelState);

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			_dbContext.SaveChanges();

			return new ObjectResult(todoItem);
		}

		// DELETE api/todo/{id}
		[HttpDelete("{id}")]
		public ActionResult DeleteTodoItem(long id)
		{
			var todoItem = _dbContext.TodoItems.Find(id);

			if (todoItem == null)
				return NotFound();

			_dbContext.TodoItems.Remove(todoItem);
			_dbContext.SaveChanges();

			return NoContent();
		}
	}

	// Controller for async actions
	[Route("api/[controller]")]
	[ApiController]
	public class TodoAsyncController : ControllerBase
	{
		private readonly TodoDbContext _dbContext;
		public TodoAsyncController(TodoDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		// GET api/todo/list
		[HttpGet("list")]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoListAsync()
		{
			return await _dbContext.TodoItems.ToListAsync();
		}

		// GET api/todo/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<TodoItem>> GetTodoItemAsync(long id)
		{
			var todoItem = await _dbContext.TodoItems.FindAsync(id);

			if (todoItem == null)
				return NotFound();

			return todoItem;
		}

		// POST api/todo
		[HttpPost]
		public async Task<ActionResult<TodoItem>> AddTodoItemAsync(TodoItem item)
		{
			_dbContext.TodoItems.Add(item);
			await _dbContext.SaveChangesAsync();

			return CreatedAtAction(nameof(GetTodoItemAsync), new { id = item.Id }, item);
		}

		// PUT api/todo/{id}
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateTodoItemAsync(long id, TodoItem item)
		{
			if (id != item.Id)
				return BadRequest();

			_dbContext.Entry(item).State = EntityState.Modified;
			await _dbContext.SaveChangesAsync();

			return NoContent();
		}

		// PATCH api/todo/{id}
		[HttpPatch("{id}")]
		public async Task<ActionResult<TodoItem>> PatchTodoItemAsync(long id, JsonPatchDocument<TodoItem> patchDocument)
		{
			if (patchDocument == null)
				return BadRequest(ModelState);

			var todoItem = await _dbContext.TodoItems.FindAsync(id);

			if (todoItem == null)
				return NotFound();

			patchDocument.ApplyTo<TodoItem>(todoItem, ModelState);

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _dbContext.SaveChangesAsync();

			return new ObjectResult(todoItem);
		}

		// DELETE api/todo/{id}
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteTodoItemAsync(long id)
		{
			var todoItem = await _dbContext.TodoItems.FindAsync(id);

			if (todoItem == null)
				return NotFound();

			_dbContext.TodoItems.Remove(todoItem);
			await _dbContext.SaveChangesAsync();

			return NoContent();
		}
	}
}
