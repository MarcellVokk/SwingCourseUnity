using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

static class DisableConsoleQuickEdit
{
    const uint ENABLE_QUICK_EDIT = 0x0040;

    // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
    const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    internal static bool Go()
    {
        IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

        // get current console mode
        uint consoleMode;
        if (!GetConsoleMode(consoleHandle, out consoleMode))
        {
            // ERROR: Unable to get console mode.
            return false;
        }

        // Clear the quick edit bit in the mode flags
        consoleMode &= ~ENABLE_QUICK_EDIT;

        // set the new mode
        if (!SetConsoleMode(consoleHandle, consoleMode) || !SetConsoleMode(consoleHandle, 0x0080))
        {
            // ERROR: Unable to set console mode
            return false;
        }

        return true;
    }
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public Coroutine ping;

    private void Awake()
    {
        //DisableConsoleQuickEdit.Go();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

	public void StopPing()
	{
        StopCoroutine(ping);
	}

    IEnumerator Ping()
    {
        for (; ; )
        {
            ServerSend.Ping();

            foreach(Client c in new List<Client>(Server.clients.Values))
			{
                if (c.isConnected)
				{
                    c.pingsMissed += 1;

                    if (c.pingsMissed >= 7)
                    {
                        c.Disconnect("Timeout");
                    }
                }
			}

            yield return new WaitForSeconds(3f);
        }
    }

	private void Start()
    {
        //Server.Start(50, 21663);
        Server.Start(50, 4853);
        ping = StartCoroutine(Ping());

        ConsoleInput();

        Application.wantsToQuit += Application_wantsToQuit;
    }

    public void ConsoleInput()
	{
        Task.Factory.StartNew(() =>
        {
			while (true)
			{
                ConsoleKeyInfo key = Console.ReadKey();

                if(key.Key == ConsoleKey.Backspace)
				{
                    if(ServerHandle.curentInputText.Length > 0)
					{
                        Console.Write("\b \b");
                        ServerHandle.curentInputText = ServerHandle.curentInputText.Remove(ServerHandle.curentInputText.Length - 1);
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    string cit = ServerHandle.curentInputText;
                    ServerHandle.curentInputText = "";
                    ServerHandle.HandleCommand(cit, new Player.PlayerPermission(Player.PlayerPermission.defaultPermissions.op), -1);
				}
                else if(key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.Tab || key.Key == ConsoleKey.LeftWindows || key.Key == ConsoleKey.RightWindows)
				{

				}
                else
				{
                    Console.Write(key.KeyChar);
                    ServerHandle.curentInputText += key.KeyChar.ToString();
                }
            }
        });
    }

    public static bool allowQuit = false;

    private void OnApplicationQuit()
    {
        Server.Stop();
        Console.Clear();
        FirebaseManagger.RemoveFromListThenQuit("Servers", Server.publicIp);
    }

    private bool Application_wantsToQuit()
    {
        //Application.wantsToQuit -= Application_wantsToQuit;
        return false;
    }
}
