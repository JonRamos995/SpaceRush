using UnityEngine;
using System;
using System.Collections.Generic;

namespace SpaceRush.Core
{
    public static class GameLogger
    {
        // Event for UI to subscribe to
        public static event Action<string> OnLogMessage;

        private static List<string> _logHistory = new List<string>();
        public static List<string> LogHistory => _logHistory;

        public static void Log(string message)
        {
            // Add timestamp
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = $"[{timestamp}] {message}";

            _logHistory.Add(formattedMessage);

            // Keep history manageable (e.g., last 100 messages)
            if (_logHistory.Count > 100)
            {
                _logHistory.RemoveAt(0);
            }

            // Fire event
            OnLogMessage?.Invoke(formattedMessage);

            // Also log to Unity Console for debugging
            Debug.Log(message);
        }
    }
}
