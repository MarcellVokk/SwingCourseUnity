using System;
using System.Threading;
using UnityEngine;

namespace SwingCourseServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title += $"SwingCourse Server (running on port: [{Server.Port}]) (160/160 TPS)";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 4853);
        }

        private static void MainThread()
        {
            ServerHandle.ConsoleWrite($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.", ConsoleColor.White);
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
