using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestItem : MonoBehaviour
{
	[HideInInspector] public Item item;

	public TMP_Text selectedDisplay;

	private void OnEnable()
	{
		item = gameObject.GetComponent<Item>();

		if (!item.isRemote)
		{
			//Use this if statement where you want the code block to execute only
			//If the item is in the local player's hand!
			//For example if you want to create a spring joint or add any sort of
			//Movement to the player!
		}

		item.OnBeginUseItem += (object sender, System.EventArgs e) =>
		{
			//Called when the player begins to use the item thay have in their hands
			//This function takes the delay set in the item script into consideration!

			if (!item.isRemote)
			{
				//If local
			}
			else
			{
				//If remote
			}
		};

		item.OnEndUseItem += (object sender, System.EventArgs e) =>
		{
			//Called when the player stops using the item (has released the mouse button)

			if (!item.isRemote)
			{
				//If local
			}
			else
			{
				//If remote
			}
		};

		item.OnDrop += (object sender, System.EventArgs e) =>
		{
			//Called when the item gets dropped
			//Use this to stop every action that is curently being performed (for example automatic weapon firing or grappling gun grappling)
		};

		if (item.itemEnabled)
		{
			if (item.GetSyncVar("selected") == null)
			{
				item.SetSyncVar("selected", 0, "int");
			}

			//Called when the item gets picked up

			//Usage of syncronised variables
			//Syncronised variables sync across to other clients and save with world save
			//item.SetSyncVar("name", "value", "string"); //!!!! type can only be 'string', 'int', 'float', 'bool' !!!!!!!!!
			//string value = (string)item.GetSyncVar("name");
		}
	}

	private void Update()
	{
		//Update hapens every frame, when the item is picked up

		if (!item.isRemote)
		{
			//If local

			if (Input.GetAxis("Mouse ScrollWheel") > 0)
			{
				item.SetSyncVar("selected", (int)item.GetSyncVar("selected") + 1);
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				item.SetSyncVar("selected", (int)item.GetSyncVar("selected") - 1);
			}
		}
		else
		{
			//If remote
		}

		if(item.GetSyncVar("selected") != null)
		{
			selectedDisplay.SetText(((int)item.GetSyncVar("selected")).ToString());
		}
	}
}
