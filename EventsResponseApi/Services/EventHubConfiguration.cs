using System;
using Microsoft.Azure.EventHubs;

namespace EventsResponseApi.Services
{
    public class EventHubConfiguration
    {
        public EventHubClient HubClient { get; }

        public EventHubConfiguration(string eventhubConnectionString, string eventHubName)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventhubConnectionString)
            {
                EntityPath = eventHubName
            };
            HubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }
    }
}