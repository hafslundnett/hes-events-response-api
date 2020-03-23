using System;
using System.Collections.Generic;
using EventsResponseApi.Services;
using Hafslund.Telemetry;
using Xunit;
using Moq;
using Newtonsoft.Json.Linq;

namespace EventsResponseApi.Tests
{
    public class RequestHelperTests
    {
        private Mock<ITelemetryInsightsLogger> _telemetryMock;

        public RequestHelperTests()
        {
            _telemetryMock = new Mock<ITelemetryInsightsLogger>();
        }

        [Fact]
        public void CreateCreatedEndDeviceEventRequest_BuildEventdata_VerifyPrivateMembersJson()
        {
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
                    createdDateTime = DateTime.Parse("2020-03-20T12:15:00Z"),
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
                    Timestamp = DateTime.Parse("2020-03-20T12:15:00Z"),
                    UseGuaranteedDelivery = false,
                    Verb = HeaderTypeVerb.created
                },
                Payload = new EndDeviceEventPayloadType
                {
                    EndDeviceEvents = endDeviceEvents.ToArray()
                }
            };

            var json = RequestHelper.BuildJsonPrivateMembers(request);

            var expected = @"{

        ""headerField"": {
		""verbField"": 5,
		""nounField"": ""EndDeviceEvent"",
		""timestampField"": ""\/Date(1584706500000+0100)\/"",
		""sourceField"": ""Trouble"",
		""replyAddressField"": ""Devils land"",
		""messageIDField"": ""12345"",
		""correlationIDField"": ""1234"",
		""accessTokenField"": ""123"",
		""organisationIDField"": ""666"",
		""useGuaranteedDeliveryFieldSpecified"": false
	},
	""payloadField"": {
		""endDeviceEventsField"": [{
			""createdDateTimeField"": ""\/Date(1584706500000+0100)\/"",
			""endDeviceEventDetailsField"": [{
				""nameField"": ""DetectionActive"",
				""valueField"": ""true""
			}],
			""endDeviceEventTypeField"": {
				""typeField"": ""3"",
				""domainField"": ""26"",
				""subDomainField"": ""126"",
				""eventOrActionField"": ""85""
			},
			""meterReadingField"": {
				""readingsField"": [{
					""valueField"": ""231.34"",
					""readingTypeField"": {
						""refField"": ""0.0.6.0.0.4.0.0.64.0.5""
					}
				}]
			},
			""usagePointField"": {
				""mRIDField"": ""666""
			},
			""endDeviceField"": {
				""mRIDField"": ""123456789""
			}
		}]
	}
}";
            JObject xptJson = JObject.Parse(expected);
            JObject actualJson = JObject.Parse(json);

            Assert.Equal(xptJson, actualJson);
        }
    }
}
