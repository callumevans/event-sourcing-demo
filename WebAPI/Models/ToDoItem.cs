using System;

namespace WebAPI.Models
{
    public class ToDoItem
    {
        public string Id { get; set; }
        
        public string Text { get; set; }
        
        public DateTimeOffset TimeStamp { get; set; }
    }
}