using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventsSpontaneousApi.Services;
using Hafslund.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.EventHubs;
using Xunit;
using Moq;

namespace EventsSpontaneousApi.Tests
{
    public class ReceiveEventsServiceTests
    {
        private readonly Mock<ITelemetryInsightsLogger> _telemetryMock;
        private readonly Mock<IEventHubService> _eventHubServiceMock;

        public ReceiveEventsServiceTests()
        {
            _telemetryMock = new Mock<ITelemetryInsightsLogger>();
            _eventHubServiceMock = new Mock<IEventHubService>();
        }

        [Fact]
        public async Task CreatedEndDeviceEventRequest_MessageCreated_MessageIsSentAndTraceTracked()
        {
            ReceiveEventsService receiveEventsService = new ReceiveEventsService(_telemetryMock.Object, _eventHubServiceMock.Object);
            CreatedEndDeviceEventRequest request = new CreatedEndDeviceEventRequest();

            List<EndDeviceEventDetail> listDetails = new List<EndDeviceEventDetail>
            {
                new EndDeviceEventDetail
                {
                    name = "DetectionActive",
                    value = "true"
                }
            };
            var readingList = new List<Reading>
            {
                new Reading
                {
                    ReadingType = new ReadingReadingType
                    {
                        @ref = "0.0.6.0.0.4.0.0.64.0.5"
                    },
                    value = "231.34"
                }
            };
            var endDeviceEvents = new List<EndDeviceEvent>
            {
                new EndDeviceEvent
                {
                    createdDateTime = DateTime.Now,
                    EndDevice = new identifiedObject
                    {
                        mRID = "123456789"
                    },
                    EndDeviceEventDetails = listDetails.ToArray(),
                    EndDeviceEventType = new EndDeviceEventType
                    {
                        domain = "26",
                        type = "3",
                        subDomain = "126",
                        eventOrAction = "85"
                    },
                    MeterReading = new MeterReading
                    {
                        Readings = readingList.ToArray()
                    },
                    UsagePoint = new identifiedObject
                    {
                        mRID = "666"
                    }
                }
            };
            request.CreatedEndDeviceEventRequest1 = new CreatedEndDeviceEventRequestMessageType
            {
                Header = new HeaderType
                {
                    Noun = "EndDeviceEvent",
                    AccessToken = "123",
                    CorrelationID = "1234",
                    MessageID = "12345",
                    OrganisationID = "666",
                    Source = "Trouble",
                    ReplyAddress = "Devils land",
                    Timestamp = DateTime.Now,
                    UseGuaranteedDelivery = false,
                    Verb = HeaderTypeVerb.created
                },
                Payload = new EndDeviceEventPayloadType
                {
                    EndDeviceEvents = endDeviceEvents.ToArray()
                }
            };

            await receiveEventsService.CreatedEndDeviceEventAsync(request);


            var prop = new Dictionary<string, string>
            {
                {"CorrelationId", "1234" },
                {"MessageId", "12345" },
                {"Noun", "EndDeviceEvent" },
                {"Source","Trouble" },
                {"Verb", HeaderTypeVerb.created.ToString() },
            };

            var message = new EventData(Encoding.UTF8.GetBytes(RequestHelper.BuildJsonPrivateMembers(request)));

            _telemetryMock.Verify(x => x.TrackTrace("Event received", prop, It.IsAny<bool>(), SeverityLevel.Information));
            _eventHubServiceMock.Verify(x => x.SendAsync(It.IsAny<EventData>()), Times.Once);
        }
    }
}
