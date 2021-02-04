using System.Collections;
using UnityEngine;

namespace Assets.Scripts.HUD
{
	public class InfoHUDFishDisplay : MonoBehaviour
	{
		[SerializeField] private GameObject fishDisplay;
		[SerializeField] private GameObject fishImage;
		private TMPro.TextMeshProUGUI fishDisplayText;
		private UnityEngine.UI.Image fishImageImage;

		private void Awake()
		{
			fishDisplayText = fishDisplay.GetComponent<TMPro.TextMeshProUGUI>();
			fishImageImage = fishImage.GetComponent<UnityEngine.UI.Image>();
			// UpdateFishData();
		}

		// Use this for initialization
		private void Start()
		{
			InvokeRepeating(nameof(UpdateFishData), 0f, 1.0f);
		}

		// Update is called once per frame
		private void Update()
		{

		}

		private void UpdateFishData()
		{
			if (GameEnd.gameHasEnded || PauseScript.GamePaused)
				return;

			fishDisplayText.text = "Fish Income: " + GameManager.instance.fishIncome;
			if (GameManager.instance.FishProsperityState() == 1)
			{
				fishImageImage.color = Utility.green;
			}
			else
			{
				fishImageImage.color = Utility.red;
			}
		}

	}
}