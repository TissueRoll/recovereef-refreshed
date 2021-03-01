using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class TutorialManager : MonoBehaviour
	{

		[SerializeField] private GameObject[] popUps;
		public int popupIndex = 0;

		// Use this for initialization
		void Start()
		{
			popupIndex = 0;
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
		}

		public void CloseTutorial()
		{
			popupIndex = popUps.Length + 1;
			GameManager.instance.ResumeTime();
		}
	}
}