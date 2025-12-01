using UnityEngine;
using System;

namespace SpaceRush.Systems
{
    public class IdleManager : MonoBehaviour
    {
        private const string LAST_LOGIN_KEY = "LastLoginTime";

        private void Start()
        {
            CalculateOfflineProgress();
        }

        private void OnApplicationQuit()
        {
            SaveExitTime();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveExitTime();
            }
            else
            {
                CalculateOfflineProgress();
            }
        }

        private void SaveExitTime()
        {
            PlayerPrefs.SetString(LAST_LOGIN_KEY, DateTime.UtcNow.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        private void CalculateOfflineProgress()
        {
            if (PlayerPrefs.HasKey(LAST_LOGIN_KEY))
            {
                long temp = Convert.ToInt64(PlayerPrefs.GetString(LAST_LOGIN_KEY));
                DateTime lastLogin = DateTime.FromBinary(temp);
                TimeSpan timeAway = DateTime.UtcNow - lastLogin;

                double secondsAway = timeAway.TotalSeconds;
                Debug.Log($"Player was away for {secondsAway} seconds.");

                // Here we would trigger offline resource gain logic
                // For now, just a placeholder
                if (secondsAway > 60)
                {
                   Debug.Log("Apply offline gains here based on Fleet Stats.");
                }
            }
        }
    }
}
