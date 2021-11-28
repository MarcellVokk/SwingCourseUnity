using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManagger : MonoBehaviour
{
	[SerializeField]
	private GameObject missingItem;

	public static GameObject missingItemPrefab;

	[Serializable]
	public struct SerializedItem
	{
		public string itemName;
		public GameObject itemPrefab;
	}

	[SerializeField]
	private SerializedItem[] itemList;

	public static Dictionary<string, GameObject> itemDictionary;

	private void Start()
	{
		itemDictionary = itemList.ToDictionary(x => x.itemName, x => x.itemPrefab);
		missingItemPrefab = missingItem;
	}

	public static void KillItem(string itemIndex)
	{
		if (GameManager.items[itemIndex].isPickedUp)
		{
			if (GameManager.items[itemIndex].isRemote)
			{
				GameManager.items[itemIndex].ownerPlayer.GetComponentInParent<RemoteInventoryManagger>().DroppItem(GameManager.items[itemIndex].ownerPlayer.transform.position);
			}
			else
			{
				GameManager.items[itemIndex].ownerPlayer.GetComponentInChildren<InventoryManagger>().Drop(GameManager.items[itemIndex].ownerPlayer.GetComponentInChildren<InventoryManagger>().inHand, true);
			}
		}

		GameObject.Destroy(GameManager.items[itemIndex].gameObject);
		GameManager.items.Remove(itemIndex);
	}

	public static GameObject GetItemPrefab(string itemId)
	{
		if (itemDictionary.ContainsKey(itemId))
		{
			return itemDictionary[itemId];
		}
		else
		{
			return missingItemPrefab;
		}
	}
}
