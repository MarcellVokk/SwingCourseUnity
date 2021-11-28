using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet, bool isLocalToWorld, int world = 0)
    {
		if (isLocalToWorld)
		{
            if(Server.clients[_toClient].player != null && Server.clients[_toClient].player.world == world)
			{
                _packet.WriteLength();
                Server.clients[_toClient].tcp.SendData(_packet);
            }
        }
		else
		{
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet, bool isLocalToWorld, int world = 0)
    {
        if (isLocalToWorld)
        {
            if (Server.clients[_toClient].player != null && Server.clients[_toClient].player.world == world)
            {
                _packet.WriteLength();
                Server.clients[_toClient].udp.SendData(_packet);
            }
        }
		else
		{
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet, bool isLocalToWorld, int world = 0)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
			if (isLocalToWorld)
			{
                if (Server.clients[i].player != null && Server.clients[i].player.world == world)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
			else
			{
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet, bool isLocalToWorld, int world = 0)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                if (isLocalToWorld)
                {
                    if (Server.clients[i].player != null && Server.clients[i].player.world == world)
                    {
                        Server.clients[i].tcp.SendData(_packet);
                    }
                }
                else
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet, bool isLocalToWorld, int world = 0)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
			if (isLocalToWorld)
			{
                if(Server.clients[i].player != null && Server.clients[i].player.world == world)
				{
                    Server.clients[i].udp.SendData(_packet);
                }
            }
			else
			{
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet, bool isLocalToWorld, int world = 0)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                if (isLocalToWorld)
                {
                    if (Server.clients[i].player != null && Server.clients[i].player.world == world)
                    {
                        Server.clients[i].udp.SendData(_packet);
                    }
                }
                else
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet, false);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.position);
            _packet.Write(_player.rotation);
            _packet.Write(_player.mounted);
            _packet.Write(_player.attachedEntityUuid);

            SendTCPData(_toClient, _packet, true, _player.world);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(int playerId, Vector3 position, Quaternion rotation, Quaternion verticalRotation, bool isTeleport, bool isMounted)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(playerId);
            _packet.Write(position);
            _packet.Write(rotation);
            _packet.Write(verticalRotation);
            _packet.Write(isTeleport);
            _packet.Write(isMounted);

            SendUDPDataToAll(_packet, true, Server.clients[playerId].player.world);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet, false);
        }
    }

    public static void RemoveAvatar(int _playerId, int curentWorld)
    {
        using (Packet _packet = new Packet((int)ServerPackets.removeAvatar))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet, true, curentWorld);
        }
    }

    public static void Ping()
	{
        using (Packet _packet = new Packet((int)ServerPackets.ping))
		{
            SendTCPDataToAll(_packet, false);
        }
    }

    public static void PlayerPickedupItem(int _playerId, string itemId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPickedupItem))
        {
            _packet.Write(_playerId);

            _packet.Write(itemId);

            SendTCPDataToAll(_packet, true, Server.clients[_playerId].player.world);
        }
    }

    public static void PlayerDroppedItem(int _playerId, Vector3 finalPos)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDroppedItem))
        {
            _packet.Write(_playerId);

            _packet.Write(finalPos);

            SendTCPDataToAll(_packet, true, Server.clients[_playerId].player.world);
        }
    }

    public static void UsingHandState(int _playerId, int state)
    {
        using (Packet _packet = new Packet((int)ServerPackets.usingHandState))
        {
            _packet.Write(_playerId);

            _packet.Write(state);

            SendTCPDataToAll(_packet, true, Server.clients[_playerId].player.world);
        }
    }

    public static void UsingHandState(int toPlayer, int _playerId, int state)
    {
        using (Packet _packet = new Packet((int)ServerPackets.usingHandState))
        {
            _packet.Write(_playerId);

            _packet.Write(state);

            SendTCPData(toPlayer, _packet, true, Server.clients[_playerId].player.world);
        }
    }

    public static void TeleportPlayer(int _playerId, Vector3 newLocation)
	{
        using (Packet _packet = new Packet((int)ServerPackets.teleportPlayer))
        {
            _packet.Write(_playerId);

            _packet.Write(newLocation);

            SendTCPDataToAll(_packet, true, Server.clients[_playerId].player.world);
        }
    }

    public static void SpawnItem(string itemId, Vector3 location, bool onPlayer, int player, string index, Vector3 progressionPosition, int world, List<SyncronizedVariable> syncVariables = null, int forPlayer = -1)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnItem))
        {
            _packet.Write(itemId);

            _packet.Write(location);

            _packet.Write(onPlayer);

            _packet.Write(player);

            _packet.Write(index);

            _packet.Write(progressionPosition);

            _packet.Write(Server.GetWorld(world).name);

            string syncVarsFinal = "";

            if (syncVariables != null)
			{
                foreach (SyncronizedVariable s in syncVariables)
                {
                    syncVarsFinal += "(" + s.name + "," + s.value.ToString() + "," + s.type + ")";
                }
            }

            _packet.Write(syncVarsFinal);

            if (forPlayer == -1)
			{
                SendTCPDataToAll(_packet, true, world);
            }
			else
			{
                SendTCPData(forPlayer, _packet, true, world);
            }
        }
    }

    public static void SpawnStructure(int structureId, Vector3 location, Quaternion rotation, string uuid, int world)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnStructure))
        {
            _packet.Write(structureId);

            _packet.Write(location);

            _packet.Write(rotation);

            _packet.Write(uuid);

            SendTCPDataToAll(_packet, true, world);
        }
    }

    public static void SpawnStructureForPlayer(int structureId, Vector3 location, Quaternion rotation, string uuid, int playerIndex, int world)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnStructure))
        {
            _packet.Write(structureId);

            _packet.Write(location);

            _packet.Write(rotation);

            _packet.Write(uuid);

            SendTCPData(playerIndex, _packet, true, world);
        }
    }

    public static void DestroyStructure(string uuid)
    {
        using (Packet _packet = new Packet((int)ServerPackets.destroyStructure))
        {
            _packet.Write(uuid);

            SendTCPDataToAll(_packet, true, Server.structures[uuid].world);
        }
    }

    public static void KillItem(string itemIndex)
    {
        using (Packet _packet = new Packet((int)ServerPackets.killItem))
        {
            _packet.Write(itemIndex);

            SendTCPDataToAll(_packet, false);
        }
    }

    public static void ChatMessageToAll(string text, UnityEngine.Color color)
    {
        using (Packet _packet = new Packet((int)ServerPackets.receiveChatMessage))
        {
            _packet.Write(text);
            _packet.Write(color.r);
            _packet.Write(color.g);
            _packet.Write(color.b);

            SendTCPDataToAll(_packet, false);
        }
    }

    public static void ChatMessageTo(int playerIndex, string text, UnityEngine.Color color)
    {
        using (Packet _packet = new Packet((int)ServerPackets.receiveChatMessage))
        {
            _packet.Write(text);
            _packet.Write(color.r);
            _packet.Write(color.g);
            _packet.Write(color.b);

            SendTCPData(playerIndex, _packet, false);
        }
    }

    public static void ChatMessageToAllExcept(int playerIndex, string text, UnityEngine.Color color)
    {
        using (Packet _packet = new Packet((int)ServerPackets.receiveChatMessage))
        {
            _packet.Write(text);
            _packet.Write(color.r);
            _packet.Write(color.g);
            _packet.Write(color.b);

            SendTCPDataToAll(playerIndex, _packet, false);
        }
    }

    public static void ChangeWorld(int playerId, int world)
    {
        using (Packet _packet = new Packet((int)ServerPackets.changeWorld))
        {
            _packet.Write(world);

            SendTCPData(playerId, _packet, false);
        }
    }

    public static void SetSyncVar(SyncronizedVariable variable, string objectUuid, int exceptionPlayer = -1)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setSyncVar))
        {
            _packet.Write(variable.name);
            _packet.Write(variable.value.ToString());
            _packet.Write(variable.type);

            _packet.Write(objectUuid);

            if(exceptionPlayer != -1)
			{
                SendTCPDataToAll(exceptionPlayer, _packet, true, Server.items[objectUuid].world);
            }
			else
			{
                SendTCPDataToAll(_packet, true, Server.items[objectUuid].world);
            }
        }
    }

    public static void ShowWorld(int playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.showWorld))
        {
            SendTCPData(playerId, _packet, false);
        }
    }

    public static void SendEntityTransform(string uuid, Vector3 position, Quaternion rotation)
	{
        using (Packet _packet = new Packet((int)ServerPackets.entityTransform))
        {
            _packet.Write(uuid);
            _packet.Write(position);
            _packet.Write(rotation);

            SendTCPDataToAll(Server.entities[uuid].playerInCharge, _packet, true, Server.entities[uuid].world);
        }
    }

    public static void SpawnEntity(int id, Vector3 position, Quaternion rotation, string uuid, int world, int playerInCharge)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnEntity))
        {
            _packet.Write(id);
            _packet.Write(position);
            _packet.Write(rotation);
            _packet.Write(uuid);
            _packet.Write(playerInCharge);

            SendTCPDataToAll(_packet, true, world);
        }
    }

    public static void SpawnEntityForPlayer(int entityId, Vector3 location, Quaternion rotation, string uuid, int playerIndex, int world, int playerInCharge)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnEntity))
        {
            _packet.Write(entityId);

            _packet.Write(location);

            _packet.Write(rotation);

            _packet.Write(uuid);

            _packet.Write(playerInCharge);

            SendTCPData(playerIndex, _packet, true, world);
        }
    }

    public static void PutPlayerInCharge(int player, string uuid)
    {
        using (Packet _packet = new Packet((int)ServerPackets.putPlayerInCharge))
        {
            _packet.Write(player);

            _packet.Write(uuid);

            Server.entities[uuid].playerInCharge = player;

            SendTCPDataToAll(_packet, true, Server.entities[uuid].world);
        }
    }

    public static void KillEntity(string uuid)
    {
        using (Packet _packet = new Packet((int)ServerPackets.killEntity))
        {
            _packet.Write(uuid);

            SendTCPDataToAll(_packet, true, Server.entities[uuid].world);
        }
    }

    public static void ParentEntityToPlayer(int player, string entityUuid, bool onOrOff)
    {
        using (Packet _packet = new Packet((int)ServerPackets.parentEntityToPlayer))
        {
            _packet.Write(player);
            _packet.Write(entityUuid);
            _packet.Write(onOrOff);

            SendTCPDataToAll(_packet, true, Server.entities[entityUuid].world);
        }
    }

    #endregion
}
