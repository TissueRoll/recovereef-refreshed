using System.Collections;
using UnityEngine;

namespace Assets.Scripts.HUD
{
	public class InfoHUDTimeLeft : MonoBehaviour
	{
		[SerializeField] private GameObject timeLeft;
		private TMPro.TextMeshProUGUI timeLeftText;
		void Awake()
		{
			timeLeftText = timeLeft.GetComponent<TMPro.TextMeshProUGUI>();
		}
		// Use this for initialization
		void Start()
		{
			timeLeftText.text = Utility.ConvertTimetoMS(GameManager.instance.gameTimer.currentTime);
		}

		// Update is called once per frame
		void Update()
		{
			timeLeftText.text = Utility.ConvertTimetoMS(GameManager.instance.gameTimer.currentTime);
		}
	}
}