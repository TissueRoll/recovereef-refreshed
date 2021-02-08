using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	public void getLevel(int level)
	{
		switch (level)
		{
			case 1:
				SceneManager.LoadScene("01_00_Game_Easy");
				break;
			case 2:
				SceneManager.LoadScene("01_01_Game_Medium");
				break;
			case 3:
				SceneManager.LoadScene("01_02_Game_Hard");
				break;
			case 4:
				SceneManager.LoadScene("00_00_Game_Tutorial");
				break;
			case 5:
				SceneManager.LoadScene("00_01_Game_Tutorial");
				break;
			case 6:
				SceneManager.LoadScene("00_02_Game_Tutorial");
				break;
			case 7:
				SceneManager.LoadScene("00_03_Game_Tutorial");
				break;
			case 8:
				SceneManager.LoadScene("00_04_Game_Tutorial");
				break;
			default:
				Debug.Log("Unknown Level. Exiting");
				getExit();
				break;
		}
	}
	public void getNewGame()
	{
		// build index method
		// SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		SceneManager.LoadScene("Test");
	}

	public void getTrained()
	{
		SceneManager.LoadScene("TestCopy");
	}

	public void getExit()
	{
		print("Game has been quit.");
		Application.Quit();
	}
}
