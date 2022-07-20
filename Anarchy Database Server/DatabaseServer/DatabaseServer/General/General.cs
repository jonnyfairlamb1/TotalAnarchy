using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DatabaseServer.Network;
using DatabaseServer.Player;
using System.Data;
using System.Data.SqlClient;


namespace Helpers {
    class General{
        public static Clients[] client = new Clients[Constants.MAX_GAME_SERVERS];
        public static Thread databaseThread;
        public static int GetTickCount(){
            return Environment.TickCount;
        }

        public static void InitializeServer(){
            int startTime = 0; int endTime = 0;
            startTime = GetTickCount();
            Helpers.Logger.WriteInfo("Initializing Server...");

            databaseThread = new Thread(Database.InitializeDatabase);
            databaseThread.Start();
            //Intializing all game data arrays
            Helpers.Logger.WriteInfo("Initializing Game Arrays...");
            for (int i = 0; i < Constants.MAX_GAME_SERVERS; i++){
                client[i] = new Clients();
            }

            //Start the Networking
            Helpers.Logger.WriteInfo("Initializing Network...");
            ServerHandleData.InitializePackets();
            ServerTCP.InitializeNetwork();

            

            endTime = GetTickCount();
            Helpers.Logger.WriteLog("Initialization complete. Server loaded in " + (endTime - startTime) + " ms.");
        }
    }
}
