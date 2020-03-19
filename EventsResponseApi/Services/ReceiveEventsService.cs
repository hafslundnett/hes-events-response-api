using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hafslund.Telemetry;
using Microsoft.Azure.EventHubs;

namespace EventsResponseApi.Services
{
    public class ReceiveEventsService: IIECReceiveEvents
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
                await SendCreatedEventsToEventHub(createdConfigurationEventResponse);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                return EventReceiverServiceUtility.GetFailedCreatedConfigurationEventResponse(request, ex, source, ResultCodes.OperationFailed);
            }

            return createdConfigurationEventResponse;
        }

        public async Task<CreatedEndDeviceEventResponse> CreatedEndDeviceEventAsync(CreatedEndDeviceEventRequest request)
        {
            try
            {
                TrackTrace(request.CreatedEndDeviceEventRequest1.Header);
                await SendCreatedEventsToEventHub(request.CreatedEndDeviceEventRequest1);
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

        private async Task SendCreatedEventsToEventHub(object eventObject)
        {
            var ser = JsonSerializer.Serialize(eventObject, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
            var message = new EventData(Encoding.UTF8.GetBytes(ser));

            await _eventHubClient.SendAsync(message);
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

        private void TrackFinalResponse(HeaderType header, int sentResponses)
        {
            // All async communication with Aidon ends with a final response (receipt) that reports the number of async messages sent.
            // These receipts should not be added to the service bus topic
            _telemetry.TrackTrace("Final response received", new
            {
                CorrelationId = header.CorrelationID,
                MessageId = header.MessageID,
                SentResponses = sentResponses
            });
        }

    }
}