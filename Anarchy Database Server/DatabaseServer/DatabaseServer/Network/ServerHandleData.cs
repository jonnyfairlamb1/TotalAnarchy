using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Helpers;

namespace DatabaseServer.Network {
    public class ServerHandleData {
        private delegate void Packet_(long connectionID, byte[] data);
        static Dictionary<long, Packet_> packets;
        static long pLength;

        public static void InitializePackets() {
            Helpers.Logger.WriteInfo("Initializing Network Packets...");
            packets = new Dictionary<long, Packet_> {
                { (long)ClientPackets.C_CreateAccountDetails, PLAYERCREATEACCOUNT },
                { (long)ClientPackets.C_LoginCredentials, PLAYERLOGIN },
            };
        }

        public static void HandleData(long connectionID, byte[] data) {
            byte[] Buffer;
            Buffer = (byte[])data.Clone();


            if (Helpers.General.client[connectionID].playerBuffer == null) {
                Helpers.General.client[connectionID].playerBuffer = new ByteBuffer();
            }
            Helpers.General.client[connectionID].playerBuffer.WriteBytes(Buffer);

            if (Helpers.General.client[connectionID].playerBuffer.Count() == 0) {
                Helpers.General.client[connectionID].playerBuffer.Clear();
                return;
            }

            if (Helpers.General.client[connectionID].playerBuffer.Length() >= 8) {
                pLength = Helpers.General.client[connectionID].playerBuffer.ReadLong(false); //this line is causing memory issues?
                if (pLength <= 0) {
                    Helpers.General.client[connectionID].playerBuffer.Clear();
                    return;
                }
            }

            //checks if the recieved message is valid
            while (pLength > 0 & pLength <= Helpers.General.client[connectionID].playerBuffer.Length() - 8) {
                if (pLength <= Helpers.General.client[connectionID].playerBuffer.Length() - 8) {
                    Helpers.General.client[connectionID].playerBuffer.ReadLong();
                    data = Helpers.General.client[connectionID].playerBuffer.ReadBytes((int)pLength);
                    HandleDataPackets(connectionID, data);
                }
                pLength = 0;
                if (Helpers.General.client[connectionID].playerBuffer.Length() >= 8) {
                    pLength = Helpers.General.client[connectionID].playerBuffer.ReadLong(false);
                    if (pLength < 0) {
                        Helpers.General.client[connectionID].playerBuffer.Clear();
                        return;
                    }
                }
            }
        }

        public static void HandleDataPackets(long connectionID, byte[] data) {
            Logger.WriteInfo(data.Length.ToString());
            long packetIdentifier;
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);

            try {
                packetIdentifier = buffer.ReadLong();

            } catch (Exception) {
                return;

            }

            buffer.Dispose();

            if (packets.TryGetValue(packetIdentifier, out Packet_ packet)) {
                packet.Invoke(connectionID, data);
            }
        }

        #region RecievePackets

        static void PLAYERLOGIN(long connectionID, byte[] data) {

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);

            long packet = buffer.ReadLong();
            string player_Email = buffer.ReadString();
            string player_Password = buffer.ReadString();

            if (Database.AccountDetailsAreCorrect(player_Email, player_Password)) {
                //login to the game
                Logger.WriteDebug("Login Confirmed");
                ServerTCP.LoginConfirmed((int)connectionID);
            } else {
                Logger.WriteDebug("Login Cancelled");
                ServerTCP.IncorrectLoginDetails((int)connectionID);
            }
        }

        static void PLAYERCREATEACCOUNT(long connectionID, byte[] data){
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);

            long packet = buffer.ReadLong();
            string player_Username = buffer.ReadString();
            string player_Email = buffer.ReadString();
            string player_Password = buffer.ReadString();

            if (!Database.CheckUsername(player_Username)) {
                if (!Database.CheckEmail(player_Email)) {
                    Database.CreateAccount(player_Username, player_Password, player_Email, (int)connectionID);
                } else {
                    //send error that email already exists - Future "would you like that account password resetting?"
                    ServerTCP.EmailExists((int)connectionID);
                }
            } else {
                //send error back saying username exists
                ServerTCP.UsernameExists((int)connectionID);
            }
        }

        #endregion
    }
}
