using tracksByPopularity.models;
using tracksByPopularity.src.models;

namespace tracksByPopularity.src.helpers;

public static class QueryParamHelper
{
    public static TimeRangeEnum GetTimeRangeQueryParam(HttpContext context)
    {
        var timeRange = context.Request.Query["timeRange"].FirstOrDefault();

        if (string.IsNullOrEmpty(timeRange))
        {
            return TimeRangeEnum.NotValid;
        }

        // convert timeRange from string to enum
        return !Enum.TryParse<TimeRangeEnum>(timeRange, out var timeRangeEnum)
            ? TimeRangeEnum.NotValid
            : timeRangeEnum;
    }
}
