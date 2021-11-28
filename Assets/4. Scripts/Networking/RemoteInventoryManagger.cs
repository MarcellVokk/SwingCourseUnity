using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteInventoryManagger : MonoBehaviour
{
	public GameObject inHand;
	public Transform rightHandTransform, rightHandTargetTransform, leftHandTargetTransform, player, verticalRotator;

	public void PickupItem(GameObject item)
	{
		if (item.GetComponent<Item>() != null && item.GetComponent<Item>().isPickupable && inHand == null)
		{
			item.GetComponent<Item>().isRemote = true;
			item.GetComponent<Item>().isUsing = false;
			item.GetComponent<Item>().OnDrop = null;
			item.GetComponent<Item>().SetDoUpdates(true);
			item.GetComponent<Item>().isDropping = false;
			item.GetComponent<Item>().isPickupable = false;
			item.GetComponent<Item>().ownerCamera = verticalRotator;
			item.GetComponent<Item>().ownerPlayer = player;
			item.transform.SetParent(rightHandTransform);
			inHand = item;
		}
	}

	public void DroppItem(Vector3 finalPos)
	{
		if (inHand != null && inHand.GetComponent<Item>() != null && inHand.GetComponent<Item>().isPickedUp)
		{
			inHand.GetComponent<Item>().isRemote = false;
			inHand.GetComponent<Item>().isUsing = false;
			inHand.GetComponent<Item>().OnDrop?.Invoke(this, EventArgs.Empty);
			inHand.transform.position = finalPos;
			inHand.transform.rotation = player.rotation;
			inHand.GetComponent<Item>().realPos = transform.position;

			inHand.GetComponent<Item>().SetDoUpdates(false);
			inHand.GetComponent<Item>().isDropping = true;
			inHand.GetComponent<Item>().isPickupable = true;
			inHand.GetComponent<Item>().isPickedUp = false;
			inHand.GetComponent<Item>().ownerCamera = null;
			inHand.GetComponent<Item>().ownerPlayer = null;
			inHand.GetComponent<Item>().OnBeginUseItem = null;
			inHand.GetComponent<Item>().OnEndUseItem = null;
			inHand.transform.SetParent(null);
			inHand = null;
		}
	}

	private void Start()
	{
		rightHandTargetTransform.Rotate(new Vector3(1.06356335f, 340.293549f, 300.532898f));
		leftHandTargetTransform.Rotate(new Vector3(2.58351707f, 10.6171379f, 59.5108986f));
	}

	private void Update()
	{
		if (inHand != null && !inHand.GetComponent<Item>().isPickedUp)
		{
			inHand.transform.localPosition = Vector3.MoveTowards(inHand.transform.localPosition, new Vector3(0f, 0f, 0f), 0.25f);

			if (Vector3.Distance(inHand.transform.localPosition, new Vector3(0, 0, 0)) < 0.1f)
			{
				inHand.transform.localPosition = new Vector3(0, 0, 0);
				inHand.transform.localScale = new Vector3(1, 1, 1);
				inHand.GetComponent<Item>().isPickedUp = true;
			}
		}
	}
}
