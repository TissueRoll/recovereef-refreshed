using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class TutorialManager : MonoBehaviour
	{

		[SerializeField] private GameObject[] popUps;
		[SerializeField] private GameObject pageNumber;
		private TMPro.TextMeshProUGUI pageNumberText;
		public int popupIndex = 0;

		// Use this for initialization
		void Start()
		{
			pageNumberText = pageNumber.GetComponent<TMPro.TextMeshProUGUI>();
			popupIndex = 0;
			pageNumberText.text = $"{popupIndex + 1} of {popUps.Length}";
			GameManager.instance.StopTime();
			// any event subscription
		}

		private void OnDestroy()
		{
			// any event unsubscription
		}

		// Update is called once per frame
		void Update()
		{
			for (int i = 0; i < popUps.Length; ++i)
			{
				if (i == popupIndex)
				{
					popUps[i].SetActive(true);
				} 
				else
				{
					popUps[i].SetActive(false);
				}
			}
		}

		public void AdvanceTutorial()
		{
			if (popupIndex == popUps.Length - 1)
			{
				// do nothing
			}
			else
			{
				popupIndex++;
			}
			pageNumberText.text = $"{popupIndex + 1} of {popUps.Length}";
		}
		public void RewindTutorial()
		{
			if (popupIndex == 0)
			{
				// do nothing
			} 
			else
			{
				popupIndex--;
			}
			pageNumberText.text = $"{popupIndex + 1} of {popUps.Length}";
		}

		public void CloseTutorial()
		{
			popupIndex = popUps.Length + 1;
			GameManager.instance.ResumeTime();
		}
	}
}