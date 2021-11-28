using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingTool : MonoBehaviour
{
	[HideInInspector] public Item item;

	private GameObject previewStructure;
	public Transform previewPosition;
	public GameObject shilouetter;
	public TMP_Text selectionDisplay;

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
				if ((int)item.GetSyncVar("mode") == 0)
				{
					ClientSend.ChatMessage("/spawn structure " + ((int)item.GetSyncVar("selectedStructure")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("targetX")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("targetY")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("targetZ")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("rotX")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("rotY")).ToString() + " " + Mathf.RoundToInt((int)item.GetSyncVar("rotZ")).ToString() + " " + item.world);
				}
				else if ((int)item.GetSyncVar("mode") == 1)
				{
					if (item.GetSyncVar("selectedStructureUuid") != null && (string)item.GetSyncVar("selectedStructureUuid") != "")
					{
						EnvironmentManagger.DestroyStructureServer((string)item.GetSyncVar("selectedStructureUuid"));
						item.SetSyncVar("selectedStructureUuid", "");
					}
				}
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

			if(previewStructure != null)
			{
				Destroy(previewStructure);
			}

			if(selectedStructure != null && selectedStructure.GetComponent<Outline>() != null)
			{
				Destroy(selectedStructure.GetComponent<Outline>());
			}
		};

		if (item.itemEnabled)
		{
			//Called when the item gets picked up

			if(item.GetSyncVar("selectedStructure") == null)
			{
				item.SetSyncVar("selectedStructure", 0, "int");
			}

			if(item.GetSyncVar("rotX") == null)
			{
				item.SetSyncVar("rotX", 0, "int");
			}

			if (item.GetSyncVar("rotY") == null)
			{
				item.SetSyncVar("rotY", 0, "int");
			}

			if (item.GetSyncVar("rotZ") == null)
			{
				item.SetSyncVar("rotZ", 0, "int");
			}

			if ((int)item.GetSyncVar("selectedStructure") > EnvironmentManagger.environmentStructures.Count)
			{
				item.SetSyncVar("selectedStructure", EnvironmentManagger.environmentStructures.Count - 1);
			}

			if (item.GetSyncVar("mode") == null)
			{
				item.SetSyncVar("mode", 0, "int");
			}

			if (item.GetSyncVar("selectedStructureUuid") == null)
			{
				item.SetSyncVar("selectedStructureUuid", "", "string");
			}

			if ((int)item.GetSyncVar("mode") == 0)
			{
				previewStructure = Instantiate(EnvironmentManagger.GetStructureGameobject((int)item.GetSyncVar("selectedStructure")));
				previewStructure.GetComponentInChildren<Collider>(true).enabled = false;
			}
		}
	}

	int selected = 0;
	GameObject selectedStructure = null;
	int mode = 0;

	private void Update()
	{
		GameObject strctre = null;
		EnvironmentManagger.existingEnvironmentStructures.TryGetValue((string)item.GetSyncVar("selectedStructureUuid"), out strctre);
		if (strctre == null)
		{
			item.SetSyncVar("selectedStructureUuid", "");
		}

		if ((string)item.GetSyncVar("selectedStructureUuid") != "" && (int)item.GetSyncVar("mode") == 1)
		{
			if(selectedStructure == null)
			{
				selectedStructure = EnvironmentManagger.existingEnvironmentStructures[(string)item.GetSyncVar("selectedStructureUuid")];
				selectedStructure.SetActive(false);
				selectedStructure.AddComponent<Outline>();
				selectedStructure.GetComponent<Outline>()._outlineFillMaterial = shilouetter.GetComponent<Outline>()._outlineFillMaterial;
				selectedStructure.GetComponent<Outline>()._outlineMaskMaterial = shilouetter.GetComponent<Outline>()._outlineMaskMaterial;
				selectedStructure.GetComponent<Outline>().OutlineColor = shilouetter.GetComponent<Outline>().OutlineColor;
				//selectedStructure.GetComponent<Outline>().precomputeOutline = shilouetter.GetComponent<Outline>()._outlineMaskMaterial;
				selectedStructure.GetComponent<Outline>().OutlineWidth = shilouetter.GetComponent<Outline>().OutlineWidth;
				selectedStructure.GetComponent<Outline>().OutlineMode = shilouetter.GetComponent<Outline>().OutlineMode;
				selectedStructure.SetActive(true);
			}
			else if (selectedStructure != null && selectedStructure.GetComponent<Structure>().uuid != (string)item.GetSyncVar("selectedStructureUuid"))
			{
				Destroy(selectedStructure.GetComponent<Outline>());
				selectedStructure = EnvironmentManagger.existingEnvironmentStructures[(string)item.GetSyncVar("selectedStructureUuid")];
				selectedStructure.SetActive(false);
				selectedStructure.AddComponent<Outline>();
				selectedStructure.GetComponent<Outline>()._outlineFillMaterial = shilouetter.GetComponent<Outline>()._outlineFillMaterial;
				selectedStructure.GetComponent<Outline>()._outlineMaskMaterial = shilouetter.GetComponent<Outline>()._outlineMaskMaterial;
				selectedStructure.GetComponent<Outline>().OutlineColor = shilouetter.GetComponent<Outline>().OutlineColor;
				//selectedStructure.GetComponent<Outline>().precomputeOutline = shilouetter.GetComponent<Outline>()._outlineMaskMaterial;
				selectedStructure.GetComponent<Outline>().OutlineWidth = shilouetter.GetComponent<Outline>().OutlineWidth;
				selectedStructure.GetComponent<Outline>().OutlineMode = shilouetter.GetComponent<Outline>().OutlineMode;
				selectedStructure.SetActive(true);
			}
		}
		else
		{
			if(selectedStructure != null)
			{
				if(GetComponent<Outline>() != null)
				{
					Destroy(selectedStructure.GetComponent<Outline>());
				}
			}
		}

		//Update hapens every frame, when the item is picked up

		if (!item.isRemote)
		{
			//If local

			if((int)item.GetSyncVar("mode") == 0 && UIManager.blockUserInput != true)
			{
				if (Input.GetAxis("Mouse ScrollWheel") > 0)
				{
					if ((int)item.GetSyncVar("selectedStructure") + 1 < EnvironmentManagger.environmentStructures.Count)
					{
						item.SetSyncVar("selectedStructure", (int)item.GetSyncVar("selectedStructure") + 1);
					}
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0)
				{
					if ((int)item.GetSyncVar("selectedStructure") - 1 >= 0)
					{
						item.SetSyncVar("selectedStructure", (int)item.GetSyncVar("selectedStructure") - 1);
					}
				}

				if (Input.GetKeyDown(KeyCode.R))
				{
					item.SetSyncVar("rotY", (int)item.GetSyncVar("rotY") + 45);
				}

				if (Input.GetKeyDown(KeyCode.F))
				{
					previewPosition.position = new Vector3(previewPosition.position.x, previewPosition.position.y, previewPosition.position.z - 1);
				}

				if (Input.GetKeyDown(KeyCode.C))
				{
					previewPosition.position = new Vector3(previewPosition.position.x, previewPosition.position.y, previewPosition.position.z + 1);
				}

				if (Input.GetKeyDown(KeyCode.G))
				{
					previewPosition.position = new Vector3(previewPosition.position.x, previewPosition.position.y - 1, previewPosition.position.z);
				}

				if (Input.GetKeyDown(KeyCode.V))
				{
					previewPosition.position = new Vector3(previewPosition.position.x, previewPosition.position.y + 1, previewPosition.position.z);
				}

				if (Input.GetKeyDown(KeyCode.H))
				{
					previewPosition.position = new Vector3(previewPosition.position.x - 1, previewPosition.position.y, previewPosition.position.z);
				}

				if (Input.GetKeyDown(KeyCode.B))
				{
					previewPosition.position = new Vector3(previewPosition.position.x + 1, previewPosition.position.y, previewPosition.position.z);
				}
			}

			if(Input.GetMouseButtonDown(1) && UIManager.blockUserInput != true)
			{
				if((int)item.GetSyncVar("mode") == 0)
				{
					item.SetSyncVar("mode", 1);
				}
				else
				{
					item.SetSyncVar("mode", 0);
				}
			}

			if ((int)item.GetSyncVar("mode") == 1)
			{
				if (Physics.Raycast(transform.GetComponent<Item>().ownerCamera.position, transform.GetComponent<Item>().ownerCamera.forward, out var hitInfo, 100f))
				{
					if (hitInfo.transform.gameObject != null)
					{
						Structure s;
						hitInfo.transform.gameObject.TryGetComponent<Structure>(out s);

						if (s != null)
						{
							item.SetSyncVar("selectedStructureUuid", s.uuid);
						}
					}
				}
			}
		}
		else
		{
			//If remote
		}

		if((int)item.GetSyncVar("mode") != mode)
		{
			if(mode == 0)
			{
				Destroy(previewStructure);
				selectedStructure = null;
			}
			else
			{
				selected = (int)item.GetSyncVar("selectedStructure");
				item.SetSyncVar("selectedStructureUuid", "");

				if (previewStructure != null)
				{
					Destroy(previewStructure);
				}

				previewStructure = Instantiate(EnvironmentManagger.GetStructureGameobject((int)item.GetSyncVar("selectedStructure")));
				previewStructure.GetComponentInChildren<Collider>(true).enabled = false;
			}

			mode = (int)item.GetSyncVar("mode");
		}

		if (previewStructure != null)
		{
			if (!item.isRemote)
			{
				item.SetSyncVar("targetX", Mathf.RoundToInt(previewPosition.position.x + EnvironmentManagger.GetStructureOffset((int)item.GetSyncVar("selectedStructure")).x), "int");
				item.SetSyncVar("targetY", Mathf.RoundToInt(previewPosition.position.y + EnvironmentManagger.GetStructureOffset((int)item.GetSyncVar("selectedStructure")).y), "int");
				item.SetSyncVar("targetZ", Mathf.RoundToInt(previewPosition.position.z + EnvironmentManagger.GetStructureOffset((int)item.GetSyncVar("selectedStructure")).z), "int");
			}

			Vector3 target = new Vector3((int)item.GetSyncVar("targetX"), (int)item.GetSyncVar("targetY"), (int)item.GetSyncVar("targetZ"));
			Quaternion rot = Quaternion.Euler((int)item.GetSyncVar("rotX"), (int)item.GetSyncVar("rotY"), (int)item.GetSyncVar("rotZ"));

			float maxM = (Vector3.Distance(target, previewStructure.transform.position) / 3f) * Time.deltaTime;

			previewStructure.transform.position = Vector3.MoveTowards(previewStructure.transform.position, target, maxM * 40f);

			previewStructure.transform.rotation = Quaternion.RotateTowards(previewStructure.transform.rotation, rot, 360f * Time.deltaTime);
		}

		selectionDisplay.SetText(((int)item.GetSyncVar("selectedStructure")).ToString());

		if(selected != (int)item.GetSyncVar("selectedStructure"))
		{
			selected = (int)item.GetSyncVar("selectedStructure");

			if (previewStructure != null)
			{
				Destroy(previewStructure);
			}

			previewStructure = Instantiate(EnvironmentManagger.GetStructureGameobject((int)item.GetSyncVar("selectedStructure")));
			previewStructure.GetComponentInChildren<Collider>(true).enabled = false;
		}
	}
}
