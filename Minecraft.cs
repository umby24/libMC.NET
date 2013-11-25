using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libMC.NET {
    /// <summary>
    /// Main class for libMC.Net, a Minecraft interaction library for .net languages.
    /// </summary>
    public class Minecraft {
        #region Variables
        public string serverIP, clientName, clientPassword, accessToken, selectedProfile;
        public int serverPort, serverState;
        public bool verifyNames, running;
        public networkHandler nh;

        #region Trackers
        public Classes.World minecraftWorld; // -- Holds all of the world information. Time, chunks, players, ect.
        public Classes.Player thisPlayer; // -- Holds all user information, location, inventory and so on.
        public Dictionary<string, short> players;
        #endregion
        #endregion

        /// <summary>
        /// Create a new Minecraft Instance
        /// </summary>
        /// <param name="ip">The IP of the server to connect to</param>
        /// <param name="port">The port of the server to connect to</param>
        /// <param name="username">The username to use when connecting to Minecraft</param>
        /// <param name="password">The password to use when connecting to Minecraft</param>
        /// <param name="nameVerification">To connect using Name Verification or not</param>
        public Minecraft(string ip, int port, string username, string password, bool nameVerification) {
            serverIP = ip;
            serverPort = port;
            clientName = username;
            clientPassword = password;
            verifyNames = nameVerification;
        }

        /// <summary>
        /// Login to Minecraft.net and store credentials
        /// </summary>
        public void Login() {
            if (verifyNames) {
                Minecraft_Net_Interaction loginHandler = new Minecraft_Net_Interaction();
                string[] credentials = loginHandler.Login(clientName, clientPassword);

                if (credentials[0] == "") {  // -- Fall back to no auth.
                    raiseError(this, "Failed to login to Minecraft.net! (Incorrect username or password)");

                    verifyNames = false;
                } else {
                    raiseInfo(this, "Logged in to Minecraft.net successfully.");

                    raiseDebug(this, string.Format("Token: {0}\nProfile: {1}", credentials[0], credentials[1]));

                    accessToken = credentials[0];
                    selectedProfile = credentials[1];
                }

            } else {
                accessToken = "Asdf";
                selectedProfile = "asd";
            }
        
        }
        
        /// <summary>
        /// Connects to the Minecraft Server.
        /// </summary>
        public void Connect() {
            if (nh != null)
                Disconnect();

            players = new Dictionary<string,short>();

            nh = new networkHandler(this);

            // -- Register our event handlers

            nh.infoMessage += new networkHandler.networkInfoHandler(networkInfo);
            nh.debugMessage += new networkHandler.networkDebugHandler(networkDebug);
            nh.socketError += new networkHandler.socketErrorHandler(networkError);
            nh.PacketHandled += new networkHandler.packetHandledHandler(raisePacketHandled);
            // -- Connect to the server and begin reading packets

            nh.Start();

            raiseDebug(this, "Network handler created, Connecting to server...");
        }

        /// <summary>
        /// Disconnects from the Minecraft server.
        /// </summary>
        public void Disconnect() {
            if (nh != null)
                nh.Stop();

            // -- Reset all variables to default so we can make a new connection.

            running = false;
            serverState = 0;
            nh = null;
            minecraftWorld = null;
            thisPlayer = null;
            players = null;

            raiseDebug(this, "Variables reset, disconnected from server.");
        }

        #region Event Messengers
        /// <summary>
        /// Raises a Minecraft Error from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Error Message</param>
        public void raiseError(object sender, string message) {
            if (errorMessage != null)
                errorMessage(sender, message);
        }
        /// <summary>
        /// Raises Minecraft Info from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Info Message</param>
        public void raiseInfo(object sender, string message) {
            if (infoMessage != null)
                infoMessage(sender, message);
        }
        /// <summary>
        /// Raises Minecraft debug message from another class
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="message">Debug message</param>
        public void raiseDebug(object sender, string message) {
            if (debugMessage != null)
                debugMessage(sender, message);
        }
        /// <summary>
        /// Raises a normal minecraft message (such as chat)
        /// </summary>
        /// <param name="sender">Sending class</param>
        /// <param name="Message">Minecraft message</param>
        public void raiseMC(object sender, string Message, string name) {
            if (message != null)
                message(sender, Message, name);
        }

        public void raiseLoginSuccess(object sender) {
            if (loginSuccess != null)
                loginSuccess(sender);
        }

        public void raiseLoginFailure(object sender, string reason) {
            if (loginFailure != null)
                loginFailure(sender, reason);
        }

        public void raiseGameJoined() {
            if (joinedGame != null)
                joinedGame();
        }

        public void raiseEntityAnimationChanged(object sender, int Entity_ID, byte Animation) {
            if (EntityAnimationChanged != null)
                
                EntityAnimationChanged(sender, Entity_ID, Animation);
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

        public void raiseBlockBreakingEvent(Classes.Vector Location, int Entity_ID, byte Stage) {
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
            if (transactionRejected != null)
                transactionRejected(Window_ID, Action_ID);
        }

        public void raiseTransactionAccepted(byte Window_ID, short Action_ID) {
            if (transactionAccepted != null)
                transactionAccepted(Window_ID, Action_ID);
        }

        public void raiseEntityDestruction(int Entity_ID) {
            if (entityDestroyed != null)
                entityDestroyed(Entity_ID);
        }

        public void raiseKicked(string reason) {
            if (playerKicked != null)
                playerKicked(reason);
        }

        public void raiseScoreBoard(byte position, string name) {
            if (displayScoreboard != null)
                displayScoreboard(position, name);
        }

        public void raiseEntityStatus(int Entity_ID) {
            if (entityStatusChanged != null)
                entityStatusChanged(Entity_ID);
        }

        public void raiseEntityEquipment(int Entity_ID, int slot, Classes.Item newItem) {
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
            if (playerListitemAdd != null)
                playerListitemAdd(name, ping);
        }

        public void raisePlayerlistRemove(string name) {
            if (playerListitemRemove != null)
                playerListitemRemove(name);
        }

        public void raisePlayerlistUpdate(string name, short ping) {
            if (playerListitemUpdate != null)
                playerListitemUpdate(name, ping);
        }

        public void raiseLocationChanged() {
            if (locationChanged != null)
                locationChanged();
        }

        public void raisePluginMessage(string channel, byte[] data) {
            if (pluginMessage != null)
                pluginMessage(channel, data);
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

        public void raiseSetWindowSlot(byte windowid, short slot, Classes.Item item) {
            if (setWindowItem != null)
                setWindowItem(windowid, slot, item);
        }

        public void raiseInventoryItem(short slot, Classes.Item item) {
            if (setInventoryItem != null)
                setInventoryItem(slot, item);
        }

        public void raisePlayerHealthUpdate(float health, short hunger, float saturation) {
            if (setPlayerHealth != null)
                setPlayerHealth(health, hunger, saturation);
        }
        #endregion
        #region Event Handlers
        void networkInfo(object Sender, string Message) {
            if (infoMessage != null)
                infoMessage(Sender, "(NETWORK): " + Message);
        }
        void networkDebug(object Sender, string Message) {
            if (debugMessage != null)
                debugMessage(Sender, "(NETWORK): " + Message);
        }
        void networkError(object Sender, string Message) {
            if (errorMessage != null)
                errorMessage(Sender, "(NETWORK): " + Message);
        }
        void raisePacketHandled(object Sender, object Packet, int id) {
            if (packetHandled != null)
                packetHandled(Sender, Packet, id);
        }

        #endregion
        #region Events
        
        #region Base Events
        public delegate void debugMessageHandler(object sender, string message);
        public event debugMessageHandler debugMessage;

        public delegate void errorMessageHandler(object sender, string message);
        public event errorMessageHandler errorMessage;

        public delegate void infoMessageHandler(object sender, string message);
        public event infoMessageHandler infoMessage;

        public delegate void messageHandler(object sender, string message, string name);
        public event messageHandler message;

        public delegate void packetHandler(object sender, object packet, int id);
        public event packetHandler packetHandled;
        #endregion

        #region Block Events
        public delegate void blockChangedEventHandler(int x, byte y, int z, int newType, byte data);
        public event blockChangedEventHandler blockChanged;

        public delegate void blockBreakAnimationHandler(Classes.Vector Location, int Entity_ID, byte Stage);
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

        public delegate void entityEquipmentChangedHandler(int Entity_ID, int slot, Classes.Item newItem);
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

        public delegate void setWindowItemHandler(byte window_ID, short slot, Classes.Item item);
        public event setWindowItemHandler setWindowItem;

        public delegate void setInventoryItemHandler(short slot, Classes.Item item);
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
        // -- Server Events (Connected, Disconnected, Kicked)
        #region Server Events
        public delegate void pluginMessageHandler(string channel, byte[] data);
        public event pluginMessageHandler pluginMessage;

        public delegate void playerListitemAddHandler(string name, short ping);
        public event playerListitemAddHandler playerListitemAdd;

        public delegate void playerListitemRemoveHandler(string name);
        public event playerListitemRemoveHandler playerListitemRemove;

        public delegate void playerListitemUpdateHandler(string name, short ping);
        public event playerListitemUpdateHandler playerListitemUpdate;

        public delegate void loginSuccessHandler(object sender);
        public event loginSuccessHandler loginSuccess;

        public delegate void loginFailureHandler(object sender, string reason);
        public event loginFailureHandler loginFailure;

        public delegate void joinGameHandler();
        public event joinGameHandler joinedGame;

        public delegate void transactionRejectedHandler(byte Window_ID, short Action_ID);
        public event transactionRejectedHandler transactionRejected;

        public delegate void transactionAcceptedHandler(byte Window_ID, short Action_ID);
        public event transactionAcceptedHandler transactionAccepted;

        public delegate void playerKickedHandler(string reason);
        public event playerKickedHandler playerKicked;
        #endregion
        #endregion
    }
}
