using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebAPI.Events;
using WebAPI.Models;

namespace WebAPI
{
    public class EventStoreClient
    {
        private readonly IEventStoreConnection connection;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptionsMonitor<EventStoreOptions> eventStoreOptions;
        
        public EventStoreClient(
            IServiceProvider serviceProvider,
            IOptionsMonitor<EventStoreOptions> eventStoreOptions)
        {
            this.serviceProvider = serviceProvider;
            this.eventStoreOptions = eventStoreOptions;
            
            connection = EventStoreConnection.Create(
                uri: new Uri(this.eventStoreOptions.CurrentValue.ConnectionString),
                connectionName: this.eventStoreOptions.CurrentValue.ConnectionName);
        }

        public async Task Start()
        {
            await connection.ConnectAsync();

            await connection.ConnectToPersistentSubscriptionAsync(
                eventStoreOptions.CurrentValue.StreamName,
                eventStoreOptions.CurrentValue.GroupName,
                ((_, message) =>
                {                                        
                    var plainText = Encoding.ASCII.GetString(message.Event.Data);

                    Console.WriteLine($"Received message of type: {message.Event.EventType}");
                    Console.WriteLine(plainText);
                    
                    using (var scope = serviceProvider.CreateScope())
                    {
                        switch (message.Event.EventType)
                        {
                            case EventTypes.ToDoCreated:
                            {
                                var parsedEvent = JsonConvert.DeserializeObject<CreateToDoItem>(plainText);
                                var dbContext = scope.ServiceProvider.GetService<ToDoListContext>();

                                dbContext.ToDoItems.Add(parsedEvent.ToDoItem);
                                dbContext.SaveChanges();
                                break;
                            }
                            case EventTypes.ToDoDeleted:
                            {
                                var parsedEvent = JsonConvert.DeserializeObject<DeleteToDoItem>(plainText);
                                var dbContext = scope.ServiceProvider.GetService<ToDoListContext>();

                                dbContext.ToDoItems.Remove(new ToDoItem
                                {
                                    Id = parsedEvent.Id
                                });

                                dbContext.SaveChanges();
                                break;
                            }
                        }
                    }
                }));
        }

        public Task Stop()
        {
            connection.Close();
            return Task.CompletedTask;
        }

        public Task PublishEvent(string eventType, object data)
        {
            return connection.AppendToStreamAsync(
                eventStoreOptions.CurrentValue.StreamName, 
                ExpectedVersion.Any, 
                new EventData(Guid.NewGuid(), 
                    eventType,
                    true, 
                    Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data)),
                    new byte [] { }
                    ));
        }
    }
}