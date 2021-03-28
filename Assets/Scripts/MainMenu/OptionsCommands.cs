using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MainMenu
{
	public class OptionsCommands : MonoBehaviour
	{
		[SerializeField] private Slider musicSlider;
		[SerializeField] private Slider sfxSlider;

		private void OnEnable()
		{
			SetSliderValues();
		}

		public void ApplyChanges()
		{
			SaveSliderValues();
			AudioManager.instance.AdjustMusicVolume();
			AudioManager.instance.AdjustSFXVolume();
		}

		public void SetSliderValues()
		{
			musicSlider.value = AudioManager.instance.musicVolume * 10;
			sfxSlider.value = AudioManager.instance.sfxVolume * 10;
		}

		public void SaveSliderValues()
		{
			
			AudioManager.instance.musicVolume = musicSlider.value / 10f;
			AudioManager.instance.sfxVolume = sfxSlider.value / 10f;
		}

	}
}