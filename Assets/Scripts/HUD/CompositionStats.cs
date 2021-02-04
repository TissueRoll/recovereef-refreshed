using System.Collections;
using UnityEngine;

namespace Assets.Scripts.HUD
{
	public class CompositionStats : MonoBehaviour
	{
		[SerializeField] private GameObject composition;
		private TMPro.TextMeshProUGUI compositionText;

		// Use this for initialization
		void Start()
		{
			compositionText = composition.GetComponent<TMPro.TextMeshProUGUI>();
			GameManager.instance.CoralCompositionChanged += UpdateCoralComposition;
		}

		private void UpdateCoralComposition(object sender, GameManager.CoralCompositionStatusChangedEventArgs e)
		{
			int total = e.branchingTotal + e.encrustingTotal + e.massiveTotal;
			float branchingPercentage = (total > 0 ? 1f * e.branchingTotal / total : 0) * 100;
			float encrustingPercentage = (total > 0 ? 1f * e.encrustingTotal / total : 0) * 100;
			float massivePercentage = (total > 0 ? 1f * e.massiveTotal / total : 0) * 100;
			compositionText.text = string.Format(
				"Branching: {0:0.00}%\n" +
				"Encrusting: {1:0.00}%\n" +
				"Massive: {2:0.00}%",
				branchingPercentage, encrustingPercentage, massivePercentage);
		}

		private void OnDestroy()
		{
			GameManager.instance.CoralCompositionChanged -= UpdateCoralComposition;
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}