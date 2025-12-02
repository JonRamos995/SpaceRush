using UnityEngine;
using TMPro;
using SpaceRush.Core;
using System.Text;

namespace SpaceRush.UI
{
    public class LogWindowController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI logText;
        public UnityEngine.UI.ScrollRect scrollRect;

        private void OnEnable()
        {
            GameLogger.OnLogMessage += HandleNewLog;
            RefreshLogDisplay(); // Load history
        }

        private void OnDisable()
        {
            GameLogger.OnLogMessage -= HandleNewLog;
        }

        private void HandleNewLog(string message)
        {
            RefreshLogDisplay();

            // Auto scroll to bottom
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void RefreshLogDisplay()
        {
            if (logText == null) return;

            StringBuilder sb = new StringBuilder();
            foreach (var msg in GameLogger.LogHistory)
            {
                sb.AppendLine(msg);
            }
            logText.text = sb.ToString();
        }
    }
}
