using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace EventsResponseApi.Services
{
    public class EventHubService : IEventHubService
    {
        public EventHubClient HubClient { get; }

        public EventHubService(string eventhubConnectionString, string eventHubName)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventhubConnectionString)
            {
                EntityPath = eventHubName
            };
            HubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        public async void Disconnect()
        {
            await HubClient.CloseAsync();
        }

        public Task SendAsync(EventData eventData)
        {
            return HubClient.SendAsync(eventData);
        }
    }
}