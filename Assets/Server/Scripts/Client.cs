using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;

public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;
    public int pingsMissed = 0;
    public bool isConnected = false;

    public string inHandItemId = "";

    public Client(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        public NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's TCP-related info.</summary>
        /// <param name="_socket">The TcpClient instance of the newly connected client.</param>
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Welcome to the server!");
        }

        /// <summary>Sends data to the client via TCP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); // Send data to appropriate client
                }
            }
            catch (Exception _ex)
            {
                ServerHandle.ConsoleWrite($"Error sending data to Player (ID: {id}) ({Server.clients[id].player.username}) [{Server.clients[id].tcp.socket.Client.RemoteEndPoint}] via TCP: {_ex}", ConsoleColor.Red);
                
            }
        }

        /// <summary>Reads incoming data from the stream.</summary>
        private void ReceiveCallback(IAsyncResult _result)
        {
            if(_result == null)
			{
                return;
			}

            try
            {
                if(stream == null)
				{
                    return;
				}

                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data)); // Reset receivedData if all data was handled
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                if (_ex.GetBaseException().HResult == -2147467259)
				{
                    Server.clients[id].Disconnect("Lost connection");
                }
				else
				{
                    ServerHandle.ConsoleWrite($"Error receiving TCP data: {_ex}", ConsoleColor.Red);
                    
                }
            }
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_data">The recieved data.</param>
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                // If client's received data contains a packet
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    // If packet contains no data
                    return true; // Reset receivedData instance to allow it to be reused
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                    }
                });

                _packetLength = 0; // Reset packet length
                if (receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains another packet
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true; // Reset receivedData instance to allow it to be reused
            }

            return false;
        }

        /// <summary>Closes and cleans up the TCP connection.</summary>
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's UDP-related info.</summary>
        /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_packetData">The packet containing the recieved data.</param>
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                }
            });
        }

        /// <summary>Cleans up the UDP connection.</summary>
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    /// <summary>Sends the client into the game and informs other clients of the new player.</summary>
    /// <param name="_playerName">The username of the new player.</param>
    public void SendIntoGame(string _playerName)
    {
        ServerSend.ChatMessageToAll(_playerName + " joined the game", new Color32(105, 255, 94, 255));
        player = new Player();
        player.Initialize(id, _playerName);

        foreach(Structure s in Server.structures.Values)
		{
            ServerSend.SpawnStructureForPlayer(s.id, s.location, s.rotation, s.uuid, id, s.world);
		}

        foreach (Entity s in Server.entities.Values)
        {
            //int playerInCharge = 1;

            //if (!Server.clients.ContainsKey(s.playerInCharge) || Server.clients.ContainsKey(s.playerInCharge) && !Server.clients[s.playerInCharge].isConnected)
            //{
            //    s.playerInCharge = id;
            //    ServerSend.PutPlayerInCharge(id, s.uuid);
            //    playerInCharge = id;
            //}

            ServerSend.SpawnEntityForPlayer(s.id, s.position, s.rotation, s.uuid, id, s.world, s.playerInCharge);
        }

        // Send all players to the new player
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        // Send the new player to all players (including himself)
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        // Send items to the new player
        foreach (string i in Server.items.Keys)
        {
            if (Server.items[i].onPlayer)
			{
                ServerSend.SpawnItem(Server.items[i].itemId, Server.clients[id].player.position, true, Server.items[i].player, i, Server.items[i].progressionPosition, Server.items[i].world, Server.items[i].syncronizedVariables.Values.ToList(), id);
            }
			else
			{
                ServerSend.SpawnItem(Server.items[i].itemId, Server.items[i].dropPosition, false, 0, i, Server.items[i].progressionPosition, Server.items[i].world, Server.items[i].syncronizedVariables.Values.ToList(), id);
            }
        }

        foreach(Client _client in Server.clients.Values)
		{
            if(_client.player != null)
			{
                if(_client.id != id)
				{
                    ServerSend.UsingHandState(id, _client.id, _client.player.handUsingState);
				}
			}
		}
    }

    /// <summary>Disconnects the client and stops all network traffic.</summary>
    public void Disconnect(string reason = "Disconnected")
    {
        foreach(string i in Server.items.Keys)
		{
            if(Server.items[i].onPlayer && Server.items[i].player == id)
			{
                //ServerHandle.ConsoleWrite(player.position);
                Server.items[i].dropPosition = player.position;
                Server.items[i].onPlayer = false;
                Server.items[i].player = 0;
                Server.items[i].progressionPosition = player.position;
            }
		}

        foreach(Entity e in Server.entities.Values.ToList())
		{
            if(e.playerInCharge == id)
			{
    //            int firstOnline = 1;

				//for (int i = 1; i < Server.MaxPlayers; i++)
				//{
				//	if (Server.clients[i].isConnected && i != id)
				//	{
				//		firstOnline = i;
				//		break;
				//	}
				//}

				e.playerInCharge = 0;
                ServerSend.PutPlayerInCharge(0, e.uuid);
            }
		}

        ServerHandle.ConsoleWrite($"Player (ID: {id}) ({player.username}) [{tcp.socket.Client.RemoteEndPoint}] has disconnected. Reason: {reason}", ConsoleColor.DarkRed);
        ServerSend.ChatMessageToAll(player.username + " left the game", new UnityEngine.Color32(255, 94, 102, 255));

        tcp.stream.Dispose();

        tcp.Disconnect();
        udp.Disconnect();

        isConnected = false;

        Server.clients[id] = new Client(id);
        player = new Player();

        ServerSend.PlayerDisconnected(id);
    }
}
