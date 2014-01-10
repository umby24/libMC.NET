using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrapped;
using System.Net.Sockets;
using System.Threading;
using libMC.NET.Packets.Handshake;
using libMC.NET.Packets.Login;
using libMC.NET.MinecraftWorld;

namespace libMC.NET {
    public class NetworkHandler {
        #region Variables
        Thread handler;
        Minecraft mainMC;
        TcpClient baseSock;
        NetworkStream baseStream;
        public Wrapped.Wrapped wSock;
        public TickHandler worldTick;

        #region Packet Dictionaries
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsLogin;
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsPlay;
        Dictionary<int, Func<Minecraft, Packets.Packet>> packetsStatus;
        #endregion
        #endregion

        public NetworkHandler(Minecraft mc) {
            mainMC = mc;
            PopulateLists();
        } 
        
        /// <summary>
        /// Starts the network handler.
        /// </summary>
        public void Start() {
            try {
                baseSock = new TcpClient();
                IAsyncResult AR = baseSock.BeginConnect(mainMC.ServerIP, mainMC.ServerPort, null, null);
                WaitHandle wh = AR.AsyncWaitHandle;

                try {
                    if (!AR.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
                        baseSock.Close();
                        RaiseSocketError(this, "Failed to connect: Connection Timeout");
                        return;
                    }

                    baseSock.EndConnect(AR);
                } finally {
                    wh.Close();
                }

            } catch (Exception e) {
                RaiseSocketError(this, "Failed to connect: " + e.Message);
                return;
            }
            
            mainMC.Running = true;

            RaiseSocketInfo(this, "Connected to server.");
            RaiseSocketDebug(this, string.Format("IP: {0} Port: {1}", mainMC.ServerIP, mainMC.ServerPort.ToString()));

            // -- Create our Wrapped socket.
            baseStream = baseSock.GetStream();
            wSock = new Wrapped.Wrapped(baseStream);

            RaiseSocketDebug(this, "Socket Created");

            // -- Send a handshake packet

            if (mainMC.ServerState != 1) {
                Handshake hs = new Handshake(ref mainMC);
                RaiseSocketDebug(this, "Handshake sent.");
            } else {
                Handshake hs = new Handshake(ref mainMC);
                RaiseSocketDebug(this, "Handshake sent, Pinging server!");
            }

            // -- Start network parsing.
            handler = new Thread(PacketHandler);
            handler.Start();
            RaiseSocketDebug(this, "Handler thread started");
        }
       
        /// <summary>
        /// Stops the network handler.
        /// </summary>
        public void Stop() {
            DebugMessage(this, "Stopping network handler...");
            handler.Abort();

            wSock = null;
            baseStream = null;

            baseSock.Close();
            InfoMessage(this, "Disconnected from Minecraft Server.");


        }
        
        /// <summary>
        /// Populates the packet lists with reconized types.
        /// </summary>
        /// 
        void PopulateLists() {
            packetsLogin = new Dictionary<int,Func<Minecraft,Packets.Packet>> {
                {0, (mainMC) => new Disconnect(ref mainMC) },
                {1, (mainMC) => new EncryptionRequest(ref mainMC) },
                {2, (mainMC) => new LoginSuccess(ref mainMC) }
            };

            packetsStatus = new Dictionary<int, Func<Minecraft, Packets.Packet>> {
                {0, (mainMC) => new Packets.Status.Response(ref mainMC) },
                {1, (mainMC) => new Packets.Status.ServerPing(ref mainMC) }
            };

            //-------------------
            packetsPlay = new Dictionary<int, Func<Minecraft, Packets.Packet>> {
                {0, (mainMC) => new Packets.Play.KeepAlive(ref mainMC) },
                {1, (mainMC) => new Packets.Play.JoinGame(ref mainMC) },
                {2, (mainMC) => new Packets.Play.ChatMessage(ref mainMC) },
                {3, (mainMC) => new Packets.Play.TimeUpdate(ref mainMC) },
                {4, (mainMC) => new Packets.Play.EntityEquipment(ref mainMC) },
                {5, (mainMC) => new Packets.Play.SpawnPosition(ref mainMC) },
                {6, (mainMC) => new Packets.Play.UpdateHealth(ref mainMC) },
                {7, (mainMC) => new Packets.Play.Respawn(ref mainMC) },
                {8, (mainMC) => new Packets.Play.PlayerPositionandLook(ref mainMC) },
                {9, (mainMC) => new Packets.Play.HeldItemChange(ref mainMC) },
                {10, (mainMC) => new Packets.Play.UseBed(ref mainMC) },
                {11, (mainMC) => new Packets.Play.Animation(ref mainMC) },
                {12, (mainMC) => new Packets.Play.SpawnPlayer(ref mainMC) },
                {13, (mainMC) => new Packets.Play.CollectItem(ref mainMC) },
                {14, (mainMC) => new Packets.Play.SpawnObject(ref mainMC) },
                {15, (mainMC) => new Packets.Play.SpawnMob(ref mainMC) },
                {16, (mainMC) => new Packets.Play.SpawnPainting(ref mainMC) },
                {17, (mainMC) => new Packets.Play.SpawnExpOrb(ref mainMC) },
                {18, (mainMC) => new Packets.Play.EntityVelocity(ref mainMC) },
                {19, (mainMC) => new Packets.Play.DestroyEntities(ref mainMC) },
                {20, (mainMC) => new Packets.Play.Entity(ref mainMC) },
                {21, (mainMC) => new Packets.Play.EntityRelativeMove(ref mainMC) },
                {22, (mainMC) => new Packets.Play.EntityLook(ref mainMC) },
                {23, (mainMC) => new Packets.Play.EntityLookRelativeMove(ref mainMC) },
                {24, (mainMC) => new Packets.Play.EntityTeleport(ref mainMC) },
                {25, (mainMC) => new Packets.Play.EntityHeadLook(ref mainMC) },
                {26, (mainMC) => new Packets.Play.EntityStatus(ref mainMC) },
                {27, (mainMC) => new Packets.Play.attachEntity(ref mainMC) },
                {28, (mainMC) => new Packets.Play.EntityMetadata(ref mainMC) },
                {29, (mainMC) => new Packets.Play.EntityEffect(ref mainMC) },
                {30, (mainMC) => new Packets.Play.RemoveEntityEffect(ref mainMC) },
                {31, (mainMC) => new Packets.Play.SetExperience(ref mainMC) },
                {32, (mainMC) => new Packets.Play.EntityProperties(ref mainMC) },
                {33, (mainMC) => new Packets.Play.ChunkData(ref mainMC) },
                {34, (mainMC) => new Packets.Play.MultiBlockChange(ref mainMC) },
                {35, (mainMC) => new Packets.Play.BlockChange(ref mainMC) },
                {36, (mainMC) => new Packets.Play.BlockAction(ref mainMC) },
                {37, (mainMC) => new Packets.Play.BlockBreakAnimation(ref mainMC) },
                {38, (mainMC) => new Packets.Play.MapChunkBulk(ref mainMC) },
                {39, (mainMC) => new Packets.Play.Explosion(ref mainMC) },
                {40, (mainMC) => new Packets.Play.Effects(ref mainMC) },
                {41, (mainMC) => new Packets.Play.SoundEffect(ref mainMC) },
                {42, (mainMC) => new Packets.Play.Particle(ref mainMC) },
                {43, (mainMC) => new Packets.Play.ChangeGameState(ref mainMC) },
                {44, (mainMC) => new Packets.Play.SpawnGlobalEntity(ref mainMC) },
                {45, (mainMC) => new Packets.Play.OpenWindow(ref mainMC) },
                {46, (mainMC) => new Packets.Play.CloseWindow(ref mainMC) },
                {47, (mainMC) => new Packets.Play.SetSlot(ref mainMC) },
                {48, (mainMC) => new Packets.Play.WindowItems(ref mainMC) },
                {49, (mainMC) => new Packets.Play.WindowProperty(ref mainMC) },
                {50, (mainMC) => new Packets.Play.ConfirmTransaction(ref mainMC) },
                {51, (mainMC) => new Packets.Play.UpdateSign(ref mainMC) },
                {52, (mainMC) => new Packets.Play.Maps(ref mainMC) }, // Still no clue what this is for.
                {53, (mainMC) => new Packets.Play.UpdateBlockEntity(ref mainMC) },
                {54, (mainMC) => new Packets.Play.SignEditorOpen(ref mainMC) },
                {55, (mainMC) => new Packets.Play.Statistics(ref mainMC) },
                {56, (mainMC) => new Packets.Play.PlayerListItem(ref mainMC) },
                {57, (mainMC) => new Packets.Play.PlayerAbilities(ref mainMC) },
                {58, (mainMC) => new Packets.Play.TabComplete(ref mainMC) },
                {59, (mainMC) => new Packets.Play.ScoreboardObjective(ref mainMC) },
                {60, (mainMC) => new Packets.Play.UpdateScore(ref mainMC) },
                {61, (mainMC) => new Packets.Play.DisplayScoreboard(ref mainMC) },
                {62, (mainMC) => new Packets.Play.Teams(ref mainMC) },
                {63, (mainMC) => new Packets.Play.PluginMessage(ref mainMC) },
                {64, (mainMC) => new Packets.Play.Disconnect(ref mainMC) }
            };

            RaiseSocketDebug(this, "List populated");
        }
        /// <summary>
        /// Creates an instance of each new packet, so it can be parsed.
        /// </summary>
        void PacketHandler() {
            try {
                int length = 0;

                while ((length = wSock.readVarInt()) != 0) {
                    if (baseSock.Connected) {
                        int packetID = wSock.readVarInt();

                        switch (mainMC.ServerState) {
                            case (int)ServerState.Status:
                                if (packetsStatus.Keys.Contains(packetID) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 1, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packet = packetsStatus[packetID](mainMC);
                                RaisePacketHandled(this, packet, packetID);

                                break;

                            case (int)ServerState.Login:
                                if (packetsLogin.Keys.Contains(packetID) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 2, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetl = packetsLogin[packetID](mainMC);
                                RaisePacketHandled(this, packetl, packetID);

                                break;

                            case (int)ServerState.Play:
                                if (packetsPlay.Keys.Contains(packetID) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 3, Packet: " + packetID);
                                    wSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetp = packetsPlay[packetID](mainMC);
                                RaisePacketHandled(this, packetp, packetID);

                                break;
                        }
                        if (worldTick != null)
                            worldTick.DoTick();
                    }
                }
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) {
                    RaiseSocketError(this, "Critical error in handling packets.");
                    RaiseSocketError(this, e.Message);
                    RaiseSocketError(this, e.StackTrace);
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
        public void RaiseSocketError(object sender, string message) {
            if (SocketError != null)
                SocketError(sender, message);
            
        }
        public void RaiseSocketInfo(object sender, string message) {
            if (InfoMessage != null)
                InfoMessage(sender, message);
        }
        public void RaiseSocketDebug(object sender, string message) {
            if (DebugMessage != null)
                DebugMessage(sender, message);
        }
        public void RaisePacketHandled(object sender, object packet, int id) {
            if (PacketHandled != null)
                PacketHandled(sender, packet, id);
        }
        #endregion
        #region Event Delegates
        public delegate void SocketErrorHandler(object sender, string message);
        public event SocketErrorHandler SocketError;

        public delegate void NetworkInfoHandler(object sender, string message);
        public event NetworkInfoHandler InfoMessage;

        public delegate void NetworkDebugHandler(object sender, string message);
        public event NetworkDebugHandler DebugMessage;

        public delegate void PacketHandledHandler(object sender, object packet, int id);
        public event PacketHandledHandler PacketHandled;
        #endregion
    }
}
