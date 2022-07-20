using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Helpers;
using DatabaseServer.Player;

namespace DatabaseServer.Network {
    class ServerTCP {
        public static TcpListener serverSocket;

        public static void InitializeNetwork() {
            serverSocket = new TcpListener(IPAddress.Any, 5557);
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(OnClientConnect, null);
        }

        static void OnClientConnect(IAsyncResult result) {
            TcpClient client = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(OnClientConnect, null);
            for (int i = 0; i < Helpers.Constants.MAX_GAME_SERVERS; i++) {
                if (Helpers.General.client[i].socket == null) {
                    Helpers.General.client[i] = new Clients();
                    Helpers.General.client[i].socket = client;
                    Helpers.General.client[i].connectionID = i;
                    Helpers.General.client[i].ip = client.Client.RemoteEndPoint.ToString();

                    Helpers.General.client[i].Start();

                    SendInGame(Helpers.General.client[i]);
                    //SendInWorld(General.client[i]);
                    Helpers.Logger.WriteLog("Connection received from " + Helpers.General.client[i].ip + " | ConnectionID: " + Helpers.General.client[i].connectionID);
                    return;
                }
            }
        }

        public static void SendDataTo(int connectionID, byte[] data) {
            try {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                buffer.WriteBytes(data);
                Helpers.General.client[connectionID].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                buffer.Dispose();
            } catch (Exception e) {
                return;
            }
        }

        public static void SendInGame(Clients client) {
            //ByteBuffer buffer = new ByteBuffer();
            //buffer.WriteLong((long)DatabaseServerPackets.DS_ConnectionID); //Packet Identifier.
            //buffer.WriteInteger(client.connectionID);

            //SendDataTo(client.connectionID, buffer.ToArray());
            //buffer.Dispose();
        }



        #region SendPackages

        public static void UsernameExists(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.DB_UsernameExistsError);
            SendDataTo(connectionID, buffer.ToArray());
        }
        public static void EmailExists(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.DB_EmailExistsError);
            SendDataTo(connectionID, buffer.ToArray());
        }
        public static void IncorrectLoginDetails(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.DB_IncorrectLoginDetails);
            SendDataTo(connectionID, buffer.ToArray());
        }
        public static void LoginConfirmed(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.DB_ConfirmLoginDetails);
            SendDataTo(connectionID, buffer.ToArray());
        }
        #endregion
    }
}

