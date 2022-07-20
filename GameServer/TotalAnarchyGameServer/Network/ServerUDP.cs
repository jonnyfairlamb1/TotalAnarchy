using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TotalAnarchyGameServer.Network {
    class ServerUDP {
        public static UdpClient serverListener;
        public static void InitializeNetwork() {
            serverListener = new UdpClient(5556);
            serverListener.BeginReceive(OnRecieve, null);
        }

        static void OnRecieve(IAsyncResult result) {
            try {
                IPEndPoint ipEndpoint = null;
                byte[] data = serverListener.EndReceive(result, ref ipEndpoint);
                HandleUDPData(data, ipEndpoint);
            } catch (Exception e) {
                Logger.WriteError("Player disconnected from UDP with error: " + e.ToString());
                //Client has disconnected
            }

            serverListener.BeginReceive(OnRecieve, null);
        }

        static void HandleUDPData(byte[] data, IPEndPoint endpoint) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            long packet = buffer.ReadLong();
            string split = buffer.ReadString();
            int connectionID = buffer.ReadInteger();
            ///reason for the client to default to connectionID 1000000
            for (int i = 0; i < General.client.Length; i++) {
                if (General.client[i].connectionID == connectionID && General.client[i].endpoint == null) {
                    General.client[i].endpoint = endpoint;
                    break; 
                }
            }
            ServerHandleData.HandleDataPackets((long)connectionID, buffer.ToArray());
        }


        public static void SendTo(IPEndPoint endpoint, byte[] data) {
            serverListener.Send(data, data.Length, endpoint);
        }

        public static void SendToAll(byte[] data) {
            for (int i = 0; i < General.client.Length; i++) {
                if (General.client[i].endpoint != null) {
                    SendTo(General.client[i].endpoint, data);
                }
            }
        }

        public static void SendToAllBut(int connectionID, byte[] data) {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++) {
                if (connectionID != i) {
                    if (General.client[i].endpoint != null) {
                        //Logger.WriteDebug("Sending Position Data To: " + i);
                        SendTo(General.client[i].endpoint, data);
                    }
                }
            }
        }
    }
}
