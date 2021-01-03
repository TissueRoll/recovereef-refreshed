using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour
{
	public static bool gameHasEnded = false;
	
	[SerializeField] private GameObject gameEndCanvas;
	[SerializeField] private GameObject resultGreeting;
	[SerializeField] private GameObject statText;
	[SerializeField] private GameObject flavorText;

	private UnityEngine.UI.Image resultGreetingImage;
	private TMPro.TextMeshProUGUI statTextContent;
	private TMPro.TextMeshProUGUI flavorTextContent;

	public void GameEndReached()
	{
		gameHasEnded = true;
		gameEndCanvas.SetActive(true);
		Time.timeScale = 0f;
	}
	// i have no idea why i have to do it this way
	public void SetCongrats(Sprite wordArt)
	{
		if (resultGreetingImage == null)
			resultGreetingImage = resultGreeting.GetComponent<UnityEngine.UI.Image>();
		resultGreetingImage.sprite = wordArt;
	}

	public void FinalStatistics(int fishIncome, string timeLeft)
	{
		if (statTextContent == null)
			statTextContent = statText.GetComponent<TMPro.TextMeshProUGUI>();
		statTextContent.text = "Fish Income: " + fishIncome + "\nTime Left: " + timeLeft;
	}

	public void EndMessage(string s)
	{
		if (flavorTextContent == null)
			flavorTextContent = flavorText.GetComponent<TMPro.TextMeshProUGUI>();
		flavorTextContent.text = s;
	}

	public void ResetEndScreen()
	{
		gameHasEnded = false;
		gameEndCanvas.SetActive(false);
		Time.timeScale = 1f;
	}

	public void GoToMainMenu()
	{
		ResetEndScreen();
		SceneManager.LoadScene("MainMenu");
	}

	public void RestartGame()
	{
		ResetEndScreen();
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}

	public void QuitGame()
	{
		print("game has been quit");
		Application.Quit();
	}
}
