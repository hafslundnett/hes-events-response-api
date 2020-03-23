using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace EventsResponseApi.Services
{
    public interface IEventHubService
    {
        void Disconnect();
        Task SendAsync(EventData eventData);
    }
}