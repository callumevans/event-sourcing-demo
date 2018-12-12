using WebAPI.Models;

namespace WebAPI.Events
{
    public class CreateToDoItem
    {
        public ToDoItem ToDoItem { get; set; }
    }
}