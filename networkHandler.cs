﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrapped;
using System.Net.Sockets;
using System.Threading;
using libMC.NET.Packets.Handshake;
using libMC.NET.Packets.Login;

namespace libMC.NET {
    public class networkHandler {
        #region Variables
        Thread handler;
        Minecraft mainMC;
        TcpClient baseSock;
        NetworkStream baseStream;
        public Wrapped.Wrapped wSock;

        #region Packet Dictionaries
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsLogin;
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsPlay;
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsStatus;
        #endregion
        #endregion

        public networkHandler(Minecraft mc) {
            mainMC = mc;
            populateLists();
        } 
        
        /// <summary>
        /// Starts the network handler.
        /// </summary>
        public void Start() {
            try {
                baseSock = new TcpClient();
                IAsyncResult AR = baseSock.BeginConnect(mainMC.serverIP, mainMC.serverPort, null, null);
                WaitHandle wh = AR.AsyncWaitHandle;

                try {
                    if (!AR.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
                        baseSock.Close();
                        raiseSocketError(this, "Failed to connect: Connection Timeout");
                        return;
                    }

                    baseSock.EndConnect(AR);
                } finally {
                    wh.Close();
                }

            } catch (Exception e) {
                raiseSocketError(this, "Failed to connect: " + e.Message);
                return;
            }
            
            mainMC.running = true;

            raiseSocketInfo(this, "Connected to server.");
            raiseSocketDebug(this, string.Format("IP: {0} Port: {1}", mainMC.serverIP,mainMC.serverPort.ToString()));

            // -- Create our Wrapped socket.
            baseStream = baseSock.GetStream();
            wSock = new Wrapped.Wrapped(baseStream);

            raiseSocketDebug(this, "Socket Created");

            // -- Send a handshake packet
            if (mainMC.serverState != 1) {
                Handshake hs = new Handshake(ref mainMC);
                raiseSocketDebug(this, "Handshake sent.");
            } else {
                //TODO: Implement Server Pinging
                Handshake hs = new Handshake(ref mainMC);
                raiseSocketDebug(this, "Handshake sent, Pinging server!");
            }

            // -- Start network parsing.
            handler = new Thread(packetHandler);
            handler.Start();
            raiseSocketDebug(this, "Handler thread started");
        }
       
        /// <summary>
        /// Stops the network handler.
        /// </summary>
        public void Stop() {
            debugMessage(this, "Stopping network handler...");
            handler.Abort();

            wSock = null;
            baseStream = null;

            baseSock.Close();
            infoMessage(this, "Disconnected from Minecraft Server.");

        }
        
        /// <summary>
        /// Populates the packet lists with reconized types.
        /// </summary>
        /// 
        void populateLists() {
            packetsLogin = new Dictionary<int,Func<Minecraft,Packets.Packet>> {
                {0, (mainMC) => new Disconnect(ref mainMC) },
                {1, (mainMC) => new encryptionRequest(ref mainMC) },
                {2, (mainMC) => new loginSuccess(ref mainMC) }
            };

            packetsStatus = new Dictionary<int, Func<Minecraft, Packets.Packet>> {
                {0, (mainMC) => new Packets.Status.Response(ref mainMC) },
                {1, (mainMC) => new Packets.Status.ServerPing(ref mainMC) }
            };

            //-------------------
            packetsPlay = new Dictionary<int, Func<Minecraft, Packets.Packet>> {
                {0, (mainMC) => new Packets.Play.keepAlive(ref mainMC) },
                {1, (mainMC) => new Packets.Play.joinGame(ref mainMC) },
                {2, (mainMC) => new Packets.Play.chatMessage(ref mainMC) },
                {3, (mainMC) => new Packets.Play.timeUpdate(ref mainMC) },
                {4, (mainMC) => new Packets.Play.entityEquipment(ref mainMC) },
                {5, (mainMC) => new Packets.Play.spawnPosition(ref mainMC) },
                {6, (mainMC) => new Packets.Play.updateHealth(ref mainMC) },
                {7, (mainMC) => new Packets.Play.Respawn(ref mainMC) },
                {8, (mainMC) => new Packets.Play.playerPositionandLook(ref mainMC) },
                {9, (mainMC) => new Packets.Play.heldItemChange(ref mainMC) },
                {10, (mainMC) => new Packets.Play.useBed(ref mainMC) },
                {11, (mainMC) => new Packets.Play.Animation(ref mainMC) },
                {12, (mainMC) => new Packets.Play.spawnPlayer(ref mainMC) },
                {13, (mainMC) => new Packets.Play.collectItem(ref mainMC) },
                {14, (mainMC) => new Packets.Play.spawnObject(ref mainMC) },
                {15, (mainMC) => new Packets.Play.spawnMob(ref mainMC) },
                {16, (mainMC) => new Packets.Play.spawnPainting(ref mainMC) },
                {17, (mainMC) => new Packets.Play.spawnExpOrb(ref mainMC) },
                {18, (mainMC) => new Packets.Play.entityVelocity(ref mainMC) },
                {19, (mainMC) => new Packets.Play.destroyEntities(ref mainMC) },
                {20, (mainMC) => new Packets.Play.Entity(ref mainMC) },
                {21, (mainMC) => new Packets.Play.entityRelativeMove(ref mainMC) },
                {22, (mainMC) => new Packets.Play.entityLook(ref mainMC) },
                {23, (mainMC) => new Packets.Play.entityLookRelativeMove(ref mainMC) },
                {24, (mainMC) => new Packets.Play.entityTeleport(ref mainMC) },
                {25, (mainMC) => new Packets.Play.entityHeadLook(ref mainMC) },
                {26, (mainMC) => new Packets.Play.entityStatus(ref mainMC) },
                {27, (mainMC) => new Packets.Play.attachEntity(ref mainMC) },
                {28, (mainMC) => new Packets.Play.entityMetadata(ref mainMC) },
                {29, (mainMC) => new Packets.Play.entityEffect(ref mainMC) },
                {30, (mainMC) => new Packets.Play.removeEntityEffect(ref mainMC) },
                {31, (mainMC) => new Packets.Play.setExperience(ref mainMC) },
                {32, (mainMC) => new Packets.Play.entityProperties(ref mainMC) },
                {33, (mainMC) => new Packets.Play.ChunkData(ref mainMC) },
                {34, (mainMC) => new Packets.Play.multiBlockChange(ref mainMC) },
                {35, (mainMC) => new Packets.Play.blockChange(ref mainMC) },
                {36, (mainMC) => new Packets.Play.blockAction(ref mainMC) },
                {37, (mainMC) => new Packets.Play.blockBreakAnimation(ref mainMC) },
                {38, (mainMC) => new Packets.Play.MapChunkBulk(ref mainMC) },
                {39, (mainMC) => new Packets.Play.explosion(ref mainMC) },
                {40, (mainMC) => new Packets.Play.Effects(ref mainMC) },
                {41, (mainMC) => new Packets.Play.soundEffect(ref mainMC) },
                {42, (mainMC) => new Packets.Play.Particle(ref mainMC) },
                {43, (mainMC) => new Packets.Play.changeGameState(ref mainMC) },
                {44, (mainMC) => new Packets.Play.spawnGlobalEntity(ref mainMC) },
                {45, (mainMC) => new Packets.Play.openWindow(ref mainMC) },
                {46, (mainMC) => new Packets.Play.closeWindow(ref mainMC) },
                {47, (mainMC) => new Packets.Play.setSlot(ref mainMC) },
                {48, (mainMC) => new Packets.Play.windowItems(ref mainMC) },
                {49, (mainMC) => new Packets.Play.windowProperty(ref mainMC) },
                {50, (mainMC) => new Packets.Play.confirmTransaction(ref mainMC) },
                {51, (mainMC) => new Packets.Play.updateSign(ref mainMC) },
                {52, (mainMC) => new Packets.Play.Maps(ref mainMC) }, // Still no clue what this is for.
                {53, (mainMC) => new Packets.Play.updateBlockEntity(ref mainMC) },
                {54, (mainMC) => new Packets.Play.signEditorOpen(ref mainMC) },
                {55, (mainMC) => new Packets.Play.Statistics(ref mainMC) },
                {56, (mainMC) => new Packets.Play.playerListItem(ref mainMC) },
                {57, (mainMC) => new Packets.Play.playerAbilities(ref mainMC) },
                {58, (mainMC) => new Packets.Play.tabComplete(ref mainMC) },
                {59, (mainMC) => new Packets.Play.scoreboardObjective(ref mainMC) },
                {60, (mainMC) => new Packets.Play.updateScore(ref mainMC) },
                {61, (mainMC) => new Packets.Play.displayScoreboard(ref mainMC) },
                {62, (mainMC) => new Packets.Play.Teams(ref mainMC) },
                {63, (mainMC) => new Packets.Play.pluginMessage(ref mainMC) },
                {64, (mainMC) => new Packets.Play.Disconnect(ref mainMC) }
            };

            raiseSocketDebug(this, "List populated");
        }
        /// <summary>
        /// Creates an instance of each new packet, so it can be parsed.
        /// </summary>
        void packetHandler() {
            try {
                int length = 0;

                while ((length = wSock.readVarInt()) != 0) {
                    if (baseSock.Connected) {
                        int packetID = wSock.readVarInt();

                        switch (mainMC.serverState) {
                            case (int)ServerState.Status:
                                if (packetsStatus.Keys.Contains(packetID) == false) {
                                    raiseSocketError(this, "Unknown Packet ID. State: 1, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packet = packetsStatus[packetID](mainMC);
                                raisePacketHandled(this, packet, packetID);

                                break;

                            case (int)ServerState.Login:
                                if (packetsLogin.Keys.Contains(packetID) == false) {
                                    raiseSocketError(this, "Unknown Packet ID. State: 2, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetl = packetsLogin[packetID](mainMC);
                                raisePacketHandled(this, packetl, packetID);

                                break;

                            case (int)ServerState.Play:
                                if (packetsPlay.Keys.Contains(packetID) == false) {
                                    raiseSocketError(this, "Unknown Packet ID. State: 3, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetp = packetsPlay[packetID](mainMC);
                                raisePacketHandled(this, packetp, packetID);

                                break;
                        
                        }
                        //Packets.Play.ServerBound.playerPositionAndLook c = new Packets.Play.ServerBound.playerPositionAndLook(ref mainMC);
                    }
                }
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) {
                    raiseSocketError(this, "Critical error in handling packets.");
                    raiseSocketError(this, e.Message);
                    raiseSocketError(this, e.StackTrace);
                    Stop();
                }
            }
        }

        enum ServerState {
            Status = 1,
            Login,
            Play
        }
        #region Event Messengers
        public void raiseSocketError(object sender, string message) {
            if (socketError != null)
                socketError(sender, message);
            
        }
        public void raiseSocketInfo(object sender, string message) {
            if (infoMessage != null)
                infoMessage(sender, message);
        }
        public void raiseSocketDebug(object sender, string message) {
            if (debugMessage != null)
                debugMessage(sender, message);
        }
        public void raisePacketHandled(object sender, object packet, int id) {
            if (PacketHandled != null)
                PacketHandled(sender, packet, id);
        }
        #endregion
        #region Events
        public delegate void socketErrorHandler(object sender, string message);
        public event socketErrorHandler socketError;

        public delegate void networkInfoHandler(object sender, string message);
        public event networkInfoHandler infoMessage;

        public delegate void networkDebugHandler(object sender, string message);
        public event networkDebugHandler debugMessage;

        public delegate void packetHandledHandler(object sender, object packet, int id);
        public event packetHandledHandler PacketHandled;
        #endregion
    }
}