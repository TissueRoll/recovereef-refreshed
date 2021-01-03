using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class ClimateChangeTimeUI : MonoBehaviour
	{
		[SerializeField] private GameObject parentObj;
		[SerializeField] private GameObject obj;
		private ProgressBar progress;
		private UnityEngine.UI.Image image;
		private void Awake()
		{
			progress = obj.GetComponent<ProgressBar>();
			image = parentObj.GetComponent<UnityEngine.UI.Image>();
		}
		// Use this for initialization
		void Start()
		{
			progress.minimum = 0.0f;
			progress.maximum = 1.0f;
			progress.current = GameManager.instance.GetClimateChangeProgress();
			obj.SetActive(false);
			image.enabled = false;
		}

		// Update is called once per frame
		void Update()
		{
			if (GameManager.instance.climateChangeHasWarned)
			{
				if (!obj.activeSelf)
				{
					image.enabled = true;
					obj.SetActive(true);
				}
				progress.current = GameManager.instance.GetClimateChangeProgress();
			}
		}
	}
}