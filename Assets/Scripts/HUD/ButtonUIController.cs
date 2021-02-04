﻿using System.Collections;
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

		// Start is called before the first frame update
		void Start()
		{
			bar = obj.GetComponent<ProgressBar>();
			ready = gameObject.transform.Find("Ready").GetComponent<TMPro.TextMeshProUGUI>();
			total = gameObject.transform.Find("Total").GetComponent<TMPro.TextMeshProUGUI>();
			bar.minimum = 0.0f;
			bar.maximum = 1.0f;
			bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type); // maybe crappy dependency but whatever
			GameManager.instance.QueueStatusChanged += ChangeAmount;
			GameManager.instance.SelectionStatusChanged += SelectionUpdate;
		}

		private void SelectionUpdate(object sender, GameManager.SelectionStatusChangedEventArgs e)
		{
			if (type == e.coralType)
			{
				// update visual
			}
			else
			{
				// reset visual back to normal
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
		}

		// Update is called once per frame
		void Update()
		{
			bar.current = GameManager.instance.GetSoonestToMatureCoralPercent(type);
		}
	}
}