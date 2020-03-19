using System;

namespace EventsResponseApi.Services
{
    public class EventReceiverServiceUtility
    {
        public static HeaderType GetCreatedEventHeaderType(object createdEventRequestMessageType, string source)
        {
            return new HeaderType
            {
                Noun = createdEventRequestMessageType.GetType() == typeof(CreatedConfigurationEventRequestMessageType)
                    ? ((CreatedConfigurationEventRequestMessageType) createdEventRequestMessageType).Header.Noun
                    : ((CreatedEndDeviceEventRequestMessageType) createdEventRequestMessageType).Header.Noun,
                Verb = createdEventRequestMessageType.GetType() == typeof(CreatedConfigurationEventRequestMessageType)
                    ? ((CreatedConfigurationEventRequestMessageType) createdEventRequestMessageType).Header.Verb
                    : ((CreatedEndDeviceEventRequestMessageType) createdEventRequestMessageType).Header.Verb,
                Timestamp = DateTime.UtcNow,
                Source = source,
                MessageID = DateTime.UtcNow.ToString(),
                CorrelationID = createdEventRequestMessageType.GetType() ==
                                typeof(CreatedConfigurationEventRequestMessageType)
                    ? ((CreatedConfigurationEventRequestMessageType) createdEventRequestMessageType).Header
                    .CorrelationID
                    : ((CreatedEndDeviceEventRequestMessageType) createdEventRequestMessageType).Header.CorrelationID
            };
        }

        public static CreatedConfigurationEventResponse GetFailedCreatedConfigurationEventResponse(
            CreatedConfigurationEventRequest request, Exception ex, string source, string faultCode)
        {
            return new CreatedConfigurationEventResponse(new CreatedConfigurationEventResponseMessageType
            {
                Header = GetCreatedEventHeaderType(request.CreatedConfigurationEventRequest1, source),
                Reply = GetFailedReplyType(ex, faultCode)
            });
        }

        public static CreatedEndDeviceEventResponse GetFailedCreatedEndDeviceEventResponse(
            CreatedEndDeviceEventRequest request, Exception ex, string source, string faultCode)
        {
            return new CreatedEndDeviceEventResponse(new CreatedEndDeviceEventResponseMessageType
            {
                Header = GetCreatedEventHeaderType(request.CreatedEndDeviceEventRequest1, source),
                Reply = GetFailedReplyType(ex, faultCode)
            });
        }

        public static ReplyType GetOkReplyType()
        {
            return new ReplyType
            {
                Result = ReplyTypeResult.OK
            };
        }

        public static ReplyType GetFailedReplyType(Exception ex, string faultCode)
        {
            return new ReplyType
            {
                Result = ReplyTypeResult.FAILED,
                Error = new[]
                {
                    new ErrorType
                    {
                        code = faultCode,
                        level = ErrorTypeLevel.FATAL,
                        reason = ex?.Message,
                        details = ex?.InnerException?.Message,
                        stackTrace = ex?.StackTrace
                    }
                }
            };
        }
    }
}