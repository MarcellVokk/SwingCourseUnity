using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentManagger : MonoBehaviour
{
	[Serializable]
	public struct environmentStructure
	{
		public Vector3 offset;
		public GameObject prefab;
	}

	[Serializable]
	public struct entity
	{
		public GameObject prefab;
	}

	public static Dictionary<string, GameObject> existingEnvironmentStructures = new Dictionary<string, GameObject>();
	public static Dictionary<string, GameObject> existingEntities = new Dictionary<string, GameObject>();

	public static List<environmentStructure> environmentStructures;
	public List<environmentStructure> _environmentStructures;

	public static List<entity> entities;
	public List<entity> _entities;

	private void Start()
	{
		environmentStructures = _environmentStructures;
		entities = _entities;
	}

	public static void ControlEntityPhysics(string entityUuid)
	{
		ClientSend.PutPlayerInCharge(Client.instance.myId, entityUuid);
	}

	public static void AbandonControlPhysics(string entityUuid)
	{
		ClientSend.PutPlayerInCharge(0, entityUuid);
	}

	public static void SpawnEntity(int id, Vector3 location, Quaternion rotation, string uuid, int playerInCharge)
	{
		GameObject newEntity = Instantiate(entities[id].prefab, location, rotation);
		newEntity.GetComponent<Entity>().targetPos = location;
		newEntity.GetComponent<Entity>().targetRot = rotation;
		newEntity.GetComponent<Entity>().uuid = uuid;
		newEntity.GetComponent<Entity>().playerInCharge = playerInCharge;
		existingEntities.Add(uuid, newEntity);
	}

	public static void CreateStructureLocal(int structureId, Vector3 location, Quaternion rotation, string uuid)
	{
		GameObject newStructure = Instantiate(environmentStructures[structureId].prefab, location, rotation);
		newStructure.GetComponent<Structure>().id = structureId;
		newStructure.GetComponent<Structure>().uuid = uuid;
		existingEnvironmentStructures.Add(uuid, newStructure);
	}

	public static void DestroyStructureLocal(string uuid)
	{
		Destroy(existingEnvironmentStructures[uuid]);
		existingEnvironmentStructures.Remove(uuid);
	}

	public static void DestroyEntityLocal(string uuid)
	{
		Destroy(existingEntities[uuid]);
		existingEntities.Remove(uuid);
	}

	public static void DestroyStructureServer(string uuid)
	{
		ClientSend.ChatMessage("/kill structure " + uuid);
	}

	public static void DestroyAllStructuresLocal()
	{
		foreach(string s in existingEnvironmentStructures.Keys.ToList())
		{
			DestroyStructureLocal(s);
		}
	}

	public static void DestroyAllEntitiesLocal()
	{
		foreach(string s in existingEntities.Keys.ToList())
		{
			DestroyEntityLocal(s);
		}
	}

	public static GameObject GetStructureGameobject(int id)
	{
		if(environmentStructures.Count > id)
		{
			return environmentStructures[id].prefab;
		}
		else
		{
			return null;
		}
	}

	public static Vector3 GetStructureOffset(int id)
	{
		if (environmentStructures.Count > id)
		{
			return environmentStructures[id].offset;
		}
		else
		{
			return Vector3.zero;
		}
	}
}
