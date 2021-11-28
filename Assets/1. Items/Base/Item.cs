using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SyncronizedVariable
{
	public string name = "";
	public object value = null;
	public string type = "";

	public void Set(object _value, string _type = "", string _name = "")
	{
		value = _value;

		if(_type != "")
		{
			type = _type;
		}

		if(_name != "")
		{
			name = _name;
		}
	}

	public object Get()
	{
		return value;
	}

	public SyncronizedVariable(object _value, string _type, string _name)
	{
		value = _value;
		type = _type;
		name = _name;
	}
}

public class Item : MonoBehaviour
{
	[Header("Item settings")]
	public string itemName;
	public rarity itemRarity = rarity.uncommon;
	public float itemUseDelay = 1f;
	public Transform itemCenterTransform;

	[Header("Environment settings")]
	public LayerMask groundLayer;
	public float itemDistanceFromGround = 1f;

	private bool isUsingDown = false;
	private Quaternion desiredRotation;
	private Quaternion lastRot;
	private GameObject rarityVFXGameObject;

	[HideInInspector] public Vector3 landingPos;
	[HideInInspector] public Vector3 realPos;

	[HideInInspector] public string uuid;

	[HideInInspector]public bool isPickupable = true;
	[HideInInspector]public bool isPickedUp = false;
	[HideInInspector]public bool isDropping = false;
	[HideInInspector]public bool isUsing = false;

	[HideInInspector]public Transform ownerCamera;
	[HideInInspector]public Transform ownerPlayer;

	[HideInInspector]public bool itemEnabled = false;
	[HideInInspector]public string world = "default";

	[HideInInspector]public bool isRemote = false;

	[HideInInspector]public EventHandler OnBeginUseItem;
	[HideInInspector]public EventHandler OnEndUseItem;
	[HideInInspector]public EventHandler OnDrop;

	[HideInInspector]public Dictionary<string, SyncronizedVariable> syncronizedVariables = new Dictionary<string, SyncronizedVariable>();

	public enum rarity
	{
		uncommon,
		common,
		rare,
		epic,
		legendary
	}

	public void RarityObject()
	{
		if (rarityVFXGameObject == null)
		{
			GameObject rarityObject = null;

			if (itemRarity == rarity.uncommon)
			{
				rarityObject = GameManager._rarity_uncommon;
				GetComponent<Outline>().OutlineColor = GameManager._mat_uncommon.color;
			}
			else if (itemRarity == rarity.common)
			{
				rarityObject = GameManager._rarity_common;
				GetComponent<Outline>().OutlineColor = GameManager._mat_common.color;
			}
			else if (itemRarity == rarity.rare)
			{
				rarityObject = GameManager._rarity_rare;
				GetComponent<Outline>().OutlineColor = GameManager._mat_rare.color;
			}
			else if (itemRarity == rarity.epic)
			{
				rarityObject = GameManager._rarity_epic;
				GetComponent<Outline>().OutlineColor = GameManager._mat_epic.color;
			}
			else if (itemRarity == rarity.legendary)
			{
				rarityObject = GameManager._rarity_legendary;
				GetComponent<Outline>().OutlineColor = GameManager._mat_legendary.color;
			}

			if (itemCenterTransform != null)
			{
				GameObject rarityVFX = Instantiate(rarityObject, itemCenterTransform);
				rarityVFX.transform.rotation = Quaternion.Euler(rarityVFX.transform.localRotation.eulerAngles - gameObject.transform.eulerAngles);
				rarityVFX.transform.localPosition = new Vector3(-itemCenterTransform.localPosition.z, 0f, 0f);
				rarityVFX.transform.localScale = new Vector3(1 / itemCenterTransform.localScale.x, 1 / itemCenterTransform.localScale.y, 1 / itemCenterTransform.localScale.z);
				rarityVFXGameObject = rarityVFX;
			}
			else
			{
				GameObject rarityVFX = Instantiate(rarityObject, gameObject.transform);
				rarityVFX.transform.rotation = Quaternion.Euler(rarityVFX.transform.localRotation.eulerAngles - gameObject.transform.eulerAngles);
				rarityVFX.transform.localScale = new Vector3(1 / gameObject.transform.localScale.x, 1 / gameObject.transform.localScale.y, 1 / gameObject.transform.localScale.z);
				rarityVFXGameObject = rarityVFX;
			}

			rarityVFXGameObject.SetActive(false);
		}
	}

	private void Start()
	{
		RarityObject();
	}

	public void SetSyncVar(string name, object value, string type = "")
	{
		if (!syncronizedVariables.ContainsKey(name))
		{
			syncronizedVariables.Add(name, new SyncronizedVariable(value, type, name));
		}
		else
		{
			syncronizedVariables[name].Set(value);
		}

		ClientSend.SetSyncVar(syncronizedVariables[name].name, syncronizedVariables[name].value, syncronizedVariables[name].type, uuid);
	}

	public object GetSyncVar(string name)
	{
		if (syncronizedVariables.ContainsKey(name))
		{
			return syncronizedVariables[name].Get();
		}
		else
		{
			return null;
		}
	}

	public void ActivateItem()
	{
		GameManager.items.Add(uuid, this);
		SetDoUpdates(false);
		realPos = transform.position;
	}

	public void SetDoUpdates(bool doOrDont)
	{
		itemEnabled = doOrDont;
		OnBeginUseItem = null;
		OnEndUseItem = null;
		OnDrop = null;

		MonoBehaviour[] scripts = gameObject.transform.GetComponentsInChildren<MonoBehaviour>(true);
		foreach (MonoBehaviour script in scripts)
		{
			if(script != this && script.GetType() != typeof(Outline))
			{
				script.enabled = doOrDont;
			}
		}

		LineRenderer[] lineRenderers = gameObject.transform.GetComponentsInChildren<LineRenderer>(true);
		foreach (LineRenderer lineRenderer in lineRenderers)
		{
			lineRenderer.enabled = doOrDont;
		}

		if (!doOrDont)
		{
			if(rarityVFXGameObject == null)
			{
				RarityObject();
			}

			rarityVFXGameObject.SetActive(true);

			GetComponent<Outline>().enabled = true;
		}
		else
		{
			if (rarityVFXGameObject == null)
			{
				RarityObject();
			}

			rarityVFXGameObject.SetActive(false);

			GetComponent<Outline>().enabled = false;
		}
	}

	public void LandFromProgressionPosition(Vector3 progressionPosition, Vector3 loc)
	{
		RaycastHit hit;
		//Debug.DrawRay(loc, new Vector3(0, -Vector3.Distance(progressionPosition, loc), 0), Color.red, Vector3.Distance(progressionPosition, loc));
		if (Physics.Raycast(loc, new Vector3(0, -Vector3.Distance(progressionPosition, loc), 0), out hit, Vector3.Distance(progressionPosition, loc), groundLayer))
		{
			if (hit.point != null)
			{
				isDropping = false;
				transform.position = hit.point + new Vector3(0, itemDistanceFromGround, 0);
				realPos = hit.point + new Vector3(0, itemDistanceFromGround, 0);
			}
		}
		else
		{
			transform.position = new Vector3(loc.x, progressionPosition.y, loc.z);
			realPos = new Vector3(loc.x, progressionPosition.y, loc.z);
			isDropping = true;
		}
	}

	public void BeginUseItemInhand()
	{
		OnBeginUseItem?.Invoke(this, EventArgs.Empty);
		isUsing = true;
		isUsingDown = true;
	}

	public void EndUseItemInhand()
	{
		OnEndUseItem?.Invoke(this, EventArgs.Empty);
		isUsing = false;
		isUsingDown = false;
	}

	public void Update()
	{
		if(transform.position.y <= -2000f)
		{
			isDropping = false;
			transform.position = new Vector3(transform.position.x, -2000f, transform.position.z);
		}

		if(UIManager.blockUserInput != true)
		{
			if (isPickedUp)
			{
				if (!isRemote)
				{
					if (Input.GetMouseButtonDown(0))
					{
						BeginUseItemInhand();
						ClientSend.UsingHandState(1);
					}
					else if (Input.GetMouseButtonUp(0))
					{
						EndUseItemInhand();
						ClientSend.UsingHandState(0);
					}

					if (Input.GetMouseButton(0))
					{
						isUsingDown = true;
					}
					else
					{
						isUsingDown = false;
					}
				}
			}
		}
		else
		{
			if (!isRemote && isUsing || !isRemote && isUsingDown)
			{
				EndUseItemInhand();
				ClientSend.UsingHandState(0);
			}
		}

		if (!isUsingDown && isUsing)
		{
			OnEndUseItem?.Invoke(this, EventArgs.Empty);
			isUsing = false;
			ClientSend.UsingHandState(0);
		}

		if (isPickedUp && !isDropping)
		{
			if (!isUsing || isUsing && lastRot == Quaternion.identity)
			{
				SetDesiredRotation(Quaternion.identity);
			}

			transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * 5f);
		}

		RaycastHit hit;
		if (Physics.Raycast(transform.position, -(transform.up), out hit, 300, groundLayer) && isPickupable && !isPickedUp)
		{
			if (Vector3.Distance(hit.point + new Vector3(0, itemDistanceFromGround, 0), realPos) == 0f || Vector3.Distance(hit.point + new Vector3(0, itemDistanceFromGround, 0), realPos) < 0.1f)
			{
				isDropping = false;
				transform.position = landingPos;
			}
			else if (hit.point != null)
			{
				landingPos = hit.point + new Vector3(0, itemDistanceFromGround, 0);
				isDropping = true;
			}
		}

		if (isPickupable && !isDropping)
		{
			//transform.Rotate(0f, 100f * Time.deltaTime, 0f, Space.World);
			float cos = Mathf.Cos(Time.time - Time.deltaTime) * 0.05f;
			//float cos = 0;
			transform.position = new Vector3(realPos.x, (realPos.y + cos), realPos.z);
		}

		if (hit.point == Vector3.zero)
		{
			landingPos = transform.position - new Vector3(0, 10f, 0);
		}

		if (isDropping)
		{
			transform.position = Vector3.MoveTowards(transform.position, landingPos, 5f * Time.deltaTime);
			//ClientSend.ItemInfo(Client.instance.myId, landingPos, itemId, isPickedUp, 0);
		}
	}

	public void SetDesiredRotation(Quaternion rotation)
	{
		if(rotation == Quaternion.identity)
		{
			desiredRotation = transform.parent.rotation;
			lastRot = Quaternion.identity;
		}
		else
		{
			desiredRotation = rotation;
			lastRot = rotation;
		}
	}

	public void LateUpdate()
	{
		if (Vector3.Distance(transform.position, landingPos) < 0.1f && isDropping && !isPickedUp)
		{
			transform.position = landingPos;
			realPos = landingPos;
			transform.localScale = new Vector3(1f, 1f, 1f);
			isDropping = false;
			landingPos = new Vector3(0f, 0f, 0f);
			Debug.LogWarning(uuid);
		}
	}
}
