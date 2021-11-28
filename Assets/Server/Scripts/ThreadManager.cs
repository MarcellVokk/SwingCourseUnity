using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;
    [DllImport("Kernel32.dll", ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetConsoleSelectionInfo(out IntPtr lpConsoleSelectionInfo);


    bool updatingConsole = false;
    private void Update()
    {
        Console.Title = $"SwingCourse Server (running on port: [{Server.Port}]) ({(int)(1f / Time.unscaledDeltaTime)}/160 TPS)";

        UpdateMain();

		try
		{
            for(int i = 0; i < Server.items.Keys.Count; i++)
			{
                Item item = null; ;
                Server.items.TryGetValue(Server.items.Keys.ElementAt(i), out item);

                if (item != null)
                {
                    if (!item.onPlayer)
                    {
                        Vector3 down = Vector3.MoveTowards(new Vector3(item.progressionPosition.X, item.progressionPosition.Y, item.progressionPosition.Z), new Vector3(item.dropPosition.X, -2000f, item.dropPosition.Z), 5f * Time.deltaTime);
                        item.progressionPosition = new System.Numerics.Vector3(down.x, down.y, down.z);
                    }
                }
            }
        }
		catch (Exception)
		{

		}

        if(ServerHandle.consoleLines.Count > 0)
		{
            ServerHandle.ConsoleLine line = ServerHandle.consoleLines[0];
            //Console.ForegroundColor = line.color;
            //Console.WriteLine("");
            //         Console.ForegroundColor = line.color;
            //Console.Write("");
            ClearCurrentConsoleLine();
            Console.ForegroundColor = line.color;
            Console.WriteLine(line.text);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("> " + ServerHandle.curentInputText);
            ServerHandle.consoleLines.RemoveAt(0);
        }
    }

    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    private void Start()
	{
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 160;
    }

	/// <summary>Sets an action to be executed on the main thread.</summary>
	/// <param name="_action">The action to be executed on the main thread.</param>
	public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            ServerHandle.ConsoleWrite("No action to execute on main thread!", ConsoleColor.White);
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
}
