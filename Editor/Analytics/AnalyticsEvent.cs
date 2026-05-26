using System;
#if UNITY_6000_3_OR_NEWER
using UnityEngine.Analytics;
#endif

namespace Unity.SharpZipLib.Editor {
internal class AnalyticsEventData
#if UNITY_6000_3_OR_NEWER
    : IAnalytic.IData
#endif
{
    public string actualPackageVersion;
}

internal abstract class AnalyticsEvent
#if UNITY_6000_3_OR_NEWER
    : IAnalytic
#endif
{

    internal abstract string eventName       { get; }
    internal virtual  int    version         => 1;
    internal virtual  int    maxEventPerHour => 10000;
    internal virtual  int    maxItems        => 1000;

    // Minimum interval to send this event
    internal virtual TimeSpan minInterval => TimeSpan.Zero;

    internal readonly AnalyticsEventData parameters;

    internal AnalyticsEvent() {
        parameters = new AnalyticsEventData();
    }

    internal AnalyticsEvent(AnalyticsEventData eventData) {
        parameters = eventData;
    }

#if UNITY_6000_3_OR_NEWER
    public bool TryGatherData(out IAnalytic.IData data, out Exception error) {
        error = null;
        data = parameters;
        return data != null;
    }
#endif
}

} //end namespace