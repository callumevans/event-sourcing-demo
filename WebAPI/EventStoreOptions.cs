namespace WebAPI
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; }
        
        public string ConnectionName { get; set; }
        
        public string StreamName { get; set; }
        
        public string GroupName { get; set; }
    }
}