using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour
{
	public static bool gameHasEnded = false;
	
	public GameObject gameEndCanvas;
	public GameObject resultGreeting;
	public GameObject statText;
	public GameObject flavorText;

	private UnityEngine.UI.Image resultGreetingImage;
	private TMPro.TextMeshProUGUI statTextContent;
	private TMPro.TextMeshProUGUI flavorTextContent;

	private void Awake()
	{
		resultGreetingImage = resultGreeting.GetComponent<UnityEngine.UI.Image>();
		statTextContent = statText.GetComponent<TMPro.TextMeshProUGUI>();
		flavorTextContent = flavorText.GetComponent<TMPro.TextMeshProUGUI>();
	}

	public void GameEndReached()
	{
		gameHasEnded = true;
		gameEndCanvas.SetActive(true);
		Time.timeScale = 0f;
	}

	public void SetCongrats(Sprite wordArt)
	{
		resultGreetingImage.sprite = wordArt;
	}

	public void FinalStatistics(int fishIncome, string timeLeft)
	{
		statTextContent.text = "Fish Income: " + fishIncome + "\nTime Left: " + timeLeft;
	}

	public void EndMessage(string s)
	{
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
