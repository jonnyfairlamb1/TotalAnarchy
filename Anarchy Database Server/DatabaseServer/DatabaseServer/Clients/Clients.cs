using System;
using System.Net.Sockets;
using System.Net;

using DatabaseServer.Network;

namespace DatabaseServer.Player {
    public class Clients {
        public int connectionID = 100000; //ghetto to make the movement work
        public string username;
        public string ip;
        public TcpClient socket;
        public IPEndPoint endpoint;
        public NetworkStream myStream;
        private byte[] readBuff;
        public ByteBuffer playerBuffer;

        public void Start() {
            socket.SendBufferSize = 4096;
            socket.ReceiveBufferSize = 4096;
            myStream = socket.GetStream();
            readBuff = new byte[4096];
            myStream.BeginRead(readBuff, 0, socket.ReceiveBufferSize, OnReceiveData, null);
        }

        private void OnReceiveData(IAsyncResult result) {
            try {
                int readbytes = myStream.EndRead(result);
                if (readbytes <= 0) {
                    //client is not connected to the server anymore
                    CloseSocket();
                    return;
                }
                byte[] newBytes = new byte[readbytes];
                Buffer.BlockCopy(readBuff, 0, newBytes, 0, readbytes);
                ServerHandleData.HandleData(connectionID, newBytes);
                myStream.BeginRead(readBuff, 0, socket.ReceiveBufferSize, OnReceiveData, null);

            } catch (Exception e) {
                Helpers.Logger.WriteError("Closing Socket Due to Error: " + e.ToString());
                CloseSocket();
            }
        }

        public void CloseSocket() {
            if (socket != null) { //Checks to see if the player has already disconnected -- more duplication issues
                Helpers.Logger.WriteLog("Connection from " + ip + " has been terminated");     

                socket.Close();
                socket = null;
            }
        }
    }
}

