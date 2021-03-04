using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.MainMenu
{
	public class LevelSelector : MonoBehaviour
	{

		public void SelectLevel(int level)
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
					SceneManager.LoadScene("00_00_Tutorial_Controls");
					break;
				case 5:
					SceneManager.LoadScene("00_01_Tutorial_Corals_Basics");
					break;
				case 6:
					SceneManager.LoadScene("00_02_Tutorial_Substrata_Algae");
					break;
				case 7:
					SceneManager.LoadScene("00_03_Tutorial_Events");
					break;
				case 8:
					SceneManager.LoadScene("00_04_Tutorial_Corals_Advanced");
					break;
				default:
					Debug.Log("Unknown Level");
					break;
			}
		}
	}
}