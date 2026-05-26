using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Analytics;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity.SharpZipLib.Editor {

internal static class AnalyticsSender {

#if UNITY_2019_4_OR_NEWER && UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
    private struct EventDetail {
        public string assemblyInfo;
        public string packageName;
        public string packageVersion;
    }

    internal static void SendEventInEditor(AnalyticsEvent analyticsEvent) {
        if (!EditorAnalytics.enabled) {
            return;
        }

#if !UNITY_6000_3_OR_NEWER
        //registration is done automatically in the 6000.3 version
        if (!IsEventRegistered(analyticsEvent)) {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (!RegisterEvent(analyticsEvent, assembly)) {
                return;
            }
        }
#endif

        if (!ShouldSendEvent(analyticsEvent)) {
            return;
        }

#if UNITY_6000_3_OR_NEWER
        if (string.IsNullOrEmpty(m_packageVersion)) {
            Assembly assembly = Assembly.GetCallingAssembly();
            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
            m_packageVersion = packageInfo != null ? packageInfo.version : string.Empty;
        }

        analyticsEvent.parameters.actualPackageVersion = m_packageVersion;
        EditorAnalytics.SendAnalytic(analyticsEvent);
#else
        analyticsEvent.parameters.actualPackageVersion = m_registeredEvents[analyticsEvent.eventName].packageVersion;
        AnalyticsResult result = EditorAnalytics.SendEventWithLimit(analyticsEvent.eventName, analyticsEvent.parameters, analyticsEvent.version);
        if (result != AnalyticsResult.Ok) {
            return;
        }
#endif

        DateTime now = DateTime.Now;
        m_lastSentDateTime[analyticsEvent.eventName] = now;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static bool ShouldSendEvent(AnalyticsEvent analyticsEvent) {
        if (!m_lastSentDateTime.ContainsKey(analyticsEvent.eventName)) {
            return true;
        }

        DateTime lastSentDateTime = m_lastSentDateTime[analyticsEvent.eventName];
        return DateTime.Now - lastSentDateTime >= analyticsEvent.minInterval;
    }

#if !UNITY_6000_3_OR_NEWER
    private static bool IsEventRegistered(AnalyticsEvent analyticsEvent) {
        return m_registeredEvents.ContainsKey(analyticsEvent.eventName);
    }
    private static bool RegisterEvent(AnalyticsEvent analyticsEvent, Assembly assembly) {
        if (!EditorAnalytics.enabled) {
            return false;
        }

        // In pre-UNITY_6000_3_OR_NEWER versions, registration must be performed explicitly here.
        // Automatic registration applies only to UNITY_6000_3_OR_NEWER.
        AnalyticsResult result = EditorAnalytics.RegisterEventWithLimit(analyticsEvent.eventName,
            analyticsEvent.maxEventPerHour, analyticsEvent.maxItems, SharpZipLibEditorConstants.VENDOR_KEY, analyticsEvent.version);

        if (result != AnalyticsResult.Ok) {
            return false;
        }

        EventDetail eventDetails = new EventDetail {
            assemblyInfo = assembly.FullName,
        };

        UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
        if (packageInfo != null) {
            eventDetails.packageName = packageInfo.name;
            eventDetails.packageVersion = packageInfo.version;
        }

        m_registeredEvents[analyticsEvent.eventName] = eventDetails;
        return true;
    }

#endif //#if !UNITY_6000_3_OR_NEWER

//--------------------------------------------------------------------------------------------------------------------------------------------------------------


    private static readonly Dictionary<string, EventDetail> m_registeredEvents = new Dictionary<string, EventDetail>();
    private static readonly Dictionary<string, DateTime>    m_lastSentDateTime = new Dictionary<string, DateTime>();

    private static string m_packageVersion = null;


#else

    internal static void SendEventInEditor(AnalyticsEvent analyticsEvent) { }


#endif //UNITY_EDITOR

}
} //end namespace