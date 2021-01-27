﻿using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class ShovelUIController : MonoBehaviour
	{
        [SerializeField] private GameObject obj;
        private ProgressBar bar;

        // Start is called before the first frame update
        void Start()
        {
            bar = obj.GetComponent<ProgressBar>();
            bar.minimum = 0.0f;
            bar.maximum = 1.0f;
            bar.current = GameManager.instance.shovelTimer.percentComplete; // maybe crappy dependency but whatever
            GameManager.instance.SelectionStatusChanged += SelectionUpdate;
        }

        private void SelectionUpdate(object sender, GameManager.SelectionStatusChangedEventArgs e)
        {
            if (e.coralType == 4)
            {
                // update visual
            }
            else
            {
                // reset visual back to normal
            }
        }

        // Update is called once per frame
        void Update()
        {
            bar.current = GameManager.instance.shovelTimer.percentComplete;
        }
    }
}