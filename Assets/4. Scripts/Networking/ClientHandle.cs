using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived(Client.instance.username);

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        bool isMounted = _packet.ReadBool();
        string mountedEntityUuid = _packet.ReadString();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, isMounted, mountedEntityUuid);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if(_id == Client.instance.myId)
		{
            return;
		}

        Vector3 _position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();
        Quaternion verticalRotation = _packet.ReadQuaternion();
        bool _isTeleport = _packet.ReadBool();
        bool isMounted = _packet.ReadBool();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.isMounted = isMounted;
			if (_isTeleport)
			{
                _player.transform.position = _position;
                _player.transform.rotation = rotation;
                _player.verticalRotation.transform.rotation = verticalRotation;
            }
			else
			{
                _player.targetTransform = _position;
                _player.transform.rotation = rotation;
                _player.verticalRotation.transform.rotation = verticalRotation;
            }
        }
    }

	public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].GetComponent<RemoteInventoryManagger>().DroppItem(GameManager.players[_id].transform.position);

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void RemoveAvatar(Packet _packet)
	{
		int _id = _packet.ReadInt();

        if(_id != Client.instance.myId)
		{
            Destroy(GameManager.players[_id].gameObject);
            GameManager.players.Remove(_id);
        }
	}

    public static void Ping(Packet _packet)
	{
        ClientSend.PingResponse();
	}

    public static void PlayerPickedupItem(Packet _packet)
    {
        int _id = _packet.ReadInt();

        string itemId = _packet.ReadString();

        if(_id != Client.instance.myId)
		{
            GameManager.players[_id].gameObject.GetComponent<RemoteInventoryManagger>().PickupItem(GameManager.items[itemId].gameObject);
        }
    }

    public static void PlayerDroppedItem(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Vector3 finalPos = _packet.ReadVector3();

        if (_id != Client.instance.myId)
        {
            GameManager.players[_id].GetComponent<RemoteInventoryManagger>().DroppItem(finalPos);
        }
    }

    public static void ReceiveUsingHandState(Packet _packet)
	{
        int _id = _packet.ReadInt();

        int state = _packet.ReadInt();

        if (_id != Client.instance.myId)
		{
            if(GameManager.players[_id].GetComponent<RemoteInventoryManagger>().inHand != null && state == 1)
			{
                GameManager.players[_id].GetComponent<RemoteInventoryManagger>().inHand.GetComponent<Item>().BeginUseItemInhand();
            }
            else if (GameManager.players[_id].GetComponent<RemoteInventoryManagger>().inHand != null && state == 0)
            {
                GameManager.players[_id].GetComponent<RemoteInventoryManagger>().inHand.GetComponent<Item>().EndUseItemInhand();
            }
        }
    }

    public static void ReceiveItemInfo(Packet _packet)
	{
        Vector3 loc = _packet.ReadVector3();

        string itemId = _packet.ReadString();

        bool onPlayer = _packet.ReadBool();

        int player = _packet.ReadInt();

        GameManager.items[itemId].transform.position = loc;
        GameManager.items[itemId].realPos = loc;
        GameManager.items[itemId].isDropping = true;

        if (onPlayer)
		{
            GameManager.items[itemId].transform.position = GameManager.players[player].transform.position;
            GameManager.players[player].GetComponent<RemoteInventoryManagger>().PickupItem(GameManager.items[itemId].gameObject);
		}
    }

    public static void TeleportPlayer(Packet _packet)
	{
        int _id = _packet.ReadInt();

        Vector3 newLoc  = _packet.ReadVector3();

        if(GameManager.players[_id] != null && GameManager.players[_id].playerController != null)
		{
            GameManager.players[_id].playerController.transform.position = newLoc;
        }
    }

    public static void SpawnItem(Packet _packet)
	{
        string itemId = _packet.ReadString();

        Vector3 location = _packet.ReadVector3();

        bool onPlayer = _packet.ReadBool();

        int player = _packet.ReadInt();

        string itemIndex = _packet.ReadString();

        Vector3 progressionPosition = _packet.ReadVector3();

        string world = _packet.ReadString();

        string syncVars = _packet.ReadString();

        Debug.Log(syncVars);

        List<char> syncVarChars = syncVars.ToCharArray().ToList();

        Dictionary<string, SyncronizedVariable> syncVarsFinal = new Dictionary<string, SyncronizedVariable>();
        List<string> syncVarStrings = new List<string>();

        bool closeParagraph = false;
        string apend = "";
        for(int i = 0; i < syncVars.Length; i++)
		{
            if(syncVarChars[i] == '(')
			{
                closeParagraph = true;
                continue;
			}
			else
			{
                if (syncVarChars[i] == ')')
                {
                    closeParagraph = false;
                    syncVarStrings.Add(apend);
                    apend = "";
                    continue;
                }

				if (closeParagraph)
				{
                    apend += syncVarChars[i].ToString();
				}
            }
		}

        foreach(string s in syncVarStrings)
		{
            string name = (s + ",").Split(',')[0];
            string value = (s + ",").Split(',')[1];
            string type = (s + ",").Split(',')[2];

            object finalValue = null;

            if (type == "int")
            {
                finalValue = int.Parse(value);
            }
            else if (type == "float")
            {
                finalValue = float.Parse(value);
            }
            else if (type == "bool")
            {
                finalValue = bool.Parse(value);
            }
            else if (type == "string")
            {
                finalValue = value.ToString();
            }

            syncVarsFinal.Add(name, new SyncronizedVariable(finalValue, type, name));
		}

        GameObject item = Instantiate(ItemManagger.GetItemPrefab(itemId));
        item.GetComponent<Item>().uuid = itemIndex;
        item.GetComponent<Item>().world = world;
        item.GetComponent<Item>().syncronizedVariables = syncVarsFinal;
        item.GetComponent<Item>().ActivateItem();

        if (onPlayer)
        {
            if (player == Client.instance.myId)
            {
                item.GetComponent<Item>().transform.position = GameManager.players[player].playerController.transform.position;
                item.GetComponent<Item>().realPos = GameManager.players[player].playerController.transform.position;
                GameManager.players[player].playerController.GetComponentInChildren<InventoryManagger>().PickUp(item.gameObject);
            }
            else
            {
                item.GetComponent<Item>().transform.position = GameManager.players[player].transform.position;
                item.GetComponent<Item>().realPos = GameManager.players[player].transform.position;
                GameManager.players[player].GetComponent<RemoteInventoryManagger>().PickupItem(item.gameObject);
            }
        }
		else
		{
            item.GetComponent<Item>().LandFromProgressionPosition(progressionPosition, location);
        }
    }

    public static void KillItem(Packet _packet)
	{
        string itemIndex = _packet.ReadString();

        ItemManagger.KillItem(itemIndex);
	}

    public static void ChatMessage(Packet _packet)
	{
        string text = _packet.ReadString();
        float r = _packet.ReadFloat();
        float g = _packet.ReadFloat();
        float b = _packet.ReadFloat();

        GameManager.instance.AddChatMessage(text, new Color(r,g,b));
	}

    public static void SpawnStructure(Packet _packet)
	{
        int structureId = _packet.ReadInt();

        Vector3 location = _packet.ReadVector3();

        Quaternion rotation = _packet.ReadQuaternion();

        string uuid = _packet.ReadString();

        EnvironmentManagger.CreateStructureLocal(structureId, location, rotation, uuid);
	}

    public static void DestroyStructure(Packet _packet)
    {
        string uuid = _packet.ReadString();

        EnvironmentManagger.DestroyStructureLocal(uuid);
    }

    public static void ChangeWorld(Packet _packet)
    {
        GameManager.players[Client.instance.myId].playerController.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GameManager.players[Client.instance.myId].playerController.gameObject.GetComponent<PlayerMovement>().enabled = false;

        int world = _packet.ReadInt();

        foreach(int id in GameManager.players.Keys.ToList())
		{
            if(id != Client.instance.myId)
			{
                Destroy(GameManager.players[id].gameObject);
                GameManager.players.Remove(id);
            }
        }

        foreach(string uuid in GameManager.items.Keys.ToList())
		{
            Destroy(GameManager.items[uuid].gameObject);
            GameManager.items.Remove(uuid);
		}

        EnvironmentManagger.DestroyAllStructuresLocal();

        EnvironmentManagger.DestroyAllEntitiesLocal();

        ClientSend.JoinWorld(world);
    }

    public static void ShowWorld(Packet _packet)
	{
        Vector3 newLoc = Vector3.zero;

        if (GameManager.players[Client.instance.myId] != null && GameManager.players[Client.instance.myId].playerController != null)
        {
            GameManager.players[Client.instance.myId].playerController.transform.position = newLoc;
        }

        GameManager.players[Client.instance.myId].playerController.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GameManager.players[Client.instance.myId].playerController.gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    public static void SetSyncVar(Packet _packet)
    {
        string name = _packet.ReadString();
        string value = _packet.ReadString();
        string type = _packet.ReadString();

        string objectUuid = _packet.ReadString();
        object finalValue = value;

        if (type == "int")
        {
            finalValue = int.Parse(value);
        }
        else if (type == "float")
        {
            finalValue = float.Parse(value);
        }
        else if (type == "bool")
        {
            finalValue = bool.Parse(value);
        }
        else if (type == "string")
        {
            finalValue = value.ToString();
        }

        if (GameManager.items[objectUuid].syncronizedVariables.ContainsKey(name))
        {
            GameManager.items[objectUuid].syncronizedVariables[name].Set(finalValue, type, name);
        }
        else
        {
            GameManager.items[objectUuid].syncronizedVariables.Add(name, new SyncronizedVariable(finalValue, type, name));
        }
    }

    public static void GetEntityTransform(Packet _packet)
    {
        string uuid = _packet.ReadString();
        Vector3 position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();

        if (EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().remote)
        {
            EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().targetPos = position;
            EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().targetRot = rotation;
        }

        //EnvironmentManagger.existingEntities[uuid].transform.position = position;
        //EnvironmentManagger.existingEntities[uuid].transform.rotation = rotation;
    }

    public static void SpawnEntity(Packet _packet)
    {
        int entityId = _packet.ReadInt();

        Vector3 location = _packet.ReadVector3();

        Quaternion rotation = _packet.ReadQuaternion();

        string uuid = _packet.ReadString();

        int playerInCharge = _packet.ReadInt();

        EnvironmentManagger.SpawnEntity(entityId, location, rotation, uuid, playerInCharge);
    }

    public static void KillEntity(Packet _packet)
    {
        string uuid = _packet.ReadString();

        EnvironmentManagger.DestroyEntityLocal(uuid);
    }

    public static void PutClientInCharge(Packet _packet)
    {
        int player = _packet.ReadInt();

        string uuid = _packet.ReadString();

		if (EnvironmentManagger.existingEntities.ContainsKey(uuid))
		{
            if(player == Client.instance.myId)
            {
                EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().PutInChargeEvent();
            }
            else
            {
                EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().LostInChargeEvent();
            }

            EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().playerInCharge = player;
        }
    }

    public static void ParentEntityToPlayer(Packet _packet)
    {
        int player = _packet.ReadInt();

        string uuid = _packet.ReadString();

        bool onOrOff = _packet.ReadBool();

        if (onOrOff)
        {
            if (EnvironmentManagger.existingEntities.ContainsKey(uuid) && GameManager.players.ContainsKey(player))
            {
                GameManager.players[player].transform.parent = EnvironmentManagger.existingEntities[uuid].transform;
                EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().childPlayer = player;
            }
        }
        else
        {
            if (EnvironmentManagger.existingEntities.ContainsKey(uuid))
            {
                GameManager.players[player].transform.parent = null;
                EnvironmentManagger.existingEntities[uuid].GetComponent<Entity>().childPlayer = 0;
            }
        }
    }
}
