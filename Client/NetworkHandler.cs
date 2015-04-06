using System;
using System.Collections.Generic;
using System.Linq;
using CWrapped;
using System.Net.Sockets;
using System.Threading;
using libMC.NET.Network;
using libMC.NET.MinecraftWorld;

namespace libMC.NET.Client {
    public class NetworkHandler {
        #region Variables
        Thread _handler;
        MinecraftClient _mainMc;
        TcpClient _baseSock;
        NetworkStream _baseStream;
        PacketEventHandler _packetHandlers;
        public Wrapped WSock;
        public TickHandler WorldTick;

        #region Packet Dictionaries
        Dictionary<int, Func<IPacket>> _packetsLogin;
        Dictionary<int, Func<IPacket>> _packetsPlay;
        Dictionary<int, Func<IPacket>> _packetsStatus;

        // -- Packet Handler Delegate...
        public delegate void PacketHandler(MinecraftClient client, IPacket packet);

        // -- Array containing packet handlers.
        public PacketHandler[] LoginHandlers;
        public PacketHandler[] PlayHandlers;
        public PacketHandler[] StatusHandlers;
        #endregion
        #endregion

        public NetworkHandler(MinecraftClient mc) {
            _mainMc = mc;
            LoginHandlers = new PacketHandler[3];
            PlayHandlers = new PacketHandler[65];
            StatusHandlers = new PacketHandler[2];
            PopulateLists();
        } 
        
        /// <summary>
        /// Starts the network handler. (Connects to a minecraft server)
        /// </summary>
        public void Start() {
            try {
                _baseSock = new TcpClient();
                var ar = _baseSock.BeginConnect(_mainMc.ServerIp, _mainMc.ServerPort, null, null);

                using (var wh = ar.AsyncWaitHandle) {
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false)) {
                        _baseSock.Close();
                        RaiseSocketError(this, "Failed to connect: Connection Timeout");
                        return;
                    }

                    _baseSock.EndConnect(ar);
                }

            } catch (Exception e) {
                RaiseSocketError(this, "Failed to connect: " + e.Message);
                return;
            }
            
            _mainMc.Running = true;

            RaiseSocketInfo(this, "Connected to server.");
            RaiseSocketDebug(this, string.Format("IP: {0} Port: {1}", _mainMc.ServerIp, _mainMc.ServerPort.ToString()));

            // -- Create our Wrapped socket.
            _baseStream = _baseSock.GetStream();
            WSock = new Wrapped(_baseStream);
            RaiseSocketDebug(this, "Socket Created");

            DoHandshake();

            _packetHandlers = new PacketEventHandler(this);

            // -- Start network parsing.
            _handler = new Thread(NetworkPacketHandler);
            _handler.Start();
            RaiseSocketDebug(this, "Handler thread started");
        }
       
        /// <summary>
        /// Stops the network handler. (Disconnects from a minecraft server)
        /// </summary>
        public void Stop() {
            DebugMessage(this, "Stopping network handler...");
            _handler.Abort();

            WSock = null;
            _baseStream = null;

            _baseSock.Close();
            InfoMessage(this, "Disconnected from Minecraft Server.");

        }

        /// <summary>
        /// Sends a server handshake, and a ping request packet if NextState is set to 1.
        /// </summary>
        public void DoHandshake() {
            var hs = new SbHandshake();
            hs.ProtocolVersion = 5;
            hs.ServerAddress = _mainMc.ServerIp;
            hs.ServerPort = (short)_mainMc.ServerPort;
            hs.NextState = 2;

            if (_mainMc.ServerState == 1)
                hs.NextState = 1;

            hs.Write(WSock);

            if (_mainMc.ServerState == 1) {
                var pingRequest = new SbRequest();
                pingRequest.Write(WSock);
            } else {
                var loginStart = new SbLoginStart();
                loginStart.Name = _mainMc.ClientName;
                loginStart.Write(WSock);
            }

            RaiseSocketDebug(this, "Handshake sent.");
        }

        /// <summary>
        /// Registers a method to be the handler of a given packet.
        /// </summary>
        /// <param name="packetId"></param>
        /// <param name="method"></param>
        public void RegisterLoginHandler(int packetId, PacketHandler method) {
            LoginHandlers[packetId] = method;
        }

        /// <summary>
        /// Registers a method to be the handler of a given packet.
        /// </summary>
        /// <param name="packetId"></param>
        /// <param name="method"></param>
        public void RegisterPlayHandler(int packetId, PacketHandler method) {
            PlayHandlers[packetId] = method;
        }

        /// <summary>
        /// Registers a method to be the handler of a given packet.
        /// </summary>
        /// <param name="packetId"></param>
        /// <param name="method"></param>
        public void RegisterStatusHandler(int packetId, PacketHandler method) {
            StatusHandlers[packetId] = method;
        }

        /// <summary>
        /// Populates the packet lists with reconized types.
        /// </summary>
        void PopulateLists() {
            _packetsLogin = new Dictionary<int,Func<IPacket>> {
                {0, () => new CbLoginDisconnect() },
                {1, () => new CbEncryptionRequest() },
                {2, () => new CbLoginSuccess() }
            };

            _packetsStatus = new Dictionary<int, Func<IPacket>> {
                {0, () => new CbResponse() },
                {1, () => new CbPing() }
            };

            //-------------------
            _packetsPlay = new Dictionary<int, Func<IPacket>> {
                {0, () => new CbKeepAlive() },
                {1, () => new CbJoinGame() },
                {2, () => new CbChatMessage() },
                {3, () => new CbTimeUpdate() },
                {4, () => new CbEntityEquipment() },
                {5, () => new CbSpawnPosition() },
                {6, () => new CbUpdateHealth() },
                {7, () => new CbRespawn() },
                {8, () => new CbPlayerPositionAndLook() },
                {9, () => new CbHeldItemChange() },
                {10, () => new CbUseBed() },
                {11, () => new CbAnimation() },
                {12, () => new CbSpawnPlayer() },
                {13, () => new CbCollectItem() },
                {14, () => new CbSpawnObject() },
                {15, () => new CbSpawnMob() },
                {16, () => new CbSpawnPainting() },
                {17, () => new CbSpawnExperienceOrb() },
                {18, () => new CbEntityVelocity() },
                {19, () => new CbDestroyEntities() },
                {20, () => new CbEntity() },
                {21, () => new CbEntityRelativeMove() },
                {22, () => new CbEntityLook() },
                {23, () => new CbEntityLookandRelativeMove() },
                {24, () => new CbEntityTeleport() },
                {25, () => new CbEntityHeadLook() },
                {26, () => new CbEntityStatus() },
                {27, () => new CbAttachEntity() },
                {28, () => new CbEntityMetadata() },
                {29, () => new CbEntityEffect() },
                {30, () => new CbRemoveEntityEffect() },
                {31, () => new CbSetExperience() },
                {32, () => new CbEntityProperties() },
                {33, () => new CbChunkData() },
                {34, () => new CbMultiBlockChange() },
                {35, () => new CbBlockChange() },
                {36, () => new CbBlockAction() },
                {37, () => new CbBlockBreakAnimation() },
                {38, () => new CbMapChunkBulk() },
                {39, () => new CbExplosion() },
                {40, () => new CbEffect() },
                {41, () => new CbSoundEffect() },
                {42, () => new CbParticle() },
                {43, () => new CbChangeGameState() },
                {44, () => new CbSpawnGlobalEntity() },
                {45, () => new CbOpenWindow() },
                {46, () => new CbCloseWindow() },
                {47, () => new CbSetSlot() },
                {48, () => new CbWindowItems() },
                {49, () => new CbWindowProperty() },
                {50, () => new CbConfirmTransaction() },
                {51, () => new CbUpdateSign() },
                {52, () => new CbMaps() }, // Still no clue what this is for.
                {53, () => new CbUpdateBlockEntity() },
                {54, () => new CbSignEditorOpen() },
                {55, () => new CbStatistics() },
                {56, () => new CbPlayerListItem() },
                {57, () => new CbPlayerAbilities() },
                {58, () => new CbTabComplete() },
                {59, () => new CbScoreboardObjective() },
                {60, () => new CbUpdateScore() },
                {61, () => new CbDisplayScoreboard() },
                {62, () => new CbTeams() },
                {63, () => new CbPluginMessage() },
                {64, () => new CbDisconnect() }
            };

            RaiseSocketDebug(this, "List populated");
        }
        
        /// <summary>
        /// Creates an instance of each new packet, so it can be parsed.
        /// </summary>
        void NetworkPacketHandler() {
            try {
                var length = -1;

                while ((length = WSock.readVarInt()) != -1) {
                    if (_baseSock.Connected) {
                        var packetId = WSock.readVarInt();

                        RaiseSocketDebug(this, _mainMc.ServerState + " " + packetId.ToString() + " " + length.ToString());

                        switch (_mainMc.ServerState) {
                            case (int)ServerState.Status:
                                if (_packetsStatus.Keys.Contains(packetId) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 1, Packet: " + packetId);
                                    WSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packet = _packetsStatus[packetId]();
                                packet.Read(WSock);

                                if (StatusHandlers[packetId] != null)
                                    StatusHandlers[packetId](_mainMc, packet);

                                RaisePacketHandled(this, packet, packetId);

                                break;

                            case (int)ServerState.Login:
                                if (_packetsLogin.Keys.Contains(packetId) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 2, Packet: " + packetId);
                                    WSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetl = _packetsLogin[packetId]();
                                packetl.Read(WSock);

                                if (LoginHandlers[packetId] != null)
                                    LoginHandlers[packetId](_mainMc, packetl);

                                RaisePacketHandled(this, packetl, packetId);

                                break;

                            case (int)ServerState.Play:
                                if (_packetsPlay.Keys.Contains(packetId) == false) {
                                    RaiseSocketError(this, "Unknown Packet ID. State: 3, Packet: " + packetId);
                                    WSock.readByteArray(length - 1); // -- bypass the packet
                                    continue;
                                }

                                var packetp = _packetsPlay[packetId]();
                                packetp.Read(WSock);

                                
                                if (PlayHandlers[packetId] != null)
                                    PlayHandlers[packetId](_mainMc, packetp);

                                RaisePacketHandled(this, packetp, packetId);

                                break;
                            default:
                                RaiseSocketDebug(this, "Uhhhh what????");
                                break;
                        }
                        if (WorldTick != null)
                            WorldTick.DoTick();
                    }
                }
                RaiseSocketDebug(this, "whhaaat??");
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) {
                    RaiseSocketError(this, "Critical error in handling packets.");
                    RaiseSocketError(this, e.Message);
                    RaiseSocketError(this, e.StackTrace);
                    Stop();
                }
            }
            RaiseSocketDebug(this, "Im ending!");
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
