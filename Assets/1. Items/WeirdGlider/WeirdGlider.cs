using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeirdGlider : MonoBehaviour
{
	[HideInInspector] public Item item;

	private GameObject previewStructure;
	public Transform previewPosition;

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

			previewStructure = Instantiate(EnvironmentManagger.GetStructureGameobject(0), previewPosition);
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

			Destroy(previewStructure);
		};

		item.OnDrop += (object sender, System.EventArgs e) =>
		{
			//Called when the item gets dropped
			//Use this to stop every action that is curently being performed (for example automatic weapon firing or grappling gun grappling)
		};

		if (item.itemEnabled)
		{
			//Called when the item gets picked up
		}
	}

	private void Update()
	{
		//Update hapens every frame, when the item is picked up

		if (!item.isRemote)
		{
			//If local
		}
		else
		{
			//If remote
		}
	}
}
