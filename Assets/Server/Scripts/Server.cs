using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;
using System.Linq;
using System.Numerics;

public class SyncronizedVariable
{
    public string name = "";
    public object value = null;
    public string type = "";

    public void Set(object _value, string _type = "", string _name = "")
    {
        value = _value;

        if (_type != "")
        {
            type = _type;
        }

        if (_name != "")
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

public class Item
{
    public Vector3 dropPosition;
    public Vector3 progressionPosition;
    public bool onPlayer;
    public int player;
    public string itemId;
    public int world;
    public string uuid;
    public Dictionary<string, SyncronizedVariable> syncronizedVariables = new Dictionary<string, SyncronizedVariable>();
}

public class Structure
{
    public Vector3 location;
    public Quaternion rotation;
    public int id;
    public string uuid;
    public int world;
}

public class World
{
    public int id;
    public string name;
}

public class Entity
{
    public string uuid;
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public int world;
    public int playerInCharge;
    public int childPlayer;
}

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; } = -1;
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public static Dictionary<string, Item> items = new Dictionary<string, Item>();
    public static Dictionary<string, Structure> structures = new Dictionary<string, Structure>();
    public static Dictionary<string, World> worlds = new Dictionary<string, World>();
    public static Dictionary<string, Entity> entities = new Dictionary<string, Entity>();

    public static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static IPAddress GetDefaultGateway()
    {
        var gateway_address = NetworkInterface.GetAllNetworkInterfaces()
            .Where(e => e.OperationalStatus == OperationalStatus.Up)
            .SelectMany(e => e.GetIPProperties().GatewayAddresses)
            .FirstOrDefault();
        if (gateway_address == null) return null;
        return gateway_address.Address;
    }

    public static void SendToWorld(int player, int world)
	{
        foreach (Structure s in Server.structures.Values)
        {
            if(s.world == world)
			{
                ServerSend.SpawnStructureForPlayer(s.id, s.location, s.rotation, s.uuid, player, world);
            }
        }

        foreach (Entity s in Server.entities.Values)
        {
            if (s.world == world)
            {
                int playerInCharge = s.playerInCharge;

                //if (!Server.clients.ContainsKey(s.playerInCharge) || Server.clients.ContainsKey(s.playerInCharge) && !Server.clients[s.playerInCharge].isConnected)
                //{
                //    s.playerInCharge = player;
                //    ServerSend.PutPlayerInCharge(player, s.uuid);
                //    playerInCharge = player;
                //}

                ServerSend.SpawnEntityForPlayer(s.id, s.position, s.rotation, s.uuid, player, world, playerInCharge);
            }
        }

        // Send all players to the new player
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.player.world == world && _client.id != player)
                {
                    ServerSend.SpawnPlayer(player, _client.player);
                }
            }
        }

        // Send the new player to all players
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if(_client.player.world == world && _client.id != player)
				{
                    ServerSend.SpawnPlayer(_client.id, Server.clients[player].player);
                }
            }
        }

        // Send items to the new player
        foreach (string i in Server.items.Keys)
        {
            if(Server.items[i].world == world)
			{
                if (Server.items[i].onPlayer)
                {
                    ServerSend.SpawnItem(Server.items[i].itemId, Server.clients[player].player.position, true, Server.items[i].player, i, Server.items[i].progressionPosition, Server.items[i].world, Server.items[i].syncronizedVariables.Values.ToList(), player);
                }
                else
                {
                    ServerSend.SpawnItem(Server.items[i].itemId, Server.items[i].dropPosition, false, 0, i, Server.items[i].progressionPosition, Server.items[i].world, Server.items[i].syncronizedVariables.Values.ToList(), player);
                }
            }
        }

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != player)
                {
                    if(_client.player.world == world)
					{
                        ServerSend.UsingHandState(player, _client.id, _client.player.handUsingState);
                    }
                }
            }
        }

        ServerSend.ShowWorld(player);
    }

    public static World GetWorld(Item item)
	{
        return Server.worlds.Where(z => z.Value.id == item.world).FirstOrDefault().Value;
    }

    public static World GetWorld(Structure structure)
    {
        return Server.worlds.Where(z => z.Value.id == structure.world).FirstOrDefault().Value;
    }

    public static World GetWorld(Player player)
    {
        return Server.worlds.Where(z => z.Value.id == player.world).FirstOrDefault().Value;
    }

    public static World GetWorld(int id)
	{
        return Server.worlds.Where(z => z.Value.id == id).FirstOrDefault().Value;
    }

    public static int CreateWorld(string name)
	{
        int id = Server.worlds.Values.Count + 1;
        Server.worlds.Add(name, new World { id = id, name = name });
        return id;
    }

    public static void DeleteWorld(string name)
    {
        if(name == "default")
		{
            return;
		}

        foreach(Client c in Array.FindAll(Server.clients.Values.ToArray(), x => x.player != null && x.player.world == Server.worlds[name].id))
		{
            Server.ChangeWorld(c.player.id, 0);
        }

        foreach (Structure s in Array.FindAll(Server.structures.Values.ToArray(), x => x.world == Server.worlds[name].id))
        {
            structures.Remove(s.uuid);
        }

        foreach (Item s in Array.FindAll(Server.items.Values.ToArray(), x => x.world == Server.worlds[name].id))
        {
            items.Remove(s.uuid);
        }

        Server.worlds.Remove(name);
    }

    public static void KillEntity(string uuid)
    {
        ServerSend.KillEntity(uuid);
        Server.entities.Remove(uuid);
    }

    public static string SpawnItem(string itemId, Vector3 location, bool onPlayer, int player, int world, string uuid = "", float _progressionPositionX = 0f, float _progressionPositionY = 0f, float _progressionPositionZ = 0f, bool customProgProc = false, Dictionary<string, SyncronizedVariable> syncVars = null)
	{
        string myuuidAsString = "";

        if (uuid == "")
		{
            Guid myuuid = Guid.NewGuid();
            myuuidAsString = myuuid.ToString();
        }
		else
		{
            myuuidAsString = uuid;
		}

        string itemIndex = myuuidAsString;
        ServerHandle.ConsoleWrite("Item uuid: " + itemIndex.ToString(), ConsoleColor.White);

        Vector3 _progressionPosition = location;

		if (customProgProc)
		{
            _progressionPosition = new Vector3(_progressionPositionX, _progressionPositionY, _progressionPositionZ);
		}

        Dictionary<string, SyncronizedVariable> finalSyncVars = new Dictionary<string, SyncronizedVariable>();

        if(syncVars != null)
		{
            finalSyncVars = syncVars;
		}
        
        items.Add(itemIndex, new Item
        {
            dropPosition = location,
            itemId = itemId,
            progressionPosition = _progressionPosition,
            world = world,
            uuid = myuuidAsString,
            syncronizedVariables = finalSyncVars
        });

        ServerSend.SpawnItem(itemId, location, onPlayer, player, itemIndex, location, world, finalSyncVars.Values.ToList());

        return itemIndex;
	}

    public static string SpawnStructure(int structureId, Vector3 location, Quaternion rotation, int world, string uuid = "")
    {
        string structureUuid;

        if(uuid == "")
		{
            Guid myuuid = Guid.NewGuid();
            structureUuid = myuuid.ToString();
        }
		else
		{
            structureUuid = uuid;
		}

        ServerHandle.ConsoleWrite("Structure uuid: " + structureUuid.ToString(), ConsoleColor.White);

        structures.Add(structureUuid, new Structure { location = location, rotation = rotation, id = structureId, uuid = structureUuid, world = world });

        ServerSend.SpawnStructure(structureId, location, rotation, structureUuid, world);

        return structureUuid;
    }

    public static string SpawnEntity(int entityId, Vector3 location, Quaternion rotation, int world, string uuid = "")
    {
        string entitiUuid;

        if (uuid == "")
        {
            Guid myuuid = Guid.NewGuid();
            entitiUuid = myuuid.ToString();
        }
        else
        {
            entitiUuid = uuid;
        }

        ServerHandle.ConsoleWrite("Entity uuid: " + entitiUuid.ToString(), ConsoleColor.White);

        entities.Add(entitiUuid, new Entity { position = location, rotation = rotation, id = entityId, uuid = entitiUuid, world = world, playerInCharge = 0 });

        ServerSend.SpawnEntity(entityId, location, rotation, entitiUuid, world, 0);

        return entitiUuid;
    }

    public static void DestroyStructure(string uuid)
    {
        ServerSend.DestroyStructure(uuid);
        structures.Remove(uuid);
    }

    public static void KillItem(string itemIndex)
    {
        items.Remove(itemIndex);
        ServerSend.KillItem(itemIndex);
    }

    public static void ChangeWorld(int playerId, int newWorld)
	{
        if(Server.clients[playerId].inHandItemId != null && Server.clients[playerId].inHandItemId != "")
		{
            Server.KillItem(Server.clients[playerId].inHandItemId);
            Server.clients[playerId].inHandItemId = null;
        }

        ServerSend.RemoveAvatar(playerId, Server.clients[playerId].player.world);
        ServerSend.ChangeWorld(playerId, newWorld);
        Server.clients[playerId].player.world = newWorld;
	}

    public static string publicIp = "";

    public static void Start(int _maxPlayers, int _port)
    {
        worlds.Add("default", new World { id = 0, name = "default" });

        MaxPlayers = _maxPlayers;
        Port = _port;

        string localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

        Console.Clear();

        ServerHandle.ConsoleWrite("Starting server...", ConsoleColor.White);
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        ServerHandle.ConsoleWrite("", ConsoleColor.White);

        ServerHandle.ConsoleWrite($"Make sure to enable port forwarding on this port! Default gateway [{GetDefaultGateway()}]", ConsoleColor.DarkYellow);
        ServerHandle.ConsoleWrite($"Port to forward: {Port}, IP address to forward: {localIp}", ConsoleColor.DarkYellow);
        ServerHandle.ConsoleWrite("NOTE: You most likely will have to wait at least 5-10 minutes before the port forward takes effect!", ConsoleColor.Blue);
        ServerHandle.ConsoleWrite("NOTE: To connect to the server, get the servers PUBLIC IP address! Get your public ip: https://ipinfo.io/ip", ConsoleColor.Blue);
        ServerHandle.ConsoleWrite("WARNING: When the window is in selection mode, you won't see new server output!.", ConsoleColor.DarkYellow);

        ServerHandle.ConsoleWrite("", ConsoleColor.White);

        ServerHandle.ConsoleWrite($"Server started on port: [{Port}].", ConsoleColor.Green);

        ServerHandle.ConsoleWrite("", ConsoleColor.White);

        Console.Title = $"SwingCourse Server (running on port: [{Port}]) (160/160 TPS)";

        FirebaseManagger.GetStringData("ipCheckService", result =>
        {
            publicIp = GetPublicIp(result).ToString();
            FirebaseManagger.AddToList("Servers", publicIp);
        });
    }

    static System.Net.IPAddress GetPublicIp(string serviceUrl)
    {
        return System.Net.IPAddress.Parse(new System.Net.WebClient().DownloadString(serviceUrl));
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        ServerHandle.ConsoleWrite($"Incoming connection from {_client.Client.RemoteEndPoint}...", ConsoleColor.DarkBlue);


        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                clients[i].isConnected = true;
                clients[i].pingsMissed = 0;

                //System.Net.EndPoint ep = clients[i].tcp.socket.Client.RemoteEndPoint;
                //System.Net.IPEndPoint ip = (System.Net.IPEndPoint)ep;
                //ServerHandle.ConsoleWrite(ip.Address.ToString());

                return;
            }
        }
        ServerHandle.ConsoleWrite($"{_client.Client.RemoteEndPoint} failed to connect: Server full!", ConsoleColor.Red);
        
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        int id = 0;
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                id = _clientId;

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            ServerHandle.ConsoleWrite($"Error receiving UDP data: {_ex}", ConsoleColor.Red);
            
            clients[id].Disconnect();
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            ServerHandle.ConsoleWrite($"Error sending data to {_clientEndPoint} via UDP: {_ex}", ConsoleColor.Red);
            
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.pingResponse, ServerHandle.PingResponse },
            { (int)ClientPackets.playerPickupItem, ServerHandle.PlayerPickupItem },
            { (int)ClientPackets.playerDroppItem, ServerHandle.PlayerDroppItem },
            { (int)ClientPackets.receiveUsingHandState, ServerHandle.ReceiveUsingHandState },
            { (int)ClientPackets.chatMessage, ServerHandle.ReceiveChatMessage },
            { (int)ClientPackets.joinWorld, ServerHandle.JoinWorld },
            { (int)ClientPackets.setSyncVar, ServerHandle.SetSyncVar },
            { (int)ClientPackets.entityTransform, ServerHandle.GetEntityTransform },
            { (int)ClientPackets.putPlayerInCharge, ServerHandle.PutPlayerInCharge },
            { (int)ClientPackets.parentEntityToPlayer, ServerHandle.ParentEntityToPlayer },
        };
        ServerHandle.ConsoleWrite("Initialized packets.", ConsoleColor.White);
    }

    public static void Stop()
    {
        try
		{
            udpListener.Close();
        }
		catch (Exception) { }

		try
		{
            tcpListener.Stop();
        }
		catch (Exception) { }
    }
}
