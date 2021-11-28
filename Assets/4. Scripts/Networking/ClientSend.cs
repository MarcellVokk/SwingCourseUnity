using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived(string username)
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(username);

            SendTCPData(_packet);
        }
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void PlayerMovement(Vector3 position, Quaternion rotation, Quaternion verticalRotation, bool isTeleport, bool isMounted)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(position);
            _packet.Write(rotation);
            _packet.Write(verticalRotation);
            _packet.Write(isTeleport);
            _packet.Write(isMounted);

            SendUDPData(_packet);
        }
    }

    public static void PingResponse()
	{
        using(Packet _packet = new Packet((int)ClientPackets.pingResponse))
		{
            SendTCPData(_packet);
		}
	}

    public static void PickupItem(string itemId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerPickupItem))
        {
            _packet.Write(itemId);

            SendTCPData(_packet);
        }
    }

    public static void DroppItem(Vector3 droppPosition)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerDroppItem))
        {
            _packet.Write(droppPosition);

            SendTCPData(_packet);
        }
    }

    public static void UsingHandState(int state)
	{
        using (Packet _packet = new Packet((int)ClientPackets.receiveUsingHandState))
        {
            _packet.Write(state);

            SendTCPData(_packet);
        }
    }

    public static void ChatMessage(string text)
	{
        using (Packet _packet = new Packet((int)ClientPackets.chatMessage))
        {
            _packet.Write(text);

            SendTCPData(_packet);
        }
    }

    public static void JoinWorld(int world)
    {
        using (Packet _packet = new Packet((int)ClientPackets.joinWorld))
        {
            _packet.Write(world);

            SendTCPData(_packet);
        }
    }

    public static void SetSyncVar(string name, object value, string type, string objectUuid)
    {
        using (Packet _packet = new Packet((int)ClientPackets.setSyncVar))
        {
            _packet.Write(name);
            _packet.Write(value.ToString());
            _packet.Write(type);
            _packet.Write(objectUuid);

            SendTCPData(_packet);
        }
    }

    public static void SendEntityTransform(string uuid, Vector3 position, Quaternion rotation)
    {
        using (Packet _packet = new Packet((int)ClientPackets.entityTransform))
        {
            _packet.Write(uuid);
            _packet.Write(position);
            _packet.Write(rotation);

            SendTCPData(_packet);
        }
    }

    public static void PutPlayerInCharge(int player, string uuid)
    {
        using (Packet _packet = new Packet((int)ClientPackets.putPlayerInCharge))
        {
            _packet.Write(player);
            _packet.Write(uuid);

            SendTCPData(_packet);
        }
    }

    public static void ParentEntityToPlayer(int player, string entityUuid, bool onOrOff)
    {
        using (Packet _packet = new Packet((int)ClientPackets.parentEntityToPlayer))
        {
            _packet.Write(player);
            _packet.Write(entityUuid);
            _packet.Write(onOrOff);

            SendTCPData(_packet);
        }
    }

    #endregion
}
