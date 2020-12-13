using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
	public class FieldControlHandler : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		// can extend this if you want certain UI to be ignored
		public void FieldControl()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				GameManager.instance.GrowCoral(0);
				GameManager.instance.ChangeCoral(0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				GameManager.instance.GrowCoral(1);
				GameManager.instance.ChangeCoral(1);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				GameManager.instance.GrowCoral(2);
				GameManager.instance.ChangeCoral(2);
			}

			if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Z))
			{
				GameManager.instance.ChangeCoral(0);
			}
			else if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.X))
			{
				GameManager.instance.ChangeCoral(1);
			}
			else if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.C))
			{
				GameManager.instance.ChangeCoral(2);
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				GameManager.instance.SelectShovel();
			}

			if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
			{
				if (GameManager.instance.shovelChosen)
				{
					GameManager.instance.ShovelArea();
				}
				else
				{
					GameManager.instance.PlantCoral(GameManager.instance.selectedCoral);
				}
			}
		}

		private bool IsMouseOverUI()
		{
			return EventSystem.current.IsPointerOverGameObject();
		}
	}
}