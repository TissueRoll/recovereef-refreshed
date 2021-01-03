using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class FeedbackTextScript : MonoBehaviour
	{
		public GameObject bar;
		public GameObject barText;
		private TMPro.TextMeshProUGUI textObj;

		private void Awake()
		{
			textObj = barText.GetComponent<TMPro.TextMeshProUGUI>();
		}
		// Use this for initialization
		void Start()
		{
			textObj.text = "";
			GameManager.instance.FeedbackTextStatus += FeedbackDialogueWrapper;
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void OnDestroy()
		{
			GameManager.instance.FeedbackTextStatus -= FeedbackDialogueWrapper;
		}

		private void FeedbackDialogueWrapper(object sender, GameManager.FeedbackTextStatusEventArgs e)
		{
			FeedbackDialogue(e.feedbackTextContent, e.feedbackTextDuration);
		}

		private void FeedbackDialogue(string text, float time) => StartCoroutine(Co_ShowMessage(text, time));

		IEnumerator Co_ShowMessage(string text, float time)
		{
			textObj.text = text;
			barText.SetActive(true);
			Debug.Log(text);
			yield return new WaitForSeconds(time);
			barText.SetActive(false);
		}
	}
}