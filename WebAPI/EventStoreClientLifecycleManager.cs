using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WebAPI
{
    public class EventStoreClientLifecycleManager : IHostedService
    {
        private readonly EventStoreClient eventStoreClient;
        
        public EventStoreClientLifecycleManager(EventStoreClient eventStoreClient)
        {
            this.eventStoreClient = eventStoreClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return eventStoreClient.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return eventStoreClient.Stop();
        }
    }
}