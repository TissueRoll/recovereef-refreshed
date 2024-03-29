using UnityEngine;
using System.Collections;

public class ItemLoader : MonoBehaviour
{
	public const string path = "items";

	void Start()
	{
		ItemContainer ic = ItemContainer.Load(path);
		foreach (Item item in ic.items)
		{
			print(item.rekt());
		}
	}
}
