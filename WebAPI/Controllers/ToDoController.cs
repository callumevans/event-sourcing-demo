using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Events;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("todo")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly ToDoListContext dbContext;
        private readonly EventStoreClient eventStoreClient;

        public ToDoController(
            ToDoListContext dbContext,
            EventStoreClient eventStoreClient)
        {
            this.dbContext = dbContext;
            this.eventStoreClient = eventStoreClient;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoItem>>> Get()
        {
            return Ok(await dbContext.ToDoItems.ToListAsync());
        }
        
        [HttpGet("{id}", Name = "GetToDoItem")]
        public async Task<ActionResult<ToDoItem>> Get([FromRoute]string id)
        {
            ToDoItem item = await dbContext.ToDoItems.SingleOrDefaultAsync(x => x.Id.Equals(id));

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
        
        [HttpDelete("{id}", Name = "GetToDoItem")]
        public async Task<ActionResult<ToDoItem>> Delete([FromRoute]string id)
        {
            await eventStoreClient.PublishEvent(
                EventTypes.ToDoDeleted, new DeleteToDoItem
                {
                    Id = id
                });

            return Accepted();
        }
        
        [HttpPost]
        public async Task<ActionResult<ToDoItem>> Post([FromForm]string text)
        {
            var toDoItem = new ToDoItem
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                TimeStamp = DateTimeOffset.UtcNow
            };
            
            await eventStoreClient.PublishEvent(
                EventTypes.ToDoCreated, new CreateToDoItem
                {
                    ToDoItem = toDoItem
                });
            
            return CreatedAtRoute("GetToDoItem", new { toDoItem.Id }, value: toDoItem);
        }
    }
}