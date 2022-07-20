using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TotalAnarchyGameServer.Player;
using TotalAnarchyGameServer.Weapons;
using TotalAnarchyGameServer.Weapons.Ammo;
using System.Diagnostics;

namespace TotalAnarchyGameServer.Network {
    public class Lobby {
        public List<Clients> clientsInLobby = new List<Clients>(new Clients[Constants.MAX_PLAYERS_PER_LOBBY]);
        public int lobbyID;
        public int connectedClients = 0;

        public List<Weapon> worldWeapons;
        public List<Ammo> worldAmmo;

        private int worldIDs = 0;
        ByteBuffer playerDataBuffer = new ByteBuffer();

        public void InitializeLobby() {
            
            worldWeapons = new List<Weapon>();
            worldAmmo = new List<Ammo>();
            //generate weapon and ammo positions

            GenerateWeapons();
            GenerateAmmo();
            //run the loop
            LobbyLoop();
        }

        void GenerateWeapons() {
            Random rndWeapon = new Random();
            for (int i = 0; i < Constants.WEAPON_SPAWN_POINTS; i++) {
                int weapon = rndWeapon.Next(0, WeaponManager.weapons.Count);
                Weapon newWeapon = null;
                switch (weapon) {
                    case 0:
                        newWeapon = new Rifle();
                        break;
                    case 1:
                        newWeapon = new Shotgun();
                        break;
                    case 2:
                        newWeapon = new Pistol();
                        break;
                    case 3:
                        newWeapon = new Knife();
                        break;
                    case 4:
                        newWeapon = new Grenade();
                        break;
                }

                newWeapon.isDropped = true;
                newWeapon.worldSpawn = i;
                newWeapon.worldID = worldIDs;
                worldIDs++;
                worldWeapons.Add(newWeapon); // need to make a new instance
            }
        }
        void GenerateAmmo() {
            Random rndAmmo = new Random();
            for (int i = 0; i < Constants.AMMO_SPAWN_POINTS; i++) {
                int ammo = rndAmmo.Next(0, WeaponManager.ammoTypes.Count);
                Ammo newAmmo = null;
                switch (ammo) {
                    case 0:
                        newAmmo = new RifleAmmo();
                        break;
                    case 1:
                        newAmmo = new PistolAmmo();
                        break;
                    case 2:
                        newAmmo = new ShotgunAmmo();
                        break;
                }

                newAmmo.isDropped = true;
                newAmmo.worldSpawn = i;
                newAmmo.worldID = worldIDs;
                worldIDs++;
                worldAmmo.Add(newAmmo); // need to make a new instance
            }
        }
        public void LobbyLoop() {
            while (true) {
                UpdatePlayerPositions();
                UpdatePlayerRotations();
                Thread.Sleep(Constants.LOBBY_TICK_RATE);
            }
        }

        public void JoinLobby(Clients client) {
            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i] == client) {
                    Logger.WriteError("Player already exists within this instance");
                    return;
                }
            }

            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].socket == null) {
                    clientsInLobby[i] = client;
                    break;
                }
            }

            //Join this game
            client.lobbyIDConnectedTo = lobbyID;
            connectedClients++;
            Logger.WriteInfo("Player: " + client.connectionID + " has joined lobby number: " + lobbyID);
            SendWelcomeMessage(client);
            SendInWorld(client);

        }               
        public void SendWelcomeMessage(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SWelcomeMessage); //Packet Identifier.
            buffer.WriteInteger(client.connectionID);
            buffer.WriteString("Welcome to Total Anarchy. You are connected to Development Server. Lobby No: " + lobbyID);
            ServerTCP.SendDataTo(client.connectionID, buffer.ToArray());
            buffer.Dispose();
        }
        public void SendInWorld(Clients client) {
            ServerTCP.SendToAllInLobby(this, LobbyPlayerData(client));
        }

        public byte[] LobbyPlayerData(Clients client) {
            playerDataBuffer.Clear();
            playerDataBuffer.WriteLong((long)ServerPackets.SPlayerData);
            playerDataBuffer.WriteInteger(connectedClients);

            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].socket != null) { //downloads all players currently on the lobby
                    playerDataBuffer.WriteInteger(clientsInLobby[i].connectionID);
                }              
            }
            return playerDataBuffer.ToArray();
        }
        public void UpdatePlayerPositions() {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerPosition);
            buffer.WriteString("-");
            buffer.WriteInteger(connectedClients);
            buffer.WriteString("-");
            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].endpoint != null) {
                    buffer.WriteInteger(clientsInLobby[i].connectionID);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.posx);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.posy);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.posz);
                }
            }

            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].endpoint != null) { 
                    ServerUDP.SendTo(clientsInLobby[i].endpoint, buffer.ToArray());
                }
            }
            //Logger.WriteDebug("Updating player positions");
        }
        public void UpdatePlayerRotations() {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerRotation);
            buffer.WriteString("-");
            buffer.WriteInteger(connectedClients);
            buffer.WriteString("-");
            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].socket != null) {
                    buffer.WriteInteger(clientsInLobby[i].connectionID);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.rotx);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.roty);
                    buffer.WriteString("-");                
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.rotz);
                    buffer.WriteString("-");
                    buffer.WriteFloat(clientsInLobby[i].playerCharacter.rotw);
                }
            }

            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i].endpoint != null) {
                    ServerUDP.SendTo(clientsInLobby[i].endpoint, buffer.ToArray());
                }
            }

        }
        public void RequestWorldWeapons(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SWorldWeapons);
            for (int i = 0; i < worldWeapons.Count; i++) {
                if (worldWeapons[i].isDropped) {
                    buffer.WriteInteger(worldWeapons[i].worldID);
                    buffer.WriteInteger(worldWeapons[i].weaponID);
                    buffer.WriteString(worldWeapons[i].weaponName);
                    buffer.WriteInteger(worldWeapons[i].worldSpawn);
                    Thread.Sleep(100);      
                }
            }
            Logger.WriteDebug("Sending World Weapons");
            ServerTCP.SendDataTo(client.connectionID, buffer.ToArray());
        }
        public void RequetWorldAmmo(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SWorldAmmo);
            for (int i = 0; i < worldAmmo.Count; i++) {
                if (worldAmmo[i].isDropped) {
                    buffer.WriteInteger(worldAmmo[i].worldID);
                    buffer.WriteInteger(worldAmmo[i].ammoID);
                    buffer.WriteString(worldAmmo[i].ammoName);
                    buffer.WriteInteger(worldAmmo[i].worldSpawn);
                }
            }
            Logger.WriteDebug("Sending World Ammo");
            ServerTCP.SendDataTo(client.connectionID, buffer.ToArray());
        }
        public void PlayerShoot(Clients client, string WepName) {
            ByteBuffer buffer = new ByteBuffer();
            try {
                if (WepName == client.playerCharacter.playerInventory.currentEquippedWep.weaponName) {
                    if (client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo == 0) {
                        return;
                    }
                    client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo--;
                }
                ServerTCP.SendToAllInLobbyBut(this, client.connectionID, buffer.ToArray());

                //update the ammo for the player
                UpdateAmmo(client);
            } catch (Exception e) {
                Logger.WriteError("Error on player shoot: " + e.ToString());
            }
        }
        public void Reload(Clients client) {

            switch (client.playerCharacter.playerInventory.currentEquippedWep.ammoType) {
                
                case AmmoType.pistol:
                    client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo = client.playerCharacter.playerInventory.currentEquippedWep.ammoPerClip;
                    client.playerCharacter.playerInventory.pistolRounds -= client.playerCharacter.playerInventory.currentEquippedWep.ammoPerClip;
                    break;
                case AmmoType.shotgun:
                    client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo++;
                    client.playerCharacter.playerInventory.shotgunRounds--;
                    break;
                case AmmoType.rifle:
                    client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo = client.playerCharacter.playerInventory.currentEquippedWep.ammoPerClip;
                    client.playerCharacter.playerInventory.rifleRounds -= client.playerCharacter.playerInventory.currentEquippedWep.ammoPerClip;
                    break;
            }
            UpdateAmmo(client);
        }
        public void UpdateAmmo(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SUpdateAmmo);
            Logger.WriteDebug("Updating Ammo");

            if (client.playerCharacter.playerInventory.currentEquippedWep == null) {
                buffer.WriteString("-");
                buffer.WriteString("-");
            } else {
                buffer.WriteString(client.playerCharacter.playerInventory.currentEquippedWep.currentAmmo.ToString());

                switch (client.playerCharacter.playerInventory.currentEquippedWep.ammoType) {
                    case AmmoType.pistol:
                        buffer.WriteString(client.playerCharacter.playerInventory.pistolRounds.ToString());
                        break;
                    case AmmoType.shotgun:
                        buffer.WriteString(client.playerCharacter.playerInventory.shotgunRounds.ToString());
                        break;
                    case AmmoType.rifle:
                        buffer.WriteString(client.playerCharacter.playerInventory.rifleRounds.ToString());
                        break;
                }
            }
            ServerTCP.SendDataTo(client.connectionID, buffer.ToArray());
        }
        public void PlayerEquippedWep(Clients client, string wepToEquip) {
            for (int i = 0; i < WeaponManager.weapons.Count; i++) {
                if (WeaponManager.weapons[i].weaponName == wepToEquip) {
                    client.playerCharacter.playerInventory.currentEquippedWep = WeaponManager.weapons[i];
                    //Tell other clients in lobby
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteLong((long)ServerPackets.SPlayerSwitchedWeapon);
                    buffer.WriteInteger(client.connectionID);
                    buffer.WriteString(wepToEquip);

                    ServerTCP.SendToAllInLobbyBut(this, client.connectionID, buffer.ToArray());
                    return;
                }
            }

        }
        public void PlayerDisconnected(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerDisconnected);
            buffer.WriteInteger(client.connectionID);

            for (int i = 0; i < clientsInLobby.Count; i++) {
                if (clientsInLobby[i] == client) {
                    clientsInLobby[i] = new Clients();
                    connectedClients--;
                    return;
                }
            }
            ServerTCP.SendToAllInLobby(this, buffer.ToArray());
        }
        public void PlayerDied(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerDied);
            buffer.WriteInteger(client.connectionID);

            Logger.WriteInfo(client.connectionID + " Has Died");
            ServerTCP.SendToAllInLobby(this, buffer.ToArray());
        }
        public void RespawnPlayer(Clients client) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SRespawn);
            buffer.WriteInteger(client.connectionID);

            client.playerCharacter = new PlayerCharacter();
            ServerTCP.SendToAllInLobby(this, buffer.ToArray());
        }
    }
}

