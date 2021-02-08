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
				default:
					Debug.Log("Unknown Level");
					break;
			}
		}
	}
}