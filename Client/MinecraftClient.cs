using System.Collections.Generic;
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
        public string ServerIp, ClientName, ClientPassword, AccessToken, ClientToken, SelectedProfile, ClientBrand;
        public int ServerPort, ServerState;
        public bool VerifyNames, Running, First = false;
        public NetworkHandler Nh;

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
            ServerIp = ip;
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
                var credentials = loginHandler.Login(ClientName, ClientPassword);

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

            var sessionVerifier = new Minecraft_Net_Interaction();
            var response = sessionVerifier.SessionRefresh(AccessToken, ClientToken);

            if (response[0] == "") {
                RaiseError(this, "Unable to Verify Session!");
                return false;
            }

            RaiseInfo(this, "Credentials verified and refreshed!");

            AccessToken = response[0];
            ClientToken = response[1];
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

            var sessionVerifier = new Minecraft_Net_Interaction();
            var response = sessionVerifier.SessionRefresh(AccessToken, ClientToken);

            if (response[0] == "") {
                RaiseError(this, "Unable to Verify Session!");
                return false;
            }

            RaiseInfo(this, "Credentials verified and refreshed!");

            AccessToken = response[0];
            ClientToken = response[1];

            return true;
        }

        /// <summary>
        /// Connects to the MinecraftClient Server.
        /// </summary>
        public void Connect() {
            if (Nh != null)
                Disconnect();

            Players = new Dictionary<string,short>();

            Nh = new NetworkHandler(this);

            // -- Register our event handlers.
            Nh.InfoMessage += NetworkInfo;
            Nh.DebugMessage += NetworkDebug;
            Nh.SocketError += NetworkError;
            Nh.PacketHandled += RaisePacketHandled;

            // -- Connect to the server and begin reading packets.

            Nh.Start();

            RaiseDebug(this, "Network handler created, Connecting to server...");
        }

        /// <summary>
        /// Disconnects from the Minecraft server.
        /// </summary>
        public void Disconnect() {
            if (Nh != null)
                Nh.Stop();

            // -- Reset all variables to default so we can make a new connection.

            Running = false;
            ServerState = 0;
            Nh = null;
            MinecraftWorld = null;
            ThisPlayer = null;
            Players = null;

            RaiseDebug(this, "Variables reset, disconnected from server.");
        }

        #region Simple Actions
        /// <summary>
        /// Sends a chat message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendChat(string message) {
            if (Nh == null) return;

            var chatPacket = new SBChatMessage {Message = message};
            chatPacket.Write(Nh.wSock);
        }
        /// <summary>
        /// Respawns the client.
        /// </summary>
        public void Respawn() {
            var respawnPacket = new SBClientStatus {ActionID = 0};
            respawnPacket.Write(Nh.wSock);
        }
        /// <summary>
        /// Receives a list of completion words from the server
        /// Note: Hook the TabComplete event to receive results!!
        /// </summary>
        /// <param name="message">The message to receive completion items for.</param>
        public void TabComplete(string message) {
            var completePacket = new SBTabComplete {Text = message};
            completePacket.Write(Nh.wSock);
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
        public void RaiseLoginSuccess(object sender) {
            if (LoginSuccess != null)
                LoginSuccess(sender);
        }

        public void RaiseLoginFailure(object sender, string reason) {
            if (LoginFailure != null)
                LoginFailure(sender, reason);
        }

        public void RaiseGameJoined() {
            if (JoinedGame != null)
                JoinedGame();
        }

        public void RaiseTransactionRejected(byte windowId, short actionId) {
            if (TransactionRejected != null)
                TransactionRejected(windowId, actionId);
        }

        public void RaiseTransactionAccepted(byte windowId, short actionId) {
            if (TransactionAccepted != null)
                TransactionAccepted(windowId, actionId);
        }

        public void RaiseKicked(string reason) {
            if (PlayerKicked != null)
                PlayerKicked(reason);
        }

        public void RaiseExplosion(float x, float y, float z) {
            if (Explosion != null)
                Explosion(x, y, z);
        }
        public void RaisePingResponse(string versionName, int protocolVersion, int maxPlayers, int onlinePlayers, string[] playersSample, string motd, Image favicon) {
            if (PingResponseReceived != null)
                PingResponseReceived(versionName, protocolVersion, maxPlayers, onlinePlayers, playersSample, motd, favicon);
        }
        public void RaisePingMs(int msPing) {
            if (MsPingReceived != null)
                MsPingReceived(msPing);
        }
        public void RaiseTabComplete(string[] results) {
            if (TabCompleteReceived != null)
                TabCompleteReceived(results);
        }
        #endregion
        #region Base Events
        void NetworkInfo(object sender, string message) {
            if (InfoMessage != null)
                InfoMessage(sender, "(NETWORK): " + message);
        }
        void NetworkDebug(object sender, string message) {
            if (DebugMessage != null)
                DebugMessage(sender, "(NETWORK): " + message);
        }
        void NetworkError(object sender, string message) {
            if (ErrorMessage != null)
                ErrorMessage(sender, "(NETWORK): " + message);
        }
        void RaisePacketHandled(object sender, object packet, int id) {
            if (PacketHandled != null)
                PacketHandled(sender, packet, id);
        }
        public void RaiseError(object sender, string message) {
            if (ErrorMessage != null)
                ErrorMessage(sender, message);
        }
        public void RaiseInfo(object sender, string message) {
            if (InfoMessage != null)
                InfoMessage(sender, message);
        }
        public void RaiseDebug(object sender, string message) {
            if (DebugMessage != null)
                DebugMessage(sender, message);
        }
        public void RaiseMc(object sender, string mcMessage, string raw) {
            if (Message != null)
                Message(sender, mcMessage, raw);
        }
        #endregion
        #region Block Events
        public void RaiseChestStateChange(byte state, int x, short y, int z) {
            if (ChestStateChanged != null)
                ChestStateChanged(state, x, y, z);
        }
         
        public void RaiseBlockBreakingEvent(Vector location, int entityId, sbyte stage) {
            if (BlockBreaking != null)
                BlockBreaking(location, entityId, stage);
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
        public void RaiseChunkUnload(int x, int z) {
            if (ChunkUnloaded != null)
                ChunkUnloaded(x, z);
        }

        public void RaiseChunkLoad(int x, int z) {
            if (ChunkLoaded != null)
                ChunkLoaded(x, z);
        }

        public void RaiseNoteBlockSound(byte instrument, byte pitch, int x, short y, int z) {
            if (NoteBlockPlay != null)
                NoteBlockPlay(instrument, pitch, x, y, z);
        }

        public void RaiseGameStateChanged(string eventName, float value) {
            if (GameStateChanged != null)
                GameStateChanged(eventName, value);
        }

        public void RaiseMultiBlockChange(int chunkX, int chunkZ) {
            if (MultiBlockChange != null)
                MultiBlockChange(chunkX, chunkZ);
        }
        #endregion
        #region Entity Events
        public void RaiseEntityAnimationChanged(object sender, int entityId, byte animation) {
            if (EntityAnimationChanged != null)
                EntityAnimationChanged(sender, entityId, animation);
        }
        public void RaiseEntityAttached(int entityId, int vehicleId, bool leashed) {
            if (EntityAttached != null)
                EntityAttached(entityId, vehicleId, leashed);
        }
        public void RaiseEntityDestruction(int entityId) {
            if (EntityDestroyed != null)
                EntityDestroyed(entityId);
        }
        public void RaiseEntityStatus(int entityId) {
            if (EntityStatusChanged != null)
                EntityStatusChanged(entityId);
        }

        public void RaiseEntityEquipment(int entityId, int slot, Item newItem) {
            if (EntityEquipmentChanged != null)
                EntityEquipmentChanged(entityId, slot, newItem);
        }

        public void RaiseEntityHeadLookChanged(int entityId, sbyte headYaw) {
            if (EntityHeadLookChanged != null)
                EntityHeadLookChanged(entityId, headYaw);
        }

        public void RaiseEntityLookChanged(int entityId, sbyte yaw, sbyte pitch) {
            if (EntityLookChanged != null)
                EntityLookChanged(entityId, yaw, pitch);
        }

        public void RaiseEntityRelMove(int entityId, int changeX, int changeY, int changeZ) {
            if (EntityRelMove != null)
                EntityRelMove(entityId, changeX, changeY, changeZ);
        }

        public void RaiseEntityTeleport(int entityId, int x, int y, int z) {
            if (EntityTeleport != null)
                EntityTeleport(entityId, x, y, z);
        }

        public void RaiseEntityVelocityChanged(int entityId, int x, int y, int z) {
            if (EntityVelocityChanged != null)
                EntityVelocityChanged(entityId, x, y, z);
        }
        #endregion
        #region Player Events 
        public void RaiseWindowClosed(byte windowId) {
            if (CloseWindow != null)
                CloseWindow(windowId);
        }
        public void RaiseOpenWindow(byte windowId, byte type, string title, byte slots, bool useTitle) {
            if (OpenWindow != null)
                OpenWindow(windowId, type, title, slots, useTitle);
        }
        public void RaiseItemCollected(int itemEid, int collectorEid) {
            if (ItemCollected != null)
                ItemCollected(itemEid, collectorEid);
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
            if (ExperienceSet != null)
                ExperienceSet(expBar, level, totalExp);
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
            if (ScoreboardObjectiveUpdate != null)
                ScoreboardObjectiveUpdate(name, value);
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

        public delegate void BlockBreakAnimationHandler(Vector location, int entityId, sbyte stage);
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

        public delegate void ChunkUnloadedHandler(int x, int z);
        public event ChunkUnloadedHandler ChunkUnloaded;

        public delegate void ChunkLoadedHandler(int x, int z);
        public event ChunkLoadedHandler ChunkLoaded;

        public delegate void ExplosionHandler(float x, float y, float z);
        public event ExplosionHandler Explosion;

        public delegate void MultiBlockChangeHandler(int chunkX, int chunkZ);
        public event MultiBlockChangeHandler MultiBlockChange;
        #endregion
        #region Entity Events
        public delegate void EntityVelocityChangedHandler(int entityId, int x, int y, int z);
        public event EntityVelocityChangedHandler EntityVelocityChanged;

        public delegate void EntityTeleportHandler(int entityId, int x, int y, int z);
        public event EntityTeleportHandler EntityTeleport;

        public delegate void EntityRelMoveHandler(int entityId, int changeX, int changeY, int changeZ);
        public event EntityRelMoveHandler EntityRelMove;

        public delegate void EntityLookChangedHandler(int entityId, sbyte yaw, sbyte pitch);
        public event EntityLookChangedHandler EntityLookChanged;

        public delegate void EntityHeadLookChangedHandler(int entityId, sbyte headYaw);
        public event EntityHeadLookChangedHandler EntityHeadLookChanged;

        public delegate void EntityEquipmentChangedHandler(int entityId, int slot, Item newItem);
        public event EntityEquipmentChangedHandler EntityEquipmentChanged;

        public delegate void EntityAnimationChangedHandler(object sender, int entityId, byte animation);
        public event EntityAnimationChangedHandler EntityAnimationChanged;

        public delegate void EntityAttachedHandler(int entityId, int vehicleId, bool leashed);
        public event EntityAttachedHandler EntityAttached;

        public delegate void ItemCollectedHandler(int itemEid, int collectorEid);
        public event ItemCollectedHandler ItemCollected;

        public delegate void EntityDestroyedHandler(int entityId);
        public event EntityDestroyedHandler EntityDestroyed;

        public delegate void EntityStatusChangedHandler(int entityId);
        public event EntityStatusChangedHandler EntityStatusChanged;
        #endregion
        #region Player Events
        public delegate void ExperienceSetHandler(float expBar, short level, short totalExp);
        public event ExperienceSetHandler ExperienceSet;

        public delegate void PlayerRespawnedHandler();
        public event PlayerRespawnedHandler PlayerRespawned;

        public delegate void LocationChangedHandler();
        public event LocationChangedHandler LocationChanged;

        public delegate void OpenWindowHandler(byte windowId, byte type, string title, byte slots, bool useTitle);
        public event OpenWindowHandler OpenWindow;

        public delegate void CloseWindowHandler(byte windowId);
        public event CloseWindowHandler CloseWindow;

        public delegate void HeldSlotChangedHandler(byte slot);
        public event HeldSlotChangedHandler HeldSlotChanged;

        public delegate void SetWindowItemHandler(sbyte windowId, short slot, Item item);
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
        public event ScoreboardObjectiveUpdateHandler ScoreboardObjectiveUpdate;

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

        public delegate void TransactionRejectedHandler(byte windowId, short actionId);
        public event TransactionRejectedHandler TransactionRejected;

        public delegate void TransactionAcceptedHandler(byte windowId, short actionId);
        public event TransactionAcceptedHandler TransactionAccepted;

        public delegate void PlayerKickedHandler(string reason);
        public event PlayerKickedHandler PlayerKicked;

        public delegate void PingMsReceivedHandler(int msPing);
        public event PingMsReceivedHandler MsPingReceived;

        public delegate void PingResponseReceivedHandler(string versionName, int protocolVersion, int maxPlayers, int onlinePlayers, string[] playersSample, string motd, Image favicon);
        public event PingResponseReceivedHandler PingResponseReceived;

        public delegate void TabCompleteReceivedHandler(string[] results);
        public event TabCompleteReceivedHandler TabCompleteReceived;
        #endregion
        #endregion
    }
}
