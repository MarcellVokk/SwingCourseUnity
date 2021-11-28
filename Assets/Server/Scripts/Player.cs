using System.Linq;
using System.Numerics;

public class Player
{
    public int id;
    public string username;
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion verticalRotation;
    public int handUsingState = 0;
    public PlayerPermission premissions;
    public int world = 0;
    public bool mounted = false;
    public string attachedEntityUuid = "";

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
    }

    public World GetWorld()
	{
        return Server.worlds.Where(z => z.Value.id == world).FirstOrDefault().Value;
    }

    public struct PlayerPermission
	{
        public bool tp;
        public bool spawn;
        public bool kill;
        public bool give;
        public bool op;
        public bool deop;
        public bool world;

        public enum defaultPermissions
		{
            guest,
            op
		}

        public PlayerPermission(defaultPermissions defaultPermission)
		{
            if(defaultPermission == defaultPermissions.op)
			{
                tp = true;
                spawn = true;
                kill = true;
                give = true;
                op = true;
                deop = true;
                world = true;
            }
			else
			{
                tp = false;
                spawn = false;
                kill = false;
                give = false;
                op = false;
                deop = false;
                world = false;
            }
		}
    }
}
