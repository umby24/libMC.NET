using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libMC.NET.Common;
using libMC.NET.World;
using libMC.NET.Entities;

// TODO: Convert cases likeThis to LikeThis.
// TODO: Comment more things
// TODO: Speed things up, optimize code.

// [Low]: Refactor packets to be universal for Server/Client, and be usable with proxies
namespace libMC.NET {
    /// <summary>
    /// Main class for libMC.Net, a Minecraft interaction library for .net languages.
    /// </summary>
    public class Minecraft {
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
        /// Create a new Minecraft Instance
        /// </summary>
        /// <param name="ip">The IP of the server to connect to</param>
        /// <param name="port">The port of the server to connect to</param>
        /// <param name="username">The username to use when connecting to Minecraft</param>
        /// <param name="password">The password to use when connecting to Minecraft (Ignore if you are providing credentials)</param>
        /// <param name="nameVerification">To connect using Name Verification or not</param>
        public Minecraft(string ip, int port, string username, string password, bool nameVerification) {
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
                Minecraft_Net_Interaction loginHandler = new Minecraft_Net_Interaction();
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
        /// Connects to the Minecraft Server.
        /// </summary>
        public void Connect() {
            if (nh != null)
                Disconnect();

            Players = new Dictionary<string,short>();

            nh = new NetworkHandler(this);

            // -- Register our event handlers.
            nh.InfoMessage += NetworkInfo;
            nh.debugMessage += NetworkDebug;
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
        public void SendChat(string Message) {
            if (nh != null) {
                Packets.Play.ServerBound.chatMessage.sendChat(this, Message);
            }
        }
        #endregion
        #region Event Messengers
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

        /// <summary>
        /// Raises a Minecraft Error from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Error Message</param>
        public void RaiseError(object Sender, string Message) {
            if (ErrorMessage != null)
                ErrorMessage(Sender, Message);
        }
        /// <summary>
        /// Raises Minecraft Info from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Info Message</param>
        public void RaiseInfo(object Sender, string Message) {
            if (InfoMessage != null)
                InfoMessage(Sender, Message);
        }
        /// <summary>
        /// Raises Minecraft debug message from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Debug message</param>
        public void RaiseDebug(object Sender, string Message) {
            if (DebugMessage != null)
                DebugMessage(Sender, Message);
        }
        /// <summary>
        /// Raises a normal minecraft message (such as chat)
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="Message">Minecraft message</param>
        public void RaiseMC(object Sender, string McMessage, string Name) {
            if (Message != null)
                Message(Sender, McMessage, Name);
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

        public void RaiseEntityAnimationChanged(object Sender, int Entity_ID, byte Animation) {
            if (EntityAnimationChanged != null)
                EntityAnimationChanged(Sender, Entity_ID, Animation);
        }
        public void raiseEntityAttached(int Entity_ID, int Vehicle_ID, bool Leashed) {
            if (entityAttached != null)
                entityAttached(Entity_ID, Vehicle_ID, Leashed);
        }

        public void raiseNoteBlockSound(byte instrument, byte pitch, int x, short y, int z) {
            if (noteBlockPlay != null)
                noteBlockPlay(instrument, pitch, x, y, z);
        }

        public void raisePistonMoved(byte state, byte direction, int x, short y, int z) {
            if (pistonMoved != null)
                pistonMoved(state, direction, x, y, z);
        }

        public void raiseChestStateChange(byte state, int x, short y, int z) {
            if (chestStateChanged != null)
                chestStateChanged(state, x, y, z);
        }

        public void raiseBlockBreakingEvent(Vector Location, int Entity_ID, byte Stage) {
            if (blockBreaking != null)
                blockBreaking(Location, Entity_ID, Stage);
        }

        public void raiseBlockChangedEvent(int x, byte y, int z, int type, byte data) {
            if (blockChanged != null)
                blockChanged(x, y, z, type, data);
        }

        public void raiseGameStateChanged(string eventName, float value) {
            if (gameStateChanged != null)
                gameStateChanged(eventName, value);
        }

        public void raiseChunkUnload(int X, int Z) {
            if (chunkUnloaded != null)
                chunkUnloaded(X, Z);
        }

        public void raiseChunkLoad(int X, int Z) {
            if (chunkLoaded != null)
                chunkLoaded(X, Z);
        }

        public void raiseWindowClosed(byte window_ID) {
            if (closeWindow != null)
                closeWindow(window_ID);
        }

        public void raiseItemCollected(int item_EID, int collector_eid) {
            if (itemCollected != null)
                itemCollected(item_EID, collector_eid);
        }

        public void raiseTransactionRejected(byte Window_ID, short Action_ID) {
            if (TransactionRejected != null)
                TransactionRejected(Window_ID, Action_ID);
        }

        public void raiseTransactionAccepted(byte Window_ID, short Action_ID) {
            if (TransactionAccepted != null)
                TransactionAccepted(Window_ID, Action_ID);
        }

        public void raiseEntityDestruction(int Entity_ID) {
            if (entityDestroyed != null)
                entityDestroyed(Entity_ID);
        }

        public void raiseKicked(string reason) {
            if (PlayerKicked != null)
                PlayerKicked(reason);
        }

        public void raiseScoreBoard(byte position, string name) {
            if (displayScoreboard != null)
                displayScoreboard(position, name);
        }

        public void raiseEntityStatus(int Entity_ID) {
            if (entityStatusChanged != null)
                entityStatusChanged(Entity_ID);
        }

        public void raiseEntityEquipment(int Entity_ID, int slot, Item newItem) {
            if (entityEquipmentChanged != null)
                entityEquipmentChanged(Entity_ID, slot, newItem);
        }

        public void raiseEntityHeadLookChanged(int Entity_ID, byte head_yaw) {
            if (entityHeadLookChanged != null)
                entityHeadLookChanged(Entity_ID, head_yaw);
        }

        public void raiseEntityLookChanged(int Entity_ID, byte yaw, byte pitch) {
            if (entityLookChanged != null)
                entityLookChanged(Entity_ID, yaw, pitch);
        }

        public void raiseEntityRelMove(int Entity_ID, int Change_X, int Change_Y, int Change_Z) {
            if (entityRelMove != null)
                entityRelMove(Entity_ID, Change_X, Change_Y, Change_Z);
        }

        public void raiseEntityTeleport(int Entity_ID, int X, int Y, int Z) {
            if (entityTeleport != null)
                entityTeleport(Entity_ID, X, Y, Z);
        }

        public void raiseEntityVelocityChanged(int Entity_ID, int X, int Y, int Z) {
            if (entityVelocityChanged != null)
                entityVelocityChanged(Entity_ID, X, Y, Z);
        }

        public void raiseExplosion(float X, float Y, float Z) {
            if (explosion != null)
                explosion(X, Y, Z);
        }

        public void raiseHeldSlotChanged(byte slot) {
            if (heldSlotChanged != null)
                heldSlotChanged(slot);
        }

        public void raiseMultiBlockChange(int Chunk_X, int Chunk_Z) {
            if (multiBlockChange != null)
                multiBlockChange(Chunk_X, Chunk_Z);
        }

        public void raiseOpenWindow(byte Window_ID, byte Type, string Title, byte slots, bool useTitle) {
            if (openWindow != null)
                openWindow(Window_ID, Type, Title, slots, useTitle);
        }

        public void raisePlayerlistAdd(string name, short ping) {
            if (PlayerListitemAdd != null)
                PlayerListitemAdd(name, ping);
        }

        public void raisePlayerlistRemove(string name) {
            if (PlayerListitemRemove != null)
                PlayerListitemRemove(name);
        }

        public void raisePlayerlistUpdate(string name, short ping) {
            if (PlayerListitemUpdate != null)
                PlayerListitemUpdate(name, ping);
        }

        public void raiseLocationChanged() {
            if (locationChanged != null)
                locationChanged();
        }

        public void raisePluginMessage(string channel, byte[] data) {
            if (PluginMessage != null)
                PluginMessage(channel, data);
        }

        public void raisePlayerRespawn() {
            if (playerRespawned != null)
                playerRespawned();
        }

        public void raiseScoreboardAdd(string name, string value) {
            if (scoreboardObjectiveAdd != null)
                scoreboardObjectiveAdd(name, value);
        }

        public void raiseScoreboardRemove(string name) {
            if (scoreboardObjectiveRemove != null)
                scoreboardObjectiveRemove(name);
        }

        public void raiseScoreboardUpdate(string name, string value) {
            if (scoreboardObjectiveUpdate != null)
                scoreboardObjectiveUpdate(name, value);
        }

        public void raiseExperienceUpdate(float expBar, short level, short totalExp) {
            if (experienceSet != null)
                experienceSet(expBar, level, totalExp);
        }

        public void raiseSetWindowSlot(byte windowid, short slot, Item item) {
            if (setWindowItem != null)
                setWindowItem(windowid, slot, item);
        }

        public void raiseInventoryItem(short slot, Item item) {
            if (setInventoryItem != null)
                setInventoryItem(slot, item);
        }

        public void raisePlayerHealthUpdate(float health, short hunger, float saturation) {
            if (setPlayerHealth != null)
                setPlayerHealth(health, hunger, saturation);
        }
        #endregion
        #region Event Delegates
        
        #region Base Events
        public delegate void DebugMessageHandler(object sender, string message);
        public event DebugMessageHandler DebugMessage;

        public delegate void ErrorMessageHandler(object sender, string message);
        public event ErrorMessageHandler ErrorMessage;

        public delegate void InfoMessageHandler(object sender, string message);
        public event InfoMessageHandler InfoMessage;

        public delegate void MessageHandler(object sender, string message, string name);
        public event MessageHandler Message;

        public delegate void PacketHandler(object sender, object packet, int id);
        public event PacketHandler PacketHandled;
        #endregion

        #region Block Events
        public delegate void blockChangedEventHandler(int x, byte y, int z, int newType, byte data);
        public event blockChangedEventHandler blockChanged;

        public delegate void blockBreakAnimationHandler(Vector Location, int Entity_ID, byte Stage);
        public event blockBreakAnimationHandler blockBreaking;

        public delegate void pistonMoveHandler(byte state, byte direction, int x, short y, int z);
        public event pistonMoveHandler pistonMoved;

        public delegate void chestStateChangedHandler(byte state, int x, short y, int z);
        public event chestStateChangedHandler chestStateChanged;
        #endregion
        #region World Events
        public delegate void noteBlockPlayHandler(byte instrument, byte pitch, int x, short y, int z);
        public event noteBlockPlayHandler noteBlockPlay;

        public delegate void gameStateChangedHandler(string eventName, float value);
        public event gameStateChangedHandler gameStateChanged;

        public delegate void chunkUnloadedHandler(int X, int Z);
        public event chunkUnloadedHandler chunkUnloaded;

        public delegate void chunkLoadedHandler(int X, int Z);
        public event chunkLoadedHandler chunkLoaded;

        public delegate void explosionHandler(float X, float Y, float Z);
        public event explosionHandler explosion;

        public delegate void multiBlockChangeHandler(int Chunk_X, int Chunk_Z);
        public event multiBlockChangeHandler multiBlockChange;
        #endregion

        #region Entity Events
        public delegate void entityVelocityChangedHandler(int Entity_ID, int X, int Y, int Z);
        public event entityVelocityChangedHandler entityVelocityChanged;

        public delegate void entityTeleportHandleR(int Entity_ID, int X, int Y, int Z);
        public event entityTeleportHandleR entityTeleport;

        public delegate void entityRelMoveHandler(int Entity_ID, int Change_X, int Change_Y, int Change_Z);
        public event entityRelMoveHandler entityRelMove;

        public delegate void entityLookChangedHandler(int Entity_ID, byte yaw, byte pitch);
        public event entityLookChangedHandler entityLookChanged;

        public delegate void entityHeadLookChangedHandler(int Entity_ID, byte head_yaw);
        public event entityHeadLookChangedHandler entityHeadLookChanged;

        public delegate void entityEquipmentChangedHandler(int Entity_ID, int slot, Item newItem);
        public event entityEquipmentChangedHandler entityEquipmentChanged;

        public delegate void entityAnimationChangedHandler(object sender, int Entity_ID, byte Animation);
        public event entityAnimationChangedHandler EntityAnimationChanged;

        public delegate void entityAttachedHandler(int Entity_ID, int Vehicle_ID, bool Leashed);
        public event entityAttachedHandler entityAttached;

        public delegate void itemCollectedHandler(int item_EID, int collector_eid);
        public event itemCollectedHandler itemCollected;

        public delegate void entityDestroyedHandler(int Entity_ID);
        public event entityDestroyedHandler entityDestroyed;

        public delegate void entityStatusChangedHandler(int Entity_ID);
        public event entityStatusChangedHandler entityStatusChanged;
        #endregion

        #region Player Events
        public delegate void ExperienceSetHandler(float expBar, short level, short totalExp);
        public event ExperienceSetHandler experienceSet;

        public delegate void playerRespawnedHandler();
        public event playerRespawnedHandler playerRespawned;

        public delegate void locationChangedHandler();
        public event locationChangedHandler locationChanged;

        public delegate void openWindowHandler(byte Window_ID, byte Type, string Title, byte slots, bool useTitle);
        public event openWindowHandler openWindow;

        public delegate void closeWindowHandler(byte windowID);
        public event closeWindowHandler closeWindow;

        public delegate void heldSlotChangedHandler(byte slot);
        public event heldSlotChangedHandler heldSlotChanged;

        public delegate void setWindowItemHandler(byte window_ID, short slot, Item item);
        public event setWindowItemHandler setWindowItem;

        public delegate void setInventoryItemHandler(short slot, Item item);
        public event setInventoryItemHandler setInventoryItem;

        public delegate void setPlayerHealthHandleR(float health, short hunger, float saturation);
        public event setPlayerHealthHandleR setPlayerHealth;

        #endregion
        #region Scoreboard Events
        public delegate void scoreboardObjectiveAddHandler(string name, string value);
        public event scoreboardObjectiveAddHandler scoreboardObjectiveAdd;

        public delegate void scoreboardObjectiveUpdateHandler(string name, string value);
        public event scoreboardObjectiveUpdateHandler scoreboardObjectiveUpdate;

        public delegate void scoreboardObjectiveRemoveHandleR(string name);
        public event scoreboardObjectiveRemoveHandleR scoreboardObjectiveRemove;

        public delegate void displayScoreboardHandler(byte position, string scoreName);
        public event displayScoreboardHandler displayScoreboard;
        #endregion

        // -- Server Events (Connected, Disconnected, Kicked).

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
        #endregion
        #endregion
    }
}
