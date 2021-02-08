using System.Collections;
using UnityEngine;

namespace Assets.Scripts.HUD
{
	public class ShovelUIController : MonoBehaviour
	{
		[SerializeField] private GameObject obj;
		[SerializeField] private GameObject textObj;
		private ProgressBar bar;
		private TMPro.TextMeshProUGUI text;
		private bool pulsing = false;
		private float max_pulse_val = 1f;
		private float min_pulse_val = 0.5f;
		private float t = 1f;

		// Start is called before the first frame update
		void Start()
		{
			bar = obj.GetComponent<ProgressBar>();
			text = textObj.GetComponent<TMPro.TextMeshProUGUI>();
			bar.minimum = 0.0f;
			bar.maximum = 1.0f;
			bar.current = GameManager.instance.shovelTimer.percentComplete; // maybe crappy dependency but whatever
			GameManager.instance.SelectionStatusChanged += SelectionUpdate;
			pulsing = false;
		}

		private void OnDestroy()
		{
			GameManager.instance.SelectionStatusChanged -= SelectionUpdate;
		}

		private void SelectionUpdate(object sender, GameManager.SelectionStatusChangedEventArgs e)
		{
			if (e.coralType == 3)
			{
				pulsing = true;
			}
			else
			{
				pulsing = false;
			}
		}

		// Update is called once per frame
		void Update()
		{
			bar.current = GameManager.instance.shovelTimer.percentComplete;
			Color color = text.color;
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
			text.color = color;
		}
	}
}