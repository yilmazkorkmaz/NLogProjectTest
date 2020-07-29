using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLogProjectTest.Models;
using Microsoft.Extensions.Logging;

namespace NLogProjectTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ILogger<TodoItemsController> _logger;
        private readonly TodoContext _context;

        public TodoItemsController(ILogger<TodoItemsController> logger, TodoContext context)
        {
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into TodoItemsController");

            _context = context;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            _logger.LogDebug("init HttpGet");
            try
            {
                return await _context.TodoItems.Select(x => ItemToDTO(x)).ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "GetTodoItems");
                return NotFound();
            }
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            _logger.LogDebug("init HttpGet {id}", id);
            try
            {
                var todoItem = await _context.TodoItems.FindAsync(id);

                if (todoItem == null)
                {
                    _logger.LogWarning("HttpGet {id}", id);
                    return NotFound();
                }

                return todoItem;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "HttpGet {id}", id);
                return NotFound();
            }
        }

        // PUT: api/TodoItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItemDTO todoItemDTO)
        {
            _logger.LogDebug("init HttpPut {id}", id);

            if (id != todoItemDTO.Id)
            {
                _logger.LogWarning("HttpPut:{id}, todoItem.Id:{todoItemDTO.Id} different ids", id, todoItemDTO.Id);
                return BadRequest();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                _logger.LogWarning("HttpPut {id} not found", id);
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogDebug("success HttpPut {id}", id);
            }
            catch (DbUpdateConcurrencyException exception) when (!TodoItemExists(id))
            {
                _logger.LogError(exception, "HttpPut {id}", id);
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/TodoItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            _logger.LogDebug("init HttpPost");

            var todoItem = new TodoItem
            {
                IsComplete = todoItemDTO.IsComplete,
                Name = todoItemDTO.Name
            };

            try
            {
                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();
                _logger.LogDebug("success HttpPost {id}", todoItem.Id);

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "HttpPost Name:{Name}, IsComplete:{IsComplete}", todoItemDTO.Name, todoItemDTO.IsComplete);
                return NotFound();
            }

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, ItemToDTO(todoItem));
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
        {
            _logger.LogDebug("init HttpDelete {id}", id);

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                _logger.LogWarning("HttpDelete {id} not found", id);
                return NotFound();
            }

            try
            {
                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();
                _logger.LogDebug("success HttpDelete {id}", id);

                return todoItem;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "HttpPut {id}", id);
                return NotFound();
            }
        }

        private bool TodoItemExists(long id)
        {
            _logger.LogDebug("init TodoItemExists {id}", id);
            try
            {
                return _context.TodoItems.Any(e => e.Id == id);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "TodoItemExists {id}", id);
                throw;
            }
        }

        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };
    }
}
