﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
	public static bool PopupOpen = false;

	public GameObject popupCanvas;
	public GameObject outText;
	public UnityEngine.UI.Image popupSprite;
	public UnityEngine.UI.Button popupButton;

	void Update()
	{
		if (!GameEnd.gameHasEnded && !PauseScript.GamePaused && Input.GetKeyDown(KeyCode.Return))
		{
			ClosePopup();
		}
	}

	public void SetPopupSprite(Sprite sprite)
	{
		popupSprite.sprite = sprite;
	}

	public void SetPopupMessage(string e)
	{
		outText.GetComponent<TMPro.TextMeshProUGUI>().text = e;
	}

	public void makeEvent(int type, string forced = "")
	{
		popupButton.GetComponent<UnityEngine.UI.Image>().sprite = GameAssets.instance.normalButtonSprite;
		if (forced.Length > 0)
		{
			SetPopupSprite(GameAssets.instance.climateChangeDisasterSprite);
			SetPopupMessage(forced);
			popupButton.GetComponent<UnityEngine.UI.Image>().sprite = GameAssets.instance.climateButtonSprite;
		}
		else if (type == 1)
		{ // toxic waste
			SetPopupSprite(GameAssets.instance.toxicWasteDisasterSprite);
			SetPopupMessage("An irresponsible company just dumped a barrel of toxic waste into your coral reef! Part of your reef has now become permanently affected.");
		}
		else if (type == 2)
		{ // tourists
			SetPopupSprite(GameAssets.instance.touristsDisasterSprite);
			SetPopupMessage("A group of tourists came and vandalized part of your reef, not knowing corals are animals. Part of your coral population has died.");
		}
		else if (type == 3)
		{ // bombing
			SetPopupSprite(GameAssets.instance.bombingDisasterSprite);
			SetPopupMessage("Fishermen have just thrown bombs into your reef in the hopes of catching some fish! Some of your corals have died.");
		}
		else
		{ // UNKNOWN EVENT
			SetPopupSprite(GameAssets.instance.defaultPopupSprite);
			SetPopupMessage("An event has happened.");
		}
		OpenPopup();
	}

	public void OpenPopup()
	{
		popupCanvas.SetActive(true);
		Time.timeScale = 0f;
		PopupOpen = true;
	}

	public void ClosePopup()
	{
		popupCanvas.SetActive(false);
		Time.timeScale = 1f;
		PopupOpen = false;
	}
}
