using System;
using System.Threading;
using System.Reflection;
using DatabaseServer;
using DatabaseServer.Network;
namespace DatabaseServer {
    class Program {
        private static Thread threadConsole;

        static void Main(string[] args) {
                threadConsole = new Thread(new ThreadStart(ConsoleThread));
                threadConsole.Start();
                Helpers.General.InitializeServer();
        }

            static void ConsoleThread() {
                string line; string[] parts;
                int countdown = 5;
                while (true) {
                    line = Console.ReadLine();
                    parts = line.Split(' ');
                    switch (parts[0]) {
                        case "/Restart":
                        Helpers.Logger.WriteLog("Restarting Server");
                            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().CodeBase);
                            Environment.Exit(0);
                            break;
                        case "/restart":
                        Helpers.Logger.WriteLog("Restarting Server in: ");

                            for (int i = 0; i < 5; i++) {
                            Helpers.Logger.WriteLog(countdown.ToString());
                                countdown--;
                                Thread.Sleep(1000);
                            }

                            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().CodeBase);
                            Environment.Exit(0);
                            break;
                        case "/Clear":
                            Console.Clear();
                            break;
                        case "/clear":
                            Console.Clear();
                            break;

                     
                    }

                }
            }
        }
    }
