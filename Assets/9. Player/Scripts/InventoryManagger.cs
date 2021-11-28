using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManagger : MonoBehaviour
{
    public Transform rightHandItem, camera, player, orientation;
	public float pickupRange = 4f;

	public GameObject inHand;

	public void PickUp(GameObject item)
	{
		if (item.GetComponent<Item>() != null && item.GetComponent<Item>().isPickupable && inHand == null)
		{
			ClientSend.PickupItem(item.GetComponent<Item>().uuid);
			item.GetComponent<Item>().isUsing = false;
			item.GetComponent<Item>().OnDrop = null;
			item.GetComponent<Item>().SetDoUpdates(true);
			item.GetComponent<Item>().isDropping = false;
			item.GetComponent<Item>().isPickupable = false;
			item.GetComponent<Item>().ownerCamera = camera;
			item.GetComponent<Item>().ownerPlayer = player;
			item.transform.SetParent(rightHandItem);
			inHand = item;
		}
	}

	public void Drop(GameObject item, bool isSilent)
	{
		if (item != null && item.GetComponent<Item>() != null && item.GetComponent<Item>().isPickedUp)
		{
			if (!isSilent)
			{
				ClientSend.DroppItem(transform.position + (orientation.rotation * new Vector3(0, 1f, 2f)));
			}

			item.GetComponent<Item>().isUsing = false;
			item.GetComponent<Item>().OnDrop?.Invoke(this, EventArgs.Empty);
			item.transform.position = transform.position + (orientation.rotation * new Vector3(0, 1f, 2f));
			item.GetComponent<Item>().SetDesiredRotation(Quaternion.identity);
			item.transform.rotation = orientation.rotation;
			item.GetComponent<Item>().realPos = transform.position;

			item.GetComponent<Item>().SetDoUpdates(false);
			item.GetComponent<Item>().isDropping = true;
			item.GetComponent<Item>().isPickupable = true;
			item.GetComponent<Item>().isPickedUp = false;
			item.GetComponent<Item>().ownerCamera = null;
			item.GetComponent<Item>().ownerPlayer = null;
			item.GetComponent<Item>().OnBeginUseItem = null;
			item.GetComponent<Item>().OnEndUseItem = null;
			item.transform.SetParent(null);
			inHand = null;
		}
	}

	private void Update()
	{
		if(UIManager.blockUserInput != true)
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Drop(inHand, false);
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				RaycastHit hit;
				if (Physics.Raycast(camera.position, camera.forward, out hit, pickupRange))
				{
					PickUp(hit.transform.gameObject);
				}
			}
		}

		if (inHand != null && !inHand.GetComponent<Item>().isPickedUp)
		{
			inHand.transform.localPosition = Vector3.MoveTowards(inHand.transform.localPosition, new Vector3(0f, 0f, 0f), 15f * Time.deltaTime);

			if (Vector3.Distance(inHand.transform.localPosition, new Vector3(0, 0, 0)) < 0.1f)
			{
				inHand.transform.localPosition = new Vector3(0, 0, 0);
				inHand.transform.localScale = new Vector3(1, 1, 1);
				inHand.GetComponent<Item>().isPickedUp = true;
			}
		}
	}
}
