using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [HideInInspector]public string uuid;
	[HideInInspector]public int playerInCharge = 0;
	[HideInInspector]public bool remote = true;

	public event Action onPutInCharge;
	public event Action onLostInCharge;

	[HideInInspector]public Vector3 targetPos;
	[HideInInspector]public Quaternion targetRot;

	[HideInInspector]public int childPlayer;

	public Transform mountPosition;

    void Update()
    {
        if(playerInCharge == Client.instance.myId)
		{
			if (remote)
			{
				remote = false;
				onPutInCharge?.Invoke();
			}

			ClientSend.SendEntityTransform(uuid, transform.position, transform.rotation);
		}
		else if(!remote)
		{
			remote = true;
			onLostInCharge?.Invoke();
		}

        if (remote)
        {
			float maxM = (Vector3.Distance(targetPos, transform.position) / 3f) * Time.deltaTime;

			transform.position = Vector3.MoveTowards(transform.position, targetPos, maxM * 40f);

			if (Vector3.Distance(targetPos, transform.position) > 100f)
			{
				transform.position = targetPos;
			}

			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 400f * Time.deltaTime);
		}
    }

	public void PutInChargeEvent()
    {
		onPutInCharge?.Invoke();
    }

	public void LostInChargeEvent()
	{
		onLostInCharge?.Invoke();
	}

	public bool PutSelfInCharge()
    {
		if(remote && playerInCharge == 0)
        {
			EnvironmentManagger.ControlEntityPhysics(uuid);
			return true;
		}

		return false;
	}

	public void RemoveSelfFromCharge()
	{
        if (!remote)
        {
			EnvironmentManagger.AbandonControlPhysics(uuid);
		}
	}

	public void AttachPlayer()
    {
		GameManager.players[Client.instance.myId].transform.parent = transform;
		GameManager.instance.localPlayer.attachedEntity = uuid;
		ClientSend.ParentEntityToPlayer(Client.instance.myId, uuid, true);
	}

	public void DeattachPlayer()
	{
		GameManager.players[Client.instance.myId].transform.parent = null;
		GameManager.instance.localPlayer.attachedEntity = "";
		ClientSend.ParentEntityToPlayer(Client.instance.myId, uuid, false);
	}

	public void MountPlayer()
    {
		GameManager.instance.localPlayer.gameObject.SetActive(false);

		PlayerController.isMounted = true;
		AttachPlayer();

		if(mountPosition != null)
        {
			GameManager.instance.localPlayer.transform.localPosition = mountPosition.localPosition;
			GameManager.instance.localPlayer.transform.localRotation = mountPosition.localRotation;
		}
        else
        {
			GameManager.instance.localPlayer.transform.localPosition = Vector3.zero;
			GameManager.instance.localPlayer.transform.localRotation = Quaternion.identity;
		}

		ClientSend.PlayerMovement(GameManager.instance.localPlayer.transform.position, GameManager.instance.localPlayer.transform.rotation, GameManager.instance.localPlayer.playerController.verticalOrientation.rotation, true, true);
	}

	public void DismountPlayer()
	{
		GameManager.instance.localPlayer.gameObject.SetActive(true);
		GameManager.instance.localPlayer.transform.rotation = Quaternion.identity;

		DeattachPlayer();
		PlayerController.isMounted = false;

		ClientSend.PlayerMovement(GameManager.instance.localPlayer.transform.position, GameManager.instance.localPlayer.transform.rotation, GameManager.instance.localPlayer.playerController.verticalOrientation.rotation, true, false);
	}
}
