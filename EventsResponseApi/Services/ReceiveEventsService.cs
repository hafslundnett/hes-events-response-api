using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventsResponseApi.Contract;
using Hafslund.Telemetry;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace EventsResponseApi.Services
{
    public class ReceiveEventsService: DefaultContractResolver, IIECReceiveEvents
    {
        private readonly ITelemetryInsightsLogger _telemetry;
        private readonly string source = "hes-events-response-api";
        private static EventHubClient _eventHubClient;

        public ReceiveEventsService(ITelemetryInsightsLogger telemetry, EventHubConfiguration eventHubConfiguration)
        {
            _telemetry = telemetry;
            _eventHubClient = eventHubConfiguration.HubClient;
        }

        public async Task<CreatedConfigurationEventResponse> CreatedConfigurationEventAsync(CreatedConfigurationEventRequest request)
        {
            var createdConfigurationEventResponse = new CreatedConfigurationEventResponse(new CreatedConfigurationEventResponseMessageType
            {
                Header = EventReceiverServiceUtility.GetCreatedEventHeaderType(request, source),
                Reply = EventReceiverServiceUtility.GetOkReplyType()
            });

            try
            {
                TrackTrace(request.CreatedConfigurationEventRequest1.Header);
                await SendCreatedEventsToEventHub(request);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                return EventReceiverServiceUtility.GetFailedCreatedConfigurationEventResponse(request, ex, source, ResultCodes.OperationFailed);
            }

            return createdConfigurationEventResponse;
        }

        private async Task SendCreatedEventsToEventHub(CreatedConfigurationEventRequest eventObject)
        {
            try
            {
                var json = JsonConvert.SerializeObject(eventObject);
                var message = new EventData(Encoding.UTF8.GetBytes(json));

                await _eventHubClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                throw;
            }
        }

        public async Task<CreatedEndDeviceEventResponse> CreatedEndDeviceEventAsync(CreatedEndDeviceEventRequest request)
        {
            try
            {
                TrackTrace(request.CreatedEndDeviceEventRequest1.Header);
                await SendCreatedEventsToEventHub(request);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                return EventReceiverServiceUtility.GetFailedCreatedEndDeviceEventResponse(request, ex, source, ResultCodes.OperationFailed);
            }
            return new CreatedEndDeviceEventResponse(new CreatedEndDeviceEventResponseMessageType
            {
                Header = EventReceiverServiceUtility.GetCreatedEventHeaderType(request.CreatedEndDeviceEventRequest1, source),
                Reply = EventReceiverServiceUtility.GetOkReplyType()
            });
        }

        private async Task SendCreatedEventsToEventHub(CreatedEndDeviceEventRequest eventObject)
        {
            try
            {
                var json = BuildJsonPrivateMembers(eventObject);
                var message = new EventData(Encoding.UTF8.GetBytes(json));

                await _eventHubClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                throw;
            }
        }

        private static string BuildJsonPrivateMembers(CreatedEndDeviceEventRequest eventObject)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            var payloadjson = JsonConvert.SerializeObject(eventObject.CreatedEndDeviceEventRequest1.Payload, jss);
            var jsonHeader = JsonConvert.SerializeObject(eventObject.CreatedEndDeviceEventRequest1.Header, jss);

            return "{\"headerField\":" + jsonHeader + ",\"payloadField\": " + payloadjson + "}";
        }

        private void TrackTrace(HeaderType header)
        {
            _telemetry.TrackTrace("Event received",
                new Dictionary<string, string>
                {
                    {"CorrelationId", header?.CorrelationID },
                    {"MessageId", header?.MessageID },
                    {"Noun", header?.Noun },
                    {"Source", header?.Source },
                    {"Verb", header?.Verb.ToString() },
                });
        }
    }
}