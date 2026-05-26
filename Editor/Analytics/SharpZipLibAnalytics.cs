using UnityEditor;
#if UNITY_6000_3_OR_NEWER
using UnityEngine.Analytics;
#endif

namespace Unity.SharpZipLib.Editor {
internal static class SharpZipLibAnalytics {
#if UNITY_6000_3_OR_NEWER
    [AnalyticInfo(eventName: EVENT_NAME, vendorKey: SharpZipLibEditorConstants.VENDOR_KEY, maxEventsPerHour: 1, maxNumberOfElements: 2, version: 1)]
#endif
    private class LoadEvent : AnalyticsEvent {
        internal override string eventName       => EVENT_NAME;
        internal override int    maxEventPerHour => 1;
        internal override int    maxItems        => 2;
        internal LoadEvent() : base(new AnalyticsEventData()) {
        }
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    [InitializeOnLoadMethod]
    private static void OnLoad() {
        AnalyticsSender.SendEventInEditor(new LoadEvent());
    }

    private const string EVENT_NAME = "sharpziplib_load";
}

} //end namespace