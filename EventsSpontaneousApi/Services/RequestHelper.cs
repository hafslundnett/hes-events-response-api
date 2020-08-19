using EventsSpontaneousApi.Contract;
using Newtonsoft.Json;

namespace EventsSpontaneousApi.Services
{
    public class RequestHelper
    {
        public static string BuildJsonPrivateMembers(CreatedEndDeviceEventRequest eventObject)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            var payloadJson = JsonConvert.SerializeObject(eventObject.CreatedEndDeviceEventRequest1.Payload, jss);
            var headerJson = JsonConvert.SerializeObject(eventObject.CreatedEndDeviceEventRequest1.Header, jss);

            return "{\"headerField\":" + headerJson + ",\"payloadField\": " + payloadJson + "}";
        }
    }
}