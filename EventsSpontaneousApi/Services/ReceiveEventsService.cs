using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hafslund.Telemetry;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace EventsSpontaneousApi.Services
{
    public class ReceiveEventsService: DefaultContractResolver, IIECReceiveEvents
    {
        private readonly ITelemetryInsightsLogger _telemetry;
        private readonly string source = "hes-events-spontaneous-api";
        private readonly IEventHubService _eventHubService;

        public ReceiveEventsService(ITelemetryInsightsLogger telemetry, IEventHubService eventHubService)
        {
            _telemetry = telemetry;
            _eventHubService = eventHubService;
        }

        public async Task<CreatedConfigurationEventResponse> CreatedConfigurationEventAsync(CreatedConfigurationEventRequest request)
        {
            var createdConfigurationEventResponse = new CreatedConfigurationEventResponse(new CreatedConfigurationEventResponseMessageType
            {
                Header = EventReceiverServiceUtility.GetCreatedEventHeaderType(request.CreatedConfigurationEventRequest1, source),
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

                await _eventHubService.SendAsync(message);
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
                var json = RequestHelper.BuildJsonPrivateMembers(eventObject);
                var message = new EventData(Encoding.UTF8.GetBytes(json));

                await _eventHubService.SendAsync(message);
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex);
                throw;
            }
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