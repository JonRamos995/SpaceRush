using UnityEngine;
using SpaceRush.Core;
using System;

namespace SpaceRush.Systems
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ScheduleNotification(string title, string body, float delaySeconds)
        {
            GameLogger.Log($"[NOTIFICATION SCHEDULED] '{title}': {body} in {delaySeconds}s");
            // Integration with Android Notification Channel would go here.
        }
    }
}
