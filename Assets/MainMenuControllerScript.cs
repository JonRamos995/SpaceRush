using UnityEngine;
using UnityEngine.Analytics;
using System.Collections;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class MainMenuControllerScript : MonoBehaviour {


	public void StartGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene ("MainGame");
	}

    public void Achievements()
    {
        if (Social.localUser.authenticated)
            Social.ShowAchievementsUI();
    }

    public void Leaderboards()
    {
      //  if (Social.localUser.authenticated)
      //      Social.ShowLeaderboardUI();

        PlayGamesPlatform.Instance.ShowLeaderboardUI("CgkItd3rhuEFEAIQAQ");

    }

	public void Rate()
	{
		Application.OpenURL("market://details?id=com.JRSGames.SpaceRun");
	}

    public void SignOut()
    {
        PlayGamesPlatform.Instance.SignOut();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate((bool success) =>
            {
                
            });
    }



}
