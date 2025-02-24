using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;

namespace SharedComponents.Events
{
    public static class DirectEventHandler
    {
        public delegate void directEventHandler(string charName, string GUID, DirectEvent directEvent);

        public static ConcurrentDictionary<string, ConcurrentDictionary<DirectEvents, DateTime?>> lastEventByType;
        public static ConcurrentDictionary<string, ConcurrentDictionary<DirectEvents, DateTime?>> rateLimitStorage;

        static DirectEventHandler()
        {
            lastEventByType = new ConcurrentDictionary<string, ConcurrentDictionary<DirectEvents, DateTime?>>();
            rateLimitStorage = new ConcurrentDictionary<string, ConcurrentDictionary<DirectEvents, DateTime?>>();
        }

        public static event directEventHandler OnDirectEvent;

        public static DateTime? GetLastEventReceived(string GUID, DirectEvents directevent)
        {
            try
            {
                if (lastEventByType.ContainsKey(GUID) && lastEventByType[GUID].ContainsKey(directevent))
                    return lastEventByType[GUID][directevent];
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        private static void SetLastEventReceived(ConcurrentDictionary<string, ConcurrentDictionary<DirectEvents, DateTime?>> dict, string GUID,
            DirectEvents directEvent)
        {
            try
            {
                if (!dict.ContainsKey(GUID))
                    dict[GUID] = new ConcurrentDictionary<DirectEvents, DateTime?>();

                dict[GUID][directEvent] = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void ClearEvents(string GUID)
        {
            try
            {
                lastEventByType.TryRemove(GUID, out _);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private static bool CheckRateLimit(string GUID, DirectEvents directEvent)
        {
            try
            {
                ConcurrentDictionary<DirectEvents, DateTime?> lastEventDict;
                rateLimitStorage.TryGetValue(GUID, out lastEventDict);

                if (lastEventDict != null && lastEventDict.ContainsKey(directEvent))
                {
                    DateTime? lastEventTime;
                    lastEventDict.TryGetValue(directEvent, out lastEventTime);
                    if (lastEventTime != null &&
                        lastEventTime.HasValue &&
                        lastEventTime.Value.AddSeconds(15) > DateTime.UtcNow)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public static void OnNewDirectEvent(string charName, string GUID, DirectEvent directEvent)
        {
            try
            {
                SetLastEventReceived(lastEventByType, GUID, directEvent.type);

                switch (directEvent.type)
                {
                    case DirectEvents.ACCEPT_MISSION:
                        break;

                    case DirectEvents.COMPLETE_MISSION:
                        break;

                    case DirectEvents.DECLINE_MISSION:
                        break;

                    case DirectEvents.DOCK_JUMP_ACTIVATE:
                        break;

                    case DirectEvents.LOCK_TARGET:
                        break;

                    case DirectEvents.UNDOCK:
                        break;

                    case DirectEvents.WARP:
                        break;

                    case DirectEvents.PANIC:
                        break;

                    case DirectEvents.CALLED_LOCALCHAT:
                    case DirectEvents.LOCKED_BY_PLAYER:
                    case DirectEvents.MISSION_INVADED:
                    case DirectEvents.PRIVATE_CHAT_RECEIVED:
                    case DirectEvents.CAPSULE:
                    case DirectEvents.ERROR:
                    case DirectEvents.NOTICE:

                        directEvent.color = Color.Red;
                        directEvent.warning = true;
                        break;
                }

                if (OnDirectEvent != null && CheckRateLimit(GUID, directEvent.type))
                {
                    OnDirectEvent(charName, GUID, directEvent);
                    SetLastEventReceived(rateLimitStorage, GUID, directEvent.type);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}