using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.HUD
{
	public class ButtonUIController : MonoBehaviour
	{
		public int type;
		[SerializeField] private GameObject obj;
		private ProgressBar bar;
		private TMPro.TextMeshProUGUI ready;
		private TMPro.TextMeshProUGUI total;
		private UnityEngine.UI.Image image;
		private bool pulsing = false;
		private float max_pulse_val = 1f;
		private float min_pulse_val = 0.5f;
		private float t = 1f;

		// Start is called before the first frame update
		void Start()
		{
			bar = obj.GetComponent<ProgressBar>();
			ready = gameObject.transform.Find("Ready").GetComponent<TMPro.TextMeshProUGUI>();
			total = gameObject.transform.Find("Total").GetComponent<TMPro.TextMeshProUGUI>();
			image = gameObject.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
			bar.minimum = 0.0f;
			bar.maximum = 1.0f;
			bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type); // maybe crappy dependency but whatever
			GameManager.instance.QueueStatusChanged += ChangeAmount;
			GameManager.instance.SelectionStatusChanged += SelectionUpdate;
			pulsing = false;
		}

		private void SelectionUpdate(object sender, GameManager.SelectionStatusChangedEventArgs e)
		{
			if (type == e.coralType)
			{
				// update visual
				pulsing = true;
			}
			else
			{
				// reset visual back to normal
				pulsing = false;
			}
		}

		private void ChangeAmount(object sender, GameManager.QueueStatusChangedEventArgs e)
		{
			if (type == e.coralType)
			{
				ready.text = e.coralReady.ToString();
				total.text = e.coralTotal.ToString();
			}
		}

		private void OnDestroy()
		{
			GameManager.instance.QueueStatusChanged -= ChangeAmount;
			GameManager.instance.SelectionStatusChanged -= SelectionUpdate;
		}

		// Update is called once per frame
		void Update()
		{
			bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type);
			Color color = image.color;
			if (pulsing)
			{
				float alpha = Mathf.Lerp(min_pulse_val, max_pulse_val, t);
				color.a = alpha;

				t += 3f * Time.deltaTime;
				if (t > 1.0f)
				{
					float tmp = max_pulse_val;
					max_pulse_val = min_pulse_val;
					min_pulse_val = tmp;
					t = 0.0f;
				}
			}
			else
			{
				color.a = 1f;
			}
			image.color = color;
		}
	}
}