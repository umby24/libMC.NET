using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using libMC.NET.Common;
using libMC.NET.World;
using libMC.NET.Entities;
using libMC.NET.Network;

// TODO: [Low] Comment more things
// TODO: [Medium] Speed things up, optimize code.
namespace libMC.NET.Client {
    /// <summary>
    /// Main class for libMC.Net.Client, the Client portion of a MinecraftClient interaction library for .NET.
    /// </summary>
    public class MinecraftClient {
        #region Variables
        public string ServerIP, ClientName, ClientPassword, AccessToken, ClientToken, SelectedProfile, ClientBrand;
        public int ServerPort, ServerState;
        public bool VerifyNames, Running, First = false;
        public NetworkHandler nh;

        #region Trackers
        public WorldClass MinecraftWorld; // -- Holds all of the world information. Time, chunks, players, ect.
        public Player ThisPlayer; // -- Holds all user information, location, inventory and so on.
        public Dictionary<string, short> Players;
        #endregion
        #endregion

        /// <summary>
        /// Create a new MinecraftClient Client Instance
        /// </summary>
        /// <param name="ip">The IP of the server to connect to</param>
        /// <param name="port">The port of the server to connect to</param>
        /// <param name="username">The username to use when connecting to MinecraftClient</param>
        /// <param name="password">The password to use when connecting to MinecraftClient (Ignore if you are providing credentials)</param>
        /// <param name="nameVerification">To connect using Name Verification or not</param>
        public MinecraftClient(string ip, int port, string username, string password, bool nameVerification) {
            ServerIP = ip;
            ServerPort = port;
            ClientName = username;
            ClientPassword = password;
            VerifyNames = nameVerification;
            ClientBrand = "libMC.NET"; // -- Used in the plugin message reporting the client brand to the server.
        }

        /// <summary>
        /// Login to Minecraft.net and store credentials
        /// </summary>
        public void Login() {
            if (VerifyNames) {
                var loginHandler = new Minecraft_Net_Interaction();
                string[] credentials = loginHandler.Login(ClientName, ClientPassword);

                if (credentials[0] == "") {  // -- Fall back to no auth.
                    RaiseError(this, "Failed to login to Minecraft.net! (Incorrect username or password)");

                    VerifyNames = false;
                } else {
                    RaiseInfo(this, "Logged in to Minecraft.net successfully.");

                    RaiseDebug(this, string.Format("Token: {0}\nProfile: {1}", credentials[0], credentials[1]));

                    AccessToken = credentials[0];
                    SelectedProfile = credentials[1];
                    ClientToken = credentials[2];
                    ClientName = credentials[3];
                }
            } else {
                AccessToken = "None";
                SelectedProfile = "None";
            }

        }
        /// <summary>
        /// Uses a client's stored credentials to verify with Minecraft.net
        /// </summary>
        public bool VerifySession() {
            if (AccessToken == null || ClientToken == null) {
                RaiseError(this, "Credentials are not set!");
                return false;
            }

            var SessionVerifier = new Minecraft_Net_Interaction();
            string[] Response = SessionVerifier.SessionRefresh(AccessToken, ClientToken);

            if (Response[0] == "") {
                RaiseError(this, "Unable to Verify Session!");
                return false;
            }

            RaiseInfo(this, "Credentials verified and refreshed!");

            AccessToken = Response[0];
            ClientToken = Response[1];
            SelectedProfile = "Potato";

            return true;
        }

        /// <summary>
        /// Uses a client's stored credentials to verify with Minecraft.net
        /// </summary>
        /// <param name="accessToken">Stored Access Token</param>
        /// <param name="clientToken">Stored Client Token</param>
        public bool VerifySession(string accessToken, string clientToken) {
            AccessToken = accessToken;
            ClientToken = clientToken;

            var SessionVerifier = new Minecraft_Net_Interaction();
            string[] Response = SessionVerifier.SessionRefresh(AccessToken, ClientToken);

            if (Response[0] == "") {
                RaiseError(this, "Unable to Verify Session!");
                return false;
            }

            RaiseInfo(this, "Credentials verified and refreshed!");

            AccessToken = Response[0];
            ClientToken = Response[1];

            return true;
        }

        /// <summary>
        /// Connects to the MinecraftClient Server.
        /// </summary>
        public void Connect() {
            if (nh != null)
                Disconnect();

            Players = new Dictionary<string,short>();

            nh = new NetworkHandler(this);

            // -- Register our event handlers.
            nh.InfoMessage += NetworkInfo;
            nh.DebugMessage += NetworkDebug;
            nh.SocketError += NetworkError;
            nh.PacketHandled += RaisePacketHandled;

            // -- Connect to the server and begin reading packets.

            nh.Start();

            RaiseDebug(this, "Network handler created, Connecting to server...");
        }

        /// <summary>
        /// Disconnects from the Minecraft server.
        /// </summary>
        public void Disconnect() {
            if (nh != null)
                nh.Stop();

            // -- Reset all variables to default so we can make a new connection.

            Running = false;
            ServerState = 0;
            nh = null;
            MinecraftWorld = null;
            ThisPlayer = null;
            Players = null;

            RaiseDebug(this, "Variables reset, disconnected from server.");
        }

        #region Simple Actions
        /// <summary>
        /// Sends a chat message to the server.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        public void SendChat(string Message) {
            if (nh != null) {
                var ChatPacket = new SBChatMessage();
                ChatPacket.Message = Message;
                ChatPacket.Write(nh.wSock);
            }
        }
        /// <summary>
        /// Respawns the client.
        /// </summary>
        public void Respawn() {
            var RespawnPacket = new SBClientStatus();
            RespawnPacket.ActionID = 0;
            RespawnPacket.Write(nh.wSock);
        }
        /// <summary>
        /// Receives a list of completion words from the server
        /// Note: Hook the TabComplete event to receive results!!
        /// </summary>
        /// <param name="Message">The message to receive completion items for.</param>
        public void TabComplete(string Message) {
            var CompletePacket = new SBTabComplete();
            CompletePacket.Text = Message;
            CompletePacket.Write(nh.wSock);
        }
        #endregion
        #region Event Messengers
        #region Server Events
        public void RaisePlayerlistAdd(string name, short ping) {
            if (PlayerListitemAdd != null)
                PlayerListitemAdd(name, ping);
        }

        public void RaisePlayerlistRemove(string name) {
            if (PlayerListitemRemove != null)
                PlayerListitemRemove(name);
        }

        public void RaisePlayerlistUpdate(string name, short ping) {
            if (PlayerListitemUpdate != null)
                PlayerListitemUpdate(name, ping);
        }

        public void RaisePluginMessage(string channel, byte[] data) {
            if (PluginMessage != null)
                PluginMessage(channel, data);
        }
        public void RaiseLoginSuccess(object Sender) {
            if (LoginSuccess != null)
                LoginSuccess(Sender);
        }

        public void RaiseLoginFailure(object Sender, string Reason) {
            if (LoginFailure != null)
                LoginFailure(Sender, Reason);
        }

        public void RaiseGameJoined() {
            if (JoinedGame != null)
                JoinedGame();
        }

        public void RaiseTransactionRejected(byte Window_ID, short Action_ID) {
            if (TransactionRejected != null)
                TransactionRejected(Window_ID, Action_ID);
        }

        public void RaiseTransactionAccepted(byte Window_ID, short Action_ID) {
            if (TransactionAccepted != null)
                TransactionAccepted(Window_ID, Action_ID);
        }

        public void RaiseKicked(string reason) {
            if (PlayerKicked != null)
                PlayerKicked(reason);
        }

        public void RaiseExplosion(float X, float Y, float Z) {
            if (Explosion != null)
                Explosion(X, Y, Z);
        }
        public void RaisePingResponse(string VersionName, int ProtocolVersion, int MaxPlayers, int OnlinePlayers, string[] PlayersSample, string MOTD, Image Favicon) {
            if (PingResponseReceived != null)
                PingResponseReceived(VersionName, ProtocolVersion, MaxPlayers, OnlinePlayers, PlayersSample, MOTD, Favicon);
        }
        public void RaisePingMs(int MsPing) {
            if (MsPingReceived != null)
                MsPingReceived(MsPing);
        }
        public void RaiseTabComplete(string[] results) {
            if (TabCompleteReceived != null)
                TabCompleteReceived(results);
        }
        #endregion
        #region Base Events
        void NetworkInfo(object Sender, string Message) {
            if (InfoMessage != null)
                InfoMessage(Sender, "(NETWORK): " + Message);
        }
        void NetworkDebug(object Sender, string Message) {
            if (DebugMessage != null)
                DebugMessage(Sender, "(NETWORK): " + Message);
        }
        void NetworkError(object Sender, string Message) {
            if (ErrorMessage != null)
                ErrorMessage(Sender, "(NETWORK): " + Message);
        }
        void RaisePacketHandled(object Sender, object Packet, int id) {
            if (PacketHandled != null)
                PacketHandled(Sender, Packet, id);
        }
        public void RaiseError(object Sender, string Message) {
            if (ErrorMessage != null)
                ErrorMessage(Sender, Message);
        }
        public void RaiseInfo(object Sender, string Message) {
            if (InfoMessage != null)
                InfoMessage(Sender, Message);
        }
        public void RaiseDebug(object Sender, string Message) {
            if (DebugMessage != null)
                DebugMessage(Sender, Message);
        }
        public void RaiseMC(object Sender, string McMessage, string Raw) {
            if (Message != null)
                Message(Sender, McMessage, Raw);
        }
        #endregion
        #region Block Events
        public void RaiseChestStateChange(byte state, int x, short y, int z) {
            if (ChestStateChanged != null)
                ChestStateChanged(state, x, y, z);
        }

        public void RaiseBlockBreakingEvent(Vector Location, int Entity_ID, sbyte Stage) {
            if (BlockBreaking != null)
                BlockBreaking(Location, Entity_ID, Stage);
        }

        public void RaiseBlockChangedEvent(int x, byte y, int z, int type, byte data) {
            if (BlockChanged != null)
                BlockChanged(x, y, z, type, data);
        }

        public void RaisePistonMoved(byte state, byte direction, int x, short y, int z) {
            if (PistonMoved != null)
                PistonMoved(state, direction, x, y, z);
        }

        #endregion
        #region World Events
        public void RaiseChunkUnload(int X, int Z) {
            if (ChunkUnloaded != null)
                ChunkUnloaded(X, Z);
        }

        public void RaiseChunkLoad(int X, int Z) {
            if (ChunkLoaded != null)
                ChunkLoaded(X, Z);
        }

        public void RaiseNoteBlockSound(byte instrument, byte pitch, int x, short y, int z) {
            if (NoteBlockPlay != null)
                NoteBlockPlay(instrument, pitch, x, y, z);
        }

        public void RaiseGameStateChanged(string eventName, float value) {
            if (GameStateChanged != null)
                GameStateChanged(eventName, value);
        }

        public void RaiseMultiBlockChange(int Chunk_X, int Chunk_Z) {
            if (MultiBlockChange != null)
                MultiBlockChange(Chunk_X, Chunk_Z);
        }
        #endregion
        #region Entity Events
        public void RaiseEntityAnimationChanged(object Sender, int Entity_ID, byte Animation) {
            if (EntityAnimationChanged != null)
                EntityAnimationChanged(Sender, Entity_ID, Animation);
        }
        public void RaiseEntityAttached(int Entity_ID, int Vehicle_ID, bool Leashed) {
            if (EntityAttached != null)
                EntityAttached(Entity_ID, Vehicle_ID, Leashed);
        }
        public void RaiseEntityDestruction(int Entity_ID) {
            if (EntityDestroyed != null)
                EntityDestroyed(Entity_ID);
        }
        public void RaiseEntityStatus(int Entity_ID) {
            if (EntityStatusChanged != null)
                EntityStatusChanged(Entity_ID);
        }

        public void RaiseEntityEquipment(int Entity_ID, int slot, Item newItem) {
            if (EntityEquipmentChanged != null)
                EntityEquipmentChanged(Entity_ID, slot, newItem);
        }

        public void RaiseEntityHeadLookChanged(int Entity_ID, sbyte head_yaw) {
            if (EntityHeadLookChanged != null)
                EntityHeadLookChanged(Entity_ID, head_yaw);
        }

        public void RaiseEntityLookChanged(int Entity_ID, sbyte yaw, sbyte pitch) {
            if (EntityLookChanged != null)
                EntityLookChanged(Entity_ID, yaw, pitch);
        }

        public void RaiseEntityRelMove(int Entity_ID, int Change_X, int Change_Y, int Change_Z) {
            if (EntityRelMove != null)
                EntityRelMove(Entity_ID, Change_X, Change_Y, Change_Z);
        }

        public void RaiseEntityTeleport(int Entity_ID, int X, int Y, int Z) {
            if (EntityTeleport != null)
                EntityTeleport(Entity_ID, X, Y, Z);
        }

        public void RaiseEntityVelocityChanged(int Entity_ID, int X, int Y, int Z) {
            if (EntityVelocityChanged != null)
                EntityVelocityChanged(Entity_ID, X, Y, Z);
        }
        #endregion
        #region Player Events
        public void RaiseWindowClosed(byte window_ID) {
            if (CloseWindow != null)
                CloseWindow(window_ID);
        }
        public void RaiseOpenWindow(byte Window_ID, byte Type, string Title, byte slots, bool useTitle) {
            if (OpenWindow != null)
                OpenWindow(Window_ID, Type, Title, slots, useTitle);
        }
        public void RaiseItemCollected(int item_EID, int collector_eid) {
            if (ItemCollected != null)
                ItemCollected(item_EID, collector_eid);
        }
        public void RaiseHeldSlotChanged(byte slot) {
            if (HeldSlotChanged != null)
                HeldSlotChanged(slot);
        }
        public void RaiseLocationChanged() {
            if (LocationChanged != null)
                LocationChanged();
        }
        public void RaisePlayerRespawn() {
            if (PlayerRespawned != null)
                PlayerRespawned();
        }
        public void RaiseExperienceUpdate(float expBar, short level, short totalExp) {
            if (experienceSet != null)
                experienceSet(expBar, level, totalExp);
        }

        public void RaiseSetWindowSlot(sbyte windowid, short slot, Item item) {
            if (SetWindowItem != null)
                SetWindowItem(windowid, slot, item);
        }

        public void RaiseInventoryItem(short slot, Item item) {
            if (SetInventoryItem != null)
                SetInventoryItem(slot, item);
        }

        public void RaisePlayerHealthUpdate(float health, short hunger, float saturation) {
            if (SetPlayerHealth != null)
                SetPlayerHealth(health, hunger, saturation);
        }
        #endregion
        #region Scoreboard Events
        public void RaiseScoreBoard(sbyte position, string name) {
            if (DisplayScoreboard != null)
                DisplayScoreboard(position, name);
        }

        public void RaiseScoreboardAdd(string name, string value) {
            if (ScoreboardObjectiveAdd != null)
                ScoreboardObjectiveAdd(name, value);
        }

        public void RaiseScoreboardRemove(string name) {
            if (ScoreboardObjectiveRemove != null)
                ScoreboardObjectiveRemove(name);
        }

        public void RaiseScoreboardUpdate(string name, string value) {
            if (scoreboardObjectiveUpdate != null)
                scoreboardObjectiveUpdate(name, value);
        }
        #endregion
        #endregion
        #region Event Delegates
        #region Base Events
        public delegate void DebugMessageHandler(object sender, string message);
        public event DebugMessageHandler DebugMessage;

        public delegate void ErrorMessageHandler(object sender, string message);
        public event ErrorMessageHandler ErrorMessage;

        public delegate void InfoMessageHandler(object sender, string message);
        public event InfoMessageHandler InfoMessage;

        public delegate void MessageHandler(object sender, string message, string raw);
        public event MessageHandler Message;

        public delegate void PacketHandler(object sender, object packet, int id);
        public event PacketHandler PacketHandled;
        #endregion
        #region Block Events
        public delegate void BlockChangedEventHandler(int x, byte y, int z, int newType, byte data);
        public event BlockChangedEventHandler BlockChanged;

        public delegate void BlockBreakAnimationHandler(Vector Location, int Entity_ID, sbyte Stage);
        public event BlockBreakAnimationHandler BlockBreaking;

        public delegate void PistonMoveHandler(byte state, byte direction, int x, short y, int z);
        public event PistonMoveHandler PistonMoved;

        public delegate void ChestStateChangedHandler(byte state, int x, short y, int z);
        public event ChestStateChangedHandler ChestStateChanged;
        #endregion
        #region World Events
        public delegate void NoteBlockPlayHandler(byte instrument, byte pitch, int x, short y, int z);
        public event NoteBlockPlayHandler NoteBlockPlay;

        public delegate void GameStateChangedHandler(string eventName, float value);
        public event GameStateChangedHandler GameStateChanged;

        public delegate void ChunkUnloadedHandler(int X, int Z);
        public event ChunkUnloadedHandler ChunkUnloaded;

        public delegate void ChunkLoadedHandler(int X, int Z);
        public event ChunkLoadedHandler ChunkLoaded;

        public delegate void ExplosionHandler(float X, float Y, float Z);
        public event ExplosionHandler Explosion;

        public delegate void MultiBlockChangeHandler(int Chunk_X, int Chunk_Z);
        public event MultiBlockChangeHandler MultiBlockChange;
        #endregion
        #region Entity Events
        public delegate void EntityVelocityChangedHandler(int Entity_ID, int X, int Y, int Z);
        public event EntityVelocityChangedHandler EntityVelocityChanged;

        public delegate void EntityTeleportHandler(int Entity_ID, int X, int Y, int Z);
        public event EntityTeleportHandler EntityTeleport;

        public delegate void EntityRelMoveHandler(int Entity_ID, int Change_X, int Change_Y, int Change_Z);
        public event EntityRelMoveHandler EntityRelMove;

        public delegate void EntityLookChangedHandler(int Entity_ID, sbyte yaw, sbyte pitch);
        public event EntityLookChangedHandler EntityLookChanged;

        public delegate void EntityHeadLookChangedHandler(int Entity_ID, sbyte head_yaw);
        public event EntityHeadLookChangedHandler EntityHeadLookChanged;

        public delegate void EntityEquipmentChangedHandler(int Entity_ID, int slot, Item newItem);
        public event EntityEquipmentChangedHandler EntityEquipmentChanged;

        public delegate void EntityAnimationChangedHandler(object sender, int Entity_ID, byte Animation);
        public event EntityAnimationChangedHandler EntityAnimationChanged;

        public delegate void EntityAttachedHandler(int Entity_ID, int Vehicle_ID, bool Leashed);
        public event EntityAttachedHandler EntityAttached;

        public delegate void ItemCollectedHandler(int item_EID, int collector_eid);
        public event ItemCollectedHandler ItemCollected;

        public delegate void EntityDestroyedHandler(int Entity_ID);
        public event EntityDestroyedHandler EntityDestroyed;

        public delegate void EntityStatusChangedHandler(int Entity_ID);
        public event EntityStatusChangedHandler EntityStatusChanged;
        #endregion
        #region Player Events
        public delegate void ExperienceSetHandler(float expBar, short level, short totalExp);
        public event ExperienceSetHandler experienceSet;

        public delegate void PlayerRespawnedHandler();
        public event PlayerRespawnedHandler PlayerRespawned;

        public delegate void LocationChangedHandler();
        public event LocationChangedHandler LocationChanged;

        public delegate void OpenWindowHandler(byte Window_ID, byte Type, string Title, byte slots, bool useTitle);
        public event OpenWindowHandler OpenWindow;

        public delegate void CloseWindowHandler(byte windowID);
        public event CloseWindowHandler CloseWindow;

        public delegate void HeldSlotChangedHandler(byte slot);
        public event HeldSlotChangedHandler HeldSlotChanged;

        public delegate void SetWindowItemHandler(sbyte window_ID, short slot, Item item);
        public event SetWindowItemHandler SetWindowItem;

        public delegate void SetInventoryItemHandler(short slot, Item item);
        public event SetInventoryItemHandler SetInventoryItem;

        public delegate void SetPlayerHealthHandler(float health, short hunger, float saturation);
        public event SetPlayerHealthHandler SetPlayerHealth;

        #endregion
        #region Scoreboard Events
        public delegate void ScoreboardObjectiveAddHandler(string name, string value);
        public event ScoreboardObjectiveAddHandler ScoreboardObjectiveAdd;

        public delegate void ScoreboardObjectiveUpdateHandler(string name, string value);
        public event ScoreboardObjectiveUpdateHandler scoreboardObjectiveUpdate;

        public delegate void ScoreboardObjectiveRemoveHandler(string name);
        public event ScoreboardObjectiveRemoveHandler ScoreboardObjectiveRemove;

        public delegate void DisplayScoreboardHandler(sbyte position, string scoreName);
        public event DisplayScoreboardHandler DisplayScoreboard;
        #endregion
        #region Server Events
        public delegate void PluginMessageHandler(string channel, byte[] data);
        public event PluginMessageHandler PluginMessage;

        public delegate void PlayerListitemAddHandler(string name, short ping);
        public event PlayerListitemAddHandler PlayerListitemAdd;

        public delegate void PlayerListitemRemoveHandler(string name);
        public event PlayerListitemRemoveHandler PlayerListitemRemove;

        public delegate void PlayerListitemUpdateHandler(string name, short ping);
        public event PlayerListitemUpdateHandler PlayerListitemUpdate;

        public delegate void LoginSuccessHandler(object sender);
        public event LoginSuccessHandler LoginSuccess;

        public delegate void LoginFailureHandler(object sender, string reason);
        public event LoginFailureHandler LoginFailure;

        public delegate void JoinGameHandler();
        public event JoinGameHandler JoinedGame;

        public delegate void TransactionRejectedHandler(byte Window_ID, short Action_ID);
        public event TransactionRejectedHandler TransactionRejected;

        public delegate void TransactionAcceptedHandler(byte Window_ID, short Action_ID);
        public event TransactionAcceptedHandler TransactionAccepted;

        public delegate void PlayerKickedHandler(string reason);
        public event PlayerKickedHandler PlayerKicked;

        public delegate void PingMsReceivedHandler(int msPing);
        public event PingMsReceivedHandler MsPingReceived;

        public delegate void PingResponseReceivedHandler(string VersionName, int ProtocolVersion, int MaxPlayers, int OnlinePlayers, string[] PlayersSample, string MOTD, Image Favicon);
        public event PingResponseReceivedHandler PingResponseReceived;

        public delegate void TabCompleteReceivedHandler(string[] results);
        public event TabCompleteReceivedHandler TabCompleteReceived;
        #endregion
        #endregion
    }
}
