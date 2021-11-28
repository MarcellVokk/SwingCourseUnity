using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SerializationAttribute
{
	public string propertyName = "";
	public object property = "";
	public string propertyType = "";
}

public class SerializationObject
{
	public List<SerializationAttribute> objectAttributes = new List<SerializationAttribute>();
	public string objectType = "";
}

public class SerializedWorld
{
	public List<SerializationObject> worldObjects = new List<SerializationObject>();
	public string worldName = "";
}

public class WorldManagger
{
	public static void LoadWorldFromSerializationObjects(List<SerializationObject> serializationObjects, string worldName)
	{
		try
		{
			Server.DeleteWorld(worldName);
		}
		catch (Exception)
		{

		}

		Server.CreateWorld(worldName);

		foreach (SerializationObject so in serializationObjects)
		{
			Dictionary<string, SerializationAttribute> attributes = new Dictionary<string, SerializationAttribute>();

			foreach(SerializationAttribute sa in so.objectAttributes)
			{
				attributes.Add(sa.propertyName, sa);
			}

			if (so.objectType == "item")
			{
				Dictionary<string, SyncronizedVariable> syncVars = new Dictionary<string, SyncronizedVariable>();
				List<string> reservedVars = new List<string> { "itemId", "dropX", "dropY", "dropZ", "onPlayer", "player", "uuid", "progX", "progY", "progZ" };

				foreach(SerializationAttribute sa in Array.FindAll(attributes.Values.ToArray(), x => !reservedVars.Contains(x.propertyName)))
				{
					syncVars.Add(sa.propertyName, new SyncronizedVariable(sa.property, sa.propertyType, sa.propertyName));
				}

				Server.SpawnItem((string)attributes["itemId"].property, new System.Numerics.Vector3((float)attributes["dropX"].property, (float)attributes["dropY"].property, (float)attributes["dropZ"].property), (bool)attributes["onPlayer"].property, (int)attributes["player"].property, Server.worlds[worldName].id, (string)attributes["uuid"].property, (float)attributes["progX"].property, (float)attributes["progY"].property, (float)attributes["progZ"].property, true, syncVars);
			}
			else if (so.objectType == "structure")
			{
				Server.SpawnStructure((int)attributes["id"].property, new System.Numerics.Vector3((float)attributes["locX"].property, (float)attributes["locY"].property, (float)attributes["locZ"].property), new System.Numerics.Quaternion((float)attributes["rotX"].property, (float)attributes["rotY"].property, (float)attributes["rotZ"].property, (float)attributes["rotW"].property), Server.worlds[worldName].id, (string)attributes["uuid"].property);
			}
			else if (so.objectType == "entity")
			{
				Server.SpawnEntity((int)attributes["id"].property, new System.Numerics.Vector3((float)attributes["locX"].property, (float)attributes["locY"].property, (float)attributes["locZ"].property), new System.Numerics.Quaternion((float)attributes["rotX"].property, (float)attributes["rotY"].property, (float)attributes["rotZ"].property, (float)attributes["rotW"].property), Server.worlds[worldName].id, (string)attributes["uuid"].property);
			}
		}
	}

	public static SerializedWorld ParseWorldFromFile(string path)
	{
		List<SerializationObject> world = new List<SerializationObject>();

		string fileContent = "";
		using (StreamReader sr = new StreamReader(path))
		{
			fileContent = sr.ReadToEnd();
		}

		List<char> chars = fileContent.Replace("\r\n", "").ToCharArray().ToList();
		List<string> serializationObjects = new List<string>();

		string apend = "";
		bool startedParagraph = false;
		for(int i = 0; i < chars.Count; i++)
		{
			if (!startedParagraph)
			{
				if (chars[i] == '{')
				{
					startedParagraph = true;
					continue;
				}
			}
			else
			{
				if (chars[i] == '}')
				{
					if(apend != "")
					{
						serializationObjects.Add(apend);
					}

					apend = "";

					startedParagraph = false;
					continue;
				}
				else
				{
					apend += chars[i].ToString();
				}
			}
		}

		foreach(string s in serializationObjects)
		{
			SerializationObject newSo = new SerializationObject { objectType = (s + ";").Split(';')[1] };
			List<string> attributes = new List<string>();

			List<char> charsSo = (s + ";").Split(';')[0].ToCharArray().ToList();

			string apendSo = "";
			bool startedParagraphSo = false;
			for(int i = 0; i < charsSo.Count; i++)
			{
				if (!startedParagraphSo)
				{
					if(charsSo[i] == '(')
					{
						startedParagraphSo = true;
						continue;
					}
				}
				else
				{
					if (charsSo[i] == ')')
					{
						if (apendSo != "")
						{
							attributes.Add(apendSo);
						}

						apendSo = "";

						startedParagraphSo = false;
						continue;
					}
					else
					{
						apendSo += charsSo[i].ToString();
					}
				}
			}

			foreach (string sa in attributes)
			{
				string name = (sa + ",").Split(',')[0];
				string value = (sa + ",").Split(',')[1];
				string type = (sa + ",").Split(',')[2];

				object realValue = null;

				if(type == "string")
				{
					realValue = value.ToString();
				}
				else if (type == "int")
				{
					realValue = int.Parse(value);
				}
				else if (type == "float")
				{
					realValue = float.Parse(value);
				}
				else if (type == "bool")
				{
					realValue = bool.Parse(value);
				}

				newSo.objectAttributes.Add(new SerializationAttribute { propertyName = name, property = realValue, propertyType = type });
			}

			world.Add(newSo);
		}

		return new SerializedWorld { worldObjects = world, worldName = Path.GetFileNameWithoutExtension(path) };
	}

	public static void SaveSerializationObjectToFile(string path, List<SerializationObject> objects)
	{
		string apend = "";
		foreach(SerializationObject s in objects)
		{
			string apendLocal = "";

			foreach (SerializationAttribute sa in s.objectAttributes)
			{
				apendLocal += "(" + sa.propertyName + "," + sa.property.ToString().Replace(",", ".") + "," + sa.propertyType + ")";
			}

			if(apend != "")
			{
				apend += "\r\n";
			}

			apend += "{" + apendLocal + ";" + s.objectType + "}";
		}

		using(StreamWriter s = new StreamWriter(path))
		{
			s.Write(apend);
		}
	}

	public static List<SerializationObject> ConvertWorldToSerializationObjects(string worldName)
	{
		World world = Server.worlds[worldName];
		List<SerializationObject> result = new List<SerializationObject>();

		foreach (Structure s in Array.FindAll(Server.structures.Values.ToArray(), x => x.world == world.id))
		{
			SerializationObject o = new SerializationObject { objectType = "structure" };

			o.objectAttributes.Add(new SerializationAttribute { property = s.location.X, propertyName = "locX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.location.Y, propertyName = "locY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.location.Z, propertyName = "locZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.W, propertyName = "rotW", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.X, propertyName = "rotX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.Y, propertyName = "rotY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.Z, propertyName = "rotZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.id, propertyName = "id", propertyType = "int" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.uuid, propertyName = "uuid", propertyType = "string" });

			result.Add(o);
		}

		foreach (Entity s in Array.FindAll(Server.entities.Values.ToArray(), x => x.world == world.id))
		{
			SerializationObject o = new SerializationObject { objectType = "entity" };

			o.objectAttributes.Add(new SerializationAttribute { property = s.position.X, propertyName = "locX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.position.Y, propertyName = "locY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.position.Z, propertyName = "locZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.W, propertyName = "rotW", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.X, propertyName = "rotX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.Y, propertyName = "rotY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.rotation.Z, propertyName = "rotZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.id, propertyName = "id", propertyType = "int" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.uuid, propertyName = "uuid", propertyType = "string" });

			result.Add(o);
		}

		foreach (Item s in Array.FindAll(Server.items.Values.ToArray(), x => x.world == world.id))
		{
			if(s.onPlayer == true)
			{
				continue;
			}

			SerializationObject o = new SerializationObject { objectType = "item" };

			o.objectAttributes.Add(new SerializationAttribute { property = s.dropPosition.X, propertyName = "dropX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.dropPosition.Y, propertyName = "dropY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.dropPosition.Z, propertyName = "dropZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.progressionPosition.X, propertyName = "progX", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.progressionPosition.Y, propertyName = "progY", propertyType = "float" });
			o.objectAttributes.Add(new SerializationAttribute { property = s.progressionPosition.Z, propertyName = "progZ", propertyType = "float" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.itemId, propertyName = "itemId", propertyType = "string" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.onPlayer, propertyName = "onPlayer", propertyType = "bool" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.player, propertyName = "player", propertyType = "int" });

			o.objectAttributes.Add(new SerializationAttribute { property = s.uuid, propertyName = "uuid", propertyType = "string" });

			foreach(SyncronizedVariable sv in s.syncronizedVariables.Values)
			{
				o.objectAttributes.Add(new SerializationAttribute { property = sv.value, propertyName = sv.name, propertyType = sv.type });
			}

			result.Add(o);
		}

		return result;
	}
}
