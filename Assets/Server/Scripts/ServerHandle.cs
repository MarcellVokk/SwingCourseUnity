using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

public class ServerHandle
{
    public static string curentInputText = "";
    public static List<ConsoleLine> consoleLines = new List<ConsoleLine>();
    
    public struct ConsoleLine
	{
        public string text { get; set; }
        public ConsoleColor color { get; set; }
    }
    
    public static void ConsoleWrite(string _text, ConsoleColor _textColor = ConsoleColor.White)
    {
        consoleLines.Add(new ConsoleLine { text = _text, color = _textColor });
    }

    public static string GetUsernameFromIndex(int _playerIndex)
	{
        if(Server.clients[_playerIndex] == null)
		{
            return "playerNotFound";
		}

        return Server.clients[_playerIndex].player.username;
	}

    public static int GetIndexFromUsername(string username)
    {
        int ret = -1;

        for(int i = 0; i < Server.clients.Count; i++)
		{
            if(Server.clients.ContainsKey(i) && Server.clients[i].player != null && Server.clients[i].player.username == username)
			{
                ret = i;
                break;
			}
		}

        return ret;
    }

    public static void HandleCommand(string command, Player.PlayerPermission permissions, int executerPlayerIndex)
    {
        string executerName = "";

        if (executerPlayerIndex == -1)
        {
            executerName = "Console";
            ServerHandle.ConsoleWrite("> " + command, ConsoleColor.Gray);
        }
        else
        {
            executerName = Server.clients[executerPlayerIndex].player.username;
            ServerHandle.ConsoleWrite(Server.clients[executerPlayerIndex].player.username + ": /" + command, ConsoleColor.White);
        }

        List<string> args = (command + " ").Split(' ').ToList();

        if (args.Count < 0)
        {
            return;
        }

        args.RemoveAt(args.Count - 1);

        try
        {
            if(args[0] == "stop")
            {
                FirebaseManagger.RemoveFromListThenQuit("Servers", Server.publicIp);
            }
            else if (args[0] == "tp")
            {
                if (!permissions.tp)
                {
                    throw new Exception("noperm");
                }

                ServerSend.TeleportPlayer(GetIndexFromUsername(args[1]), new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4])));
            }
            else if (args[0] == "spawn")
            {
                if (!permissions.spawn)
                {
                    throw new Exception("noperm");
                }

                if (args[1] == "item")
                {
                    Server.SpawnItem(args[2], new Vector3(float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5])), false, 0, Server.worlds[args[6]].id);
                }
                else if(args[1] == "structure")
				{
                    UnityEngine.Quaternion eulerToQuaternion = UnityEngine.Quaternion.Euler(float.Parse(args[6]), float.Parse(args[7]), float.Parse(args[8]));
                    Server.SpawnStructure(int.Parse(args[2]), new Vector3(float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5])), new Quaternion(eulerToQuaternion.x, eulerToQuaternion.y, eulerToQuaternion.z, eulerToQuaternion.w), Server.worlds[args[9]].id);
                }
                else if (args[1] == "entity")
                {
                    UnityEngine.Quaternion eulerToQuaternion = UnityEngine.Quaternion.Euler(float.Parse(args[6]), float.Parse(args[7]), float.Parse(args[8]));
                    Server.SpawnEntity(int.Parse(args[2]), new Vector3(float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5])), new Quaternion(eulerToQuaternion.x, eulerToQuaternion.y, eulerToQuaternion.z, eulerToQuaternion.w), Server.worlds[args[9]].id);
                }
            }
            else if (args[0] == "kill")
            {
                if (!permissions.kill)
                {
                    throw new Exception("noperm");
                }

                if (args[1] == "item")
                {
                    if (args[2] == "all")
                    {
                        foreach (string i in new List<string>(Server.items.Keys))
                        {
                            Server.KillItem(i);
                        }
                    }
                    else
                    {
                        Server.KillItem(args[2]);
                    }
                }
                else if (args[1] == "structure")
                {
                    if (args[2] == "all")
                    {
                        foreach (string i in new List<string>(Server.structures.Keys))
                        {
                            Server.DestroyStructure(i);
                        }
                    }
                    else
                    {
                        Server.DestroyStructure(args[2]);
                    }
                }
                else if (args[1] == "entity")
                {
                    if (args[2] == "all")
                    {
                        foreach (string i in new List<string>(Server.entities.Keys))
                        {
                            Server.KillEntity(i);
                        }
                    }
                    else
                    {
                        Server.KillEntity(args[2]);
                    }
                }
            }
            else if (args[0] == "give")
            {
                if (!permissions.give)
                {
                    throw new Exception("noperm");
                }

                Server.SpawnItem(args[2], Server.clients[GetIndexFromUsername(args[1])].player.position, true, GetIndexFromUsername(args[1]), Server.clients[GetIndexFromUsername(args[1])].player.world);
            }
            else if (args[0] == "help")
            {
                CommandOut("List of commands:", UnityEngine.Color.white);
                CommandOut("tp <player_name> <x> <y> <z> - Teleport player to location", UnityEngine.Color.white);
                CommandOut("", UnityEngine.Color.white);
                CommandOut("spawn <item> <item_name> <x> <y> <z> <world_name> - Spawn new item at location", UnityEngine.Color.white);
                CommandOut("spawn <structure> <structure_id> <x> <y> <z> <rotation_x> <rotation_y> <rotation_z> <world_name> - Spawn new structure at location", UnityEngine.Color.white);
                CommandOut("", UnityEngine.Color.white);
                CommandOut("kill <item|structure> <object_uuid> - Kill the object", UnityEngine.Color.white);
                CommandOut("", UnityEngine.Color.white);
                CommandOut("give <player_name> <item_name> - Give new item to player", UnityEngine.Color.white);
                CommandOut("", UnityEngine.Color.white);
                CommandOut("op <player_name> - Gives player permission to use any comand", UnityEngine.Color.white);
                CommandOut("deop <player_name> - Removes player permission to use any comand", UnityEngine.Color.white);
                CommandOut("", UnityEngine.Color.white);
                CommandOut("world tp <player_name> <world_name> - Teleports a player to a world", UnityEngine.Color.white);
                CommandOut("world get <player_name> - Get the world the player is curently in", UnityEngine.Color.white);
                CommandOut("world create <world_name> - Creates a new world", UnityEngine.Color.white);
                CommandOut("world save <world_name> <file_path> - Saves the world to a file", UnityEngine.Color.white);
                CommandOut("world load <file_path> - Loads a world from a file (creates/replaces a new world named the file name)", UnityEngine.Color.white);
            }
            else if (args[0] == "op")
            {
                if (!permissions.op)
                {
                    throw new Exception("noperm");
                }

                Server.clients[GetIndexFromUsername(args[1])].player.premissions = new Player.PlayerPermission(Player.PlayerPermission.defaultPermissions.op);
                CommandOut(GetIndexFromUsername(args[1]) + " has been made operator", UnityEngine.Color.white);
            }
            else if (args[0] == "deop")
            {
                if (!permissions.deop)
                {
                    throw new Exception("noperm");
                }

                Server.clients[GetIndexFromUsername(args[1])].player.premissions = new Player.PlayerPermission(Player.PlayerPermission.defaultPermissions.guest);
                CommandOut(GetIndexFromUsername(args[1]) + " is no longer an operator", UnityEngine.Color.white);
            }
            else if (args[0] == "world")
            {
                if (!permissions.world)
                {
                    throw new Exception("noperm");
                }

                if (args[1] == "tp")
                {
                    Server.ChangeWorld(GetIndexFromUsername(args[2]), Server.worlds[args[3]].id);
                }
                else if (args[1] == "create")
                {
                    Server.CreateWorld(args[2]);
                }
                else if (args[1] == "delete")
                {
                    Server.DeleteWorld(args[2]);
                }
                else if (args[1] == "get")
                {
                    World world = Server.GetWorld(Server.clients[GetIndexFromUsername(args[2])].player);
                    CommandOut(GetIndexFromUsername(args[2]) + " is in world: {id:" + world.id + ", name: " + world.name + "}", UnityEngine.Color.white);
                }
                else if (args[1] == "save")
                {
                    WorldManagger.SaveSerializationObjectToFile(args[3], WorldManagger.ConvertWorldToSerializationObjects(args[2]));
                }
                else if (args[1] == "load")
                {
                    SerializedWorld world = WorldManagger.ParseWorldFromFile(args[2]);
                    WorldManagger.LoadWorldFromSerializationObjects(world.worldObjects, world.worldName);
                }
            }
            else
            {
                CommandOut("There is no such command as: '" + args[0] + "'! Type 'help' for list of commands!", new UnityEngine.Color32(255, 94, 102, 255));
            }
        }
        catch (Exception ex)
        {
            if (ex.Message == "noperm")
            {
                ServerHandle.ConsoleWrite(executerName + " does not have the permissions to run this command!", ConsoleColor.DarkRed);
                
                CommandOut("You don't have the permissions to run this command", new UnityEngine.Color32(255, 94, 102, 255));
            }
            else
            {
                ServerHandle.ConsoleWrite("An error had occured while executing this command! Type 'help' for list of commands! " + ex.Message, ConsoleColor.DarkRed);
                
                CommandOut("An error had occured while executing this command! Type 'help' for list of commands!", new UnityEngine.Color32(255, 94, 102, 255));
            }
        }

        void CommandOut(string text, UnityEngine.Color color)
        {
            if (executerPlayerIndex == -1)
            {
                ConsoleColor c = FromColor(color);
                if(c == ConsoleColor.White)
				{
                    c = ConsoleColor.Gray;
				}
                ServerHandle.ConsoleWrite(text, FromColor(color));
            }
            else
            {
                ServerSend.ChatMessageTo(executerPlayerIndex, text, color);
            }
        }
    }

    public static ConsoleColor FromColor(UnityEngine.Color c)
    {
        UnityEngine.Color32 color = c;
        int index = (color.r > 128 | color.g > 128 | color.b > 128) ? 8 : 0; // Bright bit
        index |= (color.r > 64) ? 4 : 0; // Red bit
        index |= (color.g > 64) ? 2 : 0; // Green bit
        index |= (color.b > 64) ? 1 : 0; // Blue bit
        return (System.ConsoleColor)index;
    }

    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        ServerHandle.ConsoleWrite(ServerHandle.GetIndexFromUsername(_username).ToString(), ConsoleColor.White);

        if (ServerHandle.GetIndexFromUsername(_username) != -1)
        {
			var chars = Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 10);

			_username = new string(chars.SelectMany(str => str).OrderBy(c => Guid.NewGuid()).Take(8).ToArray());
		}

        ServerHandle.ConsoleWrite($"Player (ID: {_fromClient}) ({_username}) [{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint}] has connected.", ConsoleColor.DarkGreen);
        
        if (_fromClient != _clientIdCheck)
        {
            ServerHandle.ConsoleWrite($"Player (ID: {_fromClient}) ({_username}) has assumed the wrong client ID ({_clientIdCheck})!", ConsoleColor.Yellow);
            
        }

        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
		try
		{
            Vector3 playerPos = _packet.ReadVector3();
            Quaternion playerRotation = _packet.ReadQuaternion();
            Quaternion playerVerticalRotation = _packet.ReadQuaternion();
            bool isTeleport = _packet.ReadBool();
            bool isMounted = _packet.ReadBool();

            if (Server.clients.ContainsKey(_fromClient))
            {
                Server.clients[_fromClient].player.position = playerPos;
                Server.clients[_fromClient].player.rotation = playerRotation;
                Server.clients[_fromClient].player.mounted = isMounted;

                ServerSend.PlayerPosition(_fromClient, playerPos, playerRotation, playerVerticalRotation, isTeleport, isMounted);
                Server.clients[_fromClient].player.verticalRotation = playerVerticalRotation;
            }
        }
		catch (Exception)
		{
            return;
		}
    }

    public static void PingResponse(int _fromClient, Packet _packet)
	{
        Server.clients[_fromClient].pingsMissed = 0;
	}

    public static void PlayerPickupItem(int _fromClient, Packet _packet)
    {
        string itemId = _packet.ReadString();

        ServerSend.PlayerPickedupItem(_fromClient, itemId);

        Server.items[itemId].onPlayer = true;
        Server.items[itemId].player = _fromClient;

        Server.clients[_fromClient].inHandItemId = itemId;
    }

    public static void PlayerDroppItem(int _fromClient, Packet _packet)
    {
        Vector3 finalPos = _packet.ReadVector3();

        ServerSend.PlayerDroppedItem(_fromClient, finalPos);

        Server.items[Server.clients[_fromClient].inHandItemId].onPlayer = false;
        Server.items[Server.clients[_fromClient].inHandItemId].player = 0;
        Server.items[Server.clients[_fromClient].inHandItemId].dropPosition = finalPos;
        Server.items[Server.clients[_fromClient].inHandItemId].progressionPosition = finalPos;
    }

    public static void ReceiveUsingHandState(int _fromClient, Packet _packet)
    {
        int state = _packet.ReadInt();

        Server.clients[_fromClient].player.handUsingState = state;

        ServerSend.UsingHandState(_fromClient, state);
    }

    public static void ReceiveChatMessage(int _fromClient, Packet _packet)
	{
        string text = _packet.ReadString();

        if(text != "" && text != null)
		{
            if (text.First() == '/')
            {
                ServerHandle.HandleCommand(text.Remove(0, 1), Server.clients[_fromClient].player.premissions, _fromClient);
                //ServerSend.ChatMessageToAllExcept(_fromClient, Server.clients[_fromClient].player.username + ": " + text, UnityEngine.Color.white);
                //ServerSend.ChatMessageToAllExcept(_fromClient, Server.clients[_fromClient].player.username + ": " + text, UnityEngine.Color.white);
            }
            else
            {
                //ServerSend.ChatMessageToAllExcept(_fromClient, Server.clients[_fromClient].player.username + ": " + text, UnityEngine.Color.white);
                ServerSend.ChatMessageToAllExcept(_fromClient, Server.clients[_fromClient].player.username + ": " + text, UnityEngine.Color.white);
                ServerHandle.ConsoleWrite(Server.clients[_fromClient].player.username + ": " + text, ConsoleColor.White);
            }
        }
	}

    public static void JoinWorld(int _fromClient, Packet _packet)
    {
        int world = _packet.ReadInt();

        Server.SendToWorld(_fromClient, world);
    }

    public static void SetSyncVar(int _fromClient, Packet _packet)
	{
        string name = _packet.ReadString();
        string value = _packet.ReadString();
        string type = _packet.ReadString();

        string objectUuid = _packet.ReadString();
        Item item = null;

        Server.items.TryGetValue(objectUuid, out item);

        if(item == null)
		{
            return;
		}

        object finalValue = value;

        if(type == "int")
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

		if (Server.items[objectUuid].syncronizedVariables.ContainsKey(name))
		{
            Server.items[objectUuid].syncronizedVariables[name].Set(finalValue, type, name);
        }
		else
		{
            Server.items[objectUuid].syncronizedVariables.Add(name, new SyncronizedVariable(finalValue, type, name));
        }

        ServerSend.SetSyncVar(new SyncronizedVariable(finalValue, type, name), objectUuid, _fromClient);
	}

    public static void GetEntityTransform(int _fromClient, Packet _packet)
	{
        string uuid = _packet.ReadString();
        Vector3 position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();

		Server.entities[uuid].position = position;
		Server.entities[uuid].rotation = rotation;

		ServerSend.SendEntityTransform(uuid, position, rotation);
	}

    public static void PutPlayerInCharge(int _fromClient, Packet _packet)
    {
        int player = _packet.ReadInt();
        string uuid = _packet.ReadString();

        Server.entities[uuid].playerInCharge = player;

        ServerSend.PutPlayerInCharge(player, uuid);
    }

    public static void ParentEntityToPlayer(int _fromClient, Packet _packet)
    {
        int player = _packet.ReadInt();
        string uuid = _packet.ReadString();
        bool onOrOff = _packet.ReadBool();

        if (onOrOff)
        {
            Server.entities[uuid].childPlayer = player;
            Server.clients[player].player.attachedEntityUuid = uuid;
        }
        else
        {
            Server.entities[uuid].childPlayer = 0;
            Server.clients[player].player.attachedEntityUuid = "";
        }

        ServerSend.ParentEntityToPlayer(player, uuid, onOrOff);
    }
}
