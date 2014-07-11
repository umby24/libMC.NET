using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using libMC.NET.Common;
using libMC.NET.Entities;
using libMC.NET.Network;
using libMC.NET.World;

using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using SMProxy;

namespace libMC.NET.Client {
    public struct ChatObject {
        public string translate, color, text, unknown;
        public bool italic, bold, strikethrough, obfs;
        public JArray with;
        public List<string> Names;
    }

    class PacketEventHandler {
        public PacketEventHandler(NetworkHandler nh) {
            // -- Login packets
            nh.RegisterLoginHandler(0, new NetworkHandler.PacketHandler(HandleLoginDisconnect));
            nh.RegisterLoginHandler(1, new NetworkHandler.PacketHandler(HandleEncryptionRequest));
            nh.RegisterLoginHandler(2, new NetworkHandler.PacketHandler(HandleLoginSuccess));

            // -- Status Packets
            nh.RegisterStatusHandler(0, new NetworkHandler.PacketHandler(HandleStatusResponse));
            nh.RegisterStatusHandler(1, new NetworkHandler.PacketHandler(HandleStatusPing));

            // -- Play packets
            nh.RegisterPlayHandler(0, new NetworkHandler.PacketHandler(HandleKeepAlive));
            nh.RegisterPlayHandler(0x01, new NetworkHandler.PacketHandler(HandleJoinGame));
            nh.RegisterPlayHandler(0x02, new NetworkHandler.PacketHandler(HandleChat));
            nh.RegisterPlayHandler(0x04, new NetworkHandler.PacketHandler(HandleEntityEquipment));
            nh.RegisterPlayHandler(0x07, new NetworkHandler.PacketHandler(HandleRespawn));
            nh.RegisterPlayHandler(0x09, new NetworkHandler.PacketHandler(HandleHeldItemChange));
            nh.RegisterPlayHandler(0x0B, new NetworkHandler.PacketHandler(HandleAnimation));
            nh.RegisterPlayHandler(0x0D, new NetworkHandler.PacketHandler(HandleCollectItem));
            nh.RegisterPlayHandler(0x12, new NetworkHandler.PacketHandler(HandleEntityVelocity));
            nh.RegisterPlayHandler(0x13, new NetworkHandler.PacketHandler(HandleDestroyEntities));
            nh.RegisterPlayHandler(0x15, new NetworkHandler.PacketHandler(HandleEntityRelMove));
            nh.RegisterPlayHandler(0x16, new NetworkHandler.PacketHandler(HandleEntityLook));
            nh.RegisterPlayHandler(0x17, new NetworkHandler.PacketHandler(HandleLookEntityRelMove));
            nh.RegisterPlayHandler(0x18, new NetworkHandler.PacketHandler(HandleEntityTeleport));
            nh.RegisterPlayHandler(0x19, new NetworkHandler.PacketHandler(HandleEntityHeadLook));
            nh.RegisterPlayHandler(0x1A, new NetworkHandler.PacketHandler(HandleEntityStatus));
            nh.RegisterPlayHandler(0x1B, new NetworkHandler.PacketHandler(AttachEntity));
            nh.RegisterPlayHandler(0x1D, new NetworkHandler.PacketHandler(HandleEntityEffect));
            nh.RegisterPlayHandler(0x21, new NetworkHandler.PacketHandler(HandleChunkData));
            nh.RegisterPlayHandler(0x23, new NetworkHandler.PacketHandler(BlockChange));
            nh.RegisterPlayHandler(0x24, new NetworkHandler.PacketHandler(BlockAction));
            nh.RegisterPlayHandler(0x25, new NetworkHandler.PacketHandler(BlockBreakAnimation));
            nh.RegisterPlayHandler(0x26, new NetworkHandler.PacketHandler(HandleMapChunkBulk));
            nh.RegisterPlayHandler(0x27, new NetworkHandler.PacketHandler(HandleExplosion));
            nh.RegisterPlayHandler(0x28, new NetworkHandler.PacketHandler(HandleEffects));
            nh.RegisterPlayHandler(0x2B, new NetworkHandler.PacketHandler(ChangeGameState));
            nh.RegisterPlayHandler(0x2E, new NetworkHandler.PacketHandler(HandleCloseWindow));
            nh.RegisterPlayHandler(0x32, new NetworkHandler.PacketHandler(HandleConfirmTransaction));
            nh.RegisterPlayHandler(0x3D, new NetworkHandler.PacketHandler(HandleDisplayScoreboard));
            nh.RegisterPlayHandler(0x40, new NetworkHandler.PacketHandler(HandleDisconnect));
            
        }

        #region Login Packets
        public void HandleLoginDisconnect(MinecraftClient client, IPacket packet) {
            var Disconnect = (CBLoginDisconnect)packet;

            client.RaiseLoginFailure(this, Disconnect.JSONData);
            client.Disconnect();
        }

        public void HandleEncryptionRequest(MinecraftClient client, IPacket packet) {
            var ER = (CBEncryptionRequest)packet;
            var SharedKey = new byte[16];

            var Random = RandomNumberGenerator.Create(); // -- Generate a random shared key.
            Random.GetBytes(SharedKey);

            if (ER.ServerID == "" && client.VerifyNames) {
                // -- Verify with Minecraft.net.
                // -- At this point, the server requires a hash containing the server id,
                // -- shared key, and original public key. So we make this, and then pass to Minecraft.net

                List<byte> HashList = new List<byte>();
                HashList.AddRange(Encoding.ASCII.GetBytes(ER.ServerID));
                HashList.AddRange(SharedKey);
                HashList.AddRange(ER.PublicKey);

                var HashData = HashList.ToArray();
                var Hash = JavaHexDigest(HashData);

                var Verify = new Minecraft_Net_Interaction();

                if (!Verify.VerifyName(client.ClientName, client.AccessToken, client.SelectedProfile, Hash)) {
                    client.RaiseLoginFailure(this, "Failed to verify name with Minecraft session server.");
                    client.Disconnect();
                    return;
                }
            } else
                client.RaiseInfo(this, "Name verification disabled, skipping authentication.");

            // -- AsnKeyParser is a part of the cryptography.dll, which is simply a compiled version
            // -- of SMProxy's Cryptography.cs, with the server side parts stripped out.
            // -- You pass it the key data and ask it to parse, and it will 
            // -- Extract the server's public key, then parse that into RSA for us.

            var KeyParser = new AsnKeyParser(ER.PublicKey);
            var Dekey = KeyParser.ParseRSAPublicKey();

            // -- Now we create an encrypter, and encrypt the token sent to us by the server
            // -- as well as our newly made shared key (Which can then only be decrypted with the server's private key)
            // -- and we send it to the server.

            var cryptoService = new RSACryptoServiceProvider(); // -- RSA Encryption class
            cryptoService.ImportParameters(Dekey); // -- Import the Server's public key to use as the RSA encryption key.

            byte[] EncryptedSecret = cryptoService.Encrypt(SharedKey, false); // -- Encrypt the Secret key and verification token.
            byte[] EncryptedVerify = cryptoService.Encrypt(ER.VerifyToken, false);

            client.nh.wSock.InitEncryption(SharedKey); // -- Give the shared secret key to the socket

            var Response = new SBEncryptionResponse(); // -- Respond to the server

            Response.SharedLength = (short)EncryptedSecret.Length;
            Response.SharedSecret = EncryptedSecret;
            Response.VerifyLength = (short)EncryptedVerify.Length;
            Response.VerifyToken = EncryptedVerify;

            Response.Write(client.nh.wSock);

            client.nh.wSock.EncEnabled = true;
            client.nh.RaiseSocketInfo(this, "Encryption Enabled.");
        }

        #region Encryption Helping Functions
        private static string GetHexString(byte[] p) {
            string result = "";
            for (int i = 0; i < p.Length; i++) {
                if (p[i] < 0x10)
                    result += "0";
                result += p[i].ToString("x"); // Converts to hex string
            }
            return result;
        }

        private static byte[] TwosCompliment(byte[] p) // little endian
        {
            int i;
            bool carry = true;
            for (i = p.Length - 1; i >= 0; i--) {
                p[i] = unchecked((byte)~p[i]);
                if (carry) {
                    carry = p[i] == 0xFF;
                    p[i]++;
                }
            }
            return p;
        }

        public static string JavaHexDigest(byte[] data) {
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(data);
            bool negative = (hash[0] & 0x80) == 0x80;
            if (negative) // check for negative hashes
                hash = TwosCompliment(hash);
            // Create the string and trim away the zeroes
            string digest = GetHexString(hash).TrimStart('0');
            if (negative)
                digest = "-" + digest;
            return digest;
        }
        #endregion

        public void HandleLoginSuccess(MinecraftClient client, IPacket packet) {
            var Success = (CBLoginSuccess)packet;
            client.RaiseLoginSuccess(this);
            client.RaiseDebug(this, "UUID: " + Success.UUID + " Username: " + Success.Username);

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.playerName = Success.Username;
            client.ServerState = 3;
            client.RaiseDebug(this, "The server state is now 3 (Play)");
        }
        #endregion
        #region Status Packets
        public void HandleStatusResponse(MinecraftClient client, IPacket packet) {
            string versionName, MOTD; // -- Variables that are enclosed in json.
            int ProtocolVersion, MaxPlayers, OnlinePlayers;
            List<string> Players = null;
            Image favicon = null;

            var Response = (CBResponse)packet;
            var jsonObj = JToken.Parse(Response.JSONResponse);

            versionName = jsonObj["version"]["name"].Value<string>();
            ProtocolVersion = jsonObj["version"]["protocol"].Value<int>();

            MaxPlayers = jsonObj["players"]["max"].Value<int>(); ;
            OnlinePlayers = jsonObj["players"]["online"].Value<int>();

            var tempPlayers = jsonObj["players"]["sample"];

            if (tempPlayers != null) {
                Players = new List<string>();

                foreach (JObject b in tempPlayers) {
                    Players.Add(b.Last.First.ToString());
                }
            }

            MOTD = jsonObj["description"].Value<string>();
            string imageString = jsonObj["favicon"].Value<string>();

            if (imageString != null) {
                try {
                    var imageBytes = Convert.FromBase64String(imageString.Replace("data:image/png;base64,", ""));

                    var ms = new MemoryStream(imageBytes);
                    favicon = Image.FromStream(ms, false, true);
                    ms.Close();
                } catch {
                    favicon = null;
                }
            }

            client.RaisePingResponse(versionName, ProtocolVersion, MaxPlayers, OnlinePlayers, Players.ToArray(), MOTD, favicon);

            var Ping = new SBPing();
            Ping.Time = DateTime.UtcNow.Ticks;
            Ping.Write(client.nh.wSock);
        }
        public void HandleStatusPing(MinecraftClient client, IPacket packet) {
            var Ping = (CBPing)packet;
            client.RaisePingMs((int)(DateTime.UtcNow.Ticks - Ping.Time) / 10000); // -- 10,000 ticks per millisecond.
            client.nh.RaiseSocketDebug(this, "Server ping complete.");
        }
        #endregion
        #region Play Packets
        public void HandleAnimation(MinecraftClient client, IPacket packet) {
            var Animation = (CBAnimation)packet;

            if (client.ThisPlayer != null && Animation.EntityID == client.ThisPlayer.Entity_ID)
                client.ThisPlayer.Animation = Animation.Animation;

            if (client.MinecraftWorld != null) {
                var index = client.MinecraftWorld.GetEntityById(Animation.EntityID);
                if (index != -1)
                    client.MinecraftWorld.Entities[index].animation = (sbyte)Animation.Animation;
            }

            client.RaiseEntityAnimationChanged(this, Animation.EntityID, Animation.Animation);
        }

        public void AttachEntity(MinecraftClient client, IPacket packet) {
            var Attach = (CBAttachEntity)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(Attach.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].attached = true;
                    client.MinecraftWorld.Entities[eIndex].Vehicle_ID = Attach.VehicleID;
                    client.MinecraftWorld.Entities[eIndex].leashed = Attach.Leash;
                }
            }

            client.RaiseEntityAttached(Attach.EntityID, Attach.VehicleID, Attach.Leash);
        }

        public void BlockAction(MinecraftClient client, IPacket packet) {
            var BlockPacket = (CBBlockAction)packet;

            switch (BlockPacket.BlockType) {
                case 25: // -- Note block
                    client.RaiseNoteBlockSound(BlockPacket.Byte1, BlockPacket.Byte2, BlockPacket.X, BlockPacket.Y, BlockPacket.Z);
                    break;
                case 29: // -- Sticky Piston
                    client.RaisePistonMoved(BlockPacket.Byte1, BlockPacket.Byte2, BlockPacket.X, BlockPacket.Y, BlockPacket.Z);
                    break;
                case 33: // -- Piston
                    client.RaisePistonMoved(BlockPacket.Byte1, BlockPacket.Byte2, BlockPacket.X, BlockPacket.Y, BlockPacket.Z);
                    break;
                case 54: // -- Chest
                    client.RaiseChestStateChange(BlockPacket.Byte2, BlockPacket.X, BlockPacket.Y, BlockPacket.Z);
                    break;
                case 146: // -- Trapped chest
                    client.RaiseChestStateChange(BlockPacket.Byte2, BlockPacket.X, BlockPacket.Y, BlockPacket.Z);
                    break;
                default:
                    client.RaiseError(this, "Unknown block action received: " + BlockPacket.BlockType.ToString());
                    break;
            }
        }

        public void BlockBreakAnimation(MinecraftClient client, IPacket packet) {
            var BlockPacket = (CBBlockBreakAnimation)packet;
            client.RaiseBlockBreakingEvent(new Vector(BlockPacket.X, BlockPacket.Y, BlockPacket.Z), BlockPacket.EntityID, BlockPacket.Stage);
        }

        public void BlockChange(MinecraftClient client, IPacket packet) {
            var BlockPacket = (CBBlockChange)packet;

            var ChunkX = decimal.Divide(BlockPacket.X, 16);
            var ChunkZ = decimal.Divide(BlockPacket.Z, 16);

            ChunkX = Math.Floor(ChunkX);
            ChunkZ = Math.Floor(ChunkZ);

            int myIndex = client.MinecraftWorld.GetChunk(int.Parse(ChunkX.ToString()), int.Parse(ChunkZ.ToString()));

            if (myIndex == -1)
                return;

            var myChunk = client.MinecraftWorld.worldChunks[myIndex];
            myChunk.UpdateBlock(BlockPacket.X, BlockPacket.Y, BlockPacket.Z, BlockPacket.BlockID);
            myChunk.SetBlockData(BlockPacket.X, BlockPacket.Y, BlockPacket.Z, BlockPacket.BlockMetadata);

            client.RaiseBlockChangedEvent(BlockPacket.X, BlockPacket.Y, BlockPacket.Z, BlockPacket.BlockID, BlockPacket.BlockMetadata);
        }

        public void ChangeGameState(MinecraftClient client, IPacket packet) {
            var GamePacket = (CBChangeGameState)packet;
            string eventName = "";

            switch (GamePacket.Reason) {
                case 0:
                    eventName = "Invalid bed";
                    break;
                case 1:
                    eventName = "Rain Start";
                    break;
                case 2:
                    eventName = "Rain End";
                    break;
                case 3:
                    eventName = "Game Mode";
                    break;
                case 4:
                    eventName = "Credits";
                    break;
                case 5:
                    eventName = "Demo";
                    break;
                case 6:
                    eventName = "Bow Hit";
                    break;
                case 7:
                    eventName = "Fade value";
                    break;
                case 8:
                    eventName = "Fade time";
                    break;
            }

            client.RaiseGameStateChanged(eventName, GamePacket.Value);
        }

        public void HandleChat(MinecraftClient client, IPacket packet) {
            var Chat = (CBChatMessage)packet;

            //string parsedMessage = ParseJsonChat(Chat.JSONData);//ParseJsonChat(Chat.JSONData, ref sender);

            client.RaiseMC(this, Chat.JSONData, Chat.JSONData);
        }
        #region Chat Message Helping Functions
        ChatObject ParseElement(JObject jsonObj) {
            var chat = new ChatObject();

            foreach (JProperty prop in jsonObj.Properties()) {
                switch (prop.Name) {
                    case "translate":
                        chat.translate = (string)prop.Value;
                        break;
                    case "italic":
                        chat.italic = (bool)prop.Value;
                        break;
                    case "color":
                        chat.color = (string)prop.Value;
                        break;
                    case "text":
                        chat.text = (string)prop.Value;
                        break;
                    case "with":
                        chat.with = (JArray)prop.Value;
                        break;
                    default:
                        chat.unknown = prop.Name;
                        break;
                }
            }

            return chat;
        }

        ChatObject ParseSecondaryElement(JObject jsonObj, ChatObject chatObj) {
            foreach (JProperty prop in jsonObj.Properties()) {
                switch (prop.Name) {
                    case "translate":
                        chatObj.translate = (string)((JValue)prop.Value);
                        break;
                    case "with":
                        chatObj.Names.Add((string)((JValue)((JArray)prop.Value)[0]).Value);
                        break;
                    case "text":
                        chatObj.Names.Add((string)((JValue)prop.Value));
                        break;
                    case "extra":
                        foreach (JValue b in (JArray)prop.Value) {
                            if (b.Type == JTokenType.String)
                                chatObj.text += (string)b.Value + " ";
                        }
                        break;
                }
            }

            return chatObj;
        }

        string ParseModifiers(ChatObject MainObj, string Text) {
            if (MainObj.italic)
                Text += "§o";

            if (MainObj.bold)
                Text += "§l";

            if (MainObj.strikethrough)
                Text += "§m";

            if (MainObj.obfs)
                Text += "§k";

            if (MainObj.color != null)
                Text += Color_To_Code(MainObj.color);

            return Text;
        }

        ChatObject ParseWith(JArray with, ChatObject chatObj) {
            for (int x = 0; x < with.Count; x++) {
                switch (with[x].Type) {
                    case JTokenType.String: // -- Add a name
                        if (chatObj.translate == "chat.type.text")
                            chatObj.text += (string)((JValue)with[x]).Value;
                        else
                            chatObj.Names.Add((string)((JValue)with[x]).Value);
                            
                        break;
                    case JTokenType.Object:
                        var myObj = (JObject)with[x];
                        chatObj = ParseSecondaryElement(myObj, chatObj);
                        break;
                }
            }

            return chatObj;
        }

        string ParseBaseMessage(string Type) {
            switch (Type) {
                case "death.attack.player":
                    return "{0} was killed by {1}";
                case "multiplayer.player.joined":
                    return "{0} joined the game";
                case "multiplayer.player.left":
                    return "{0} left the game";
                case "commands.op.success":
                    return "{1} was made an op by {0}";
                case "chat.type.text":
                    return "{0}: ";
                case "chat.type.announcement":
                    return "{0}: ";
                default:
                    return "";
            }
        }

        ChatObject DoExtra(ChatObject chatObj, JObject myObj) {
            foreach (JProperty prop in myObj.Children()) {
                switch (prop.Name) {
                    case "color":
                        chatObj.text += Color_To_Code((string)prop.Value);
                        break;
                    case "text":
                        chatObj.text += (string)prop.Value;
                        break;
                }
            }

            return chatObj;
        }

        string ParseJsonChat(string raw) {
            string Final = "";
            var MainObj = ParseElement(JObject.Parse(raw));
            MainObj.Names = new List<string>();

            Final = ParseModifiers(MainObj, Final);
            MainObj = ParseWith(MainObj.with, MainObj);

            if (JObject.Parse(raw)["extra"] != null) {
                foreach (JObject c in (JArray)JObject.Parse(raw)["extra"])
                    MainObj = DoExtra(MainObj, c);
            }

            MainObj.Names.RemoveAll(string.IsNullOrWhiteSpace);
            Final += string.Format(ParseBaseMessage(MainObj.translate), MainObj.Names.ToArray());
            Final += MainObj.text;

            return Final;
        }

        public string Color_To_Code(string Color) {
            string code = "";

            switch (Color) {
                case "black":
                    code = "§0";
                    break;
                case "darkblue":
                    code = "§1";
                    break;
                case "darkgreen":
                    code = "§2";
                    break;
                case "darkcyan":
                    code = "§3";
                    break;
                case "darkred":
                    code = "§4";
                    break;
                case "purple":
                    code = "§5";
                    break;
                case "orange":
                    code = "§6";
                    break;
                case "gray":
                    code = "§7";
                    break;
                case "darkgray":
                    code = "§8";
                    break;
                case "blue":
                    code = "§9";
                    break;
                case "brightgreen":
                    code = "§A";
                    break;
                case "cyan":
                    code = "§B";
                    break;
                case "red":
                    code = "§C";
                    break;
                case "pink":
                    code = "§D";
                    break;
                case "yellow":
                    code = "§E";
                    break;
                case "white":
                    code = "§F";
                    break;
            }

            return code;
        }
        #endregion
        public void HandleChunkData(MinecraftClient client, IPacket packet) {
            var ChunkData = (CBChunkData)packet;

            byte[] trim = new byte[ChunkData.Compressedsize - 2];
            byte[] decompressedData;

            if (ChunkData.Primarybitmap == 0) {
                // -- Unload chunk.
                int cIndex = -1;

                if (client.MinecraftWorld != null)
                    cIndex = client.MinecraftWorld.GetChunk(ChunkData.ChunkX, ChunkData.ChunkZ);

                if (cIndex != -1)
                    client.MinecraftWorld.worldChunks.RemoveAt(cIndex);

                client.RaiseChunkUnload(ChunkData.ChunkX, ChunkData.ChunkZ);
                return;
            }

            // -- Remove GZip Header
            Buffer.BlockCopy(ChunkData.Compresseddata, 2, trim, 0, trim.Length);

            // -- Decompress the data
            decompressedData = Decompressor.Decompress(trim);

            // -- Create new chunk
            Chunk newChunk = new Chunk(ChunkData.ChunkX, ChunkData.ChunkZ, (short)ChunkData.Primarybitmap, (short)ChunkData.Addbitmap, true, ChunkData.GroundUpcontinuous); // -- Skylight assumed true
            newChunk.GetData(decompressedData);

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            // -- Add the chunk to the world
            client.MinecraftWorld.worldChunks.Add(newChunk);

            client.RaiseChunkLoad(ChunkData.ChunkX, ChunkData.ChunkZ);
        }
        public void HandleCloseWindow(MinecraftClient client, IPacket packet) {
            var myWindow = (CBCloseWindow)packet;
            client.RaiseWindowClosed(myWindow.WindowID);
        }
        public void HandleCollectItem(MinecraftClient client, IPacket packet) {
            var myCollection = (CBCollectItem)packet;
            client.RaiseItemCollected(myCollection.CollectedEntityID, myCollection.CollectorEntityID);
        }
        public void HandleConfirmTransaction(MinecraftClient client, IPacket packet) {
            var myPacket = (CBConfirmTransaction)packet;

            if (myPacket.Accepted)
                client.RaiseTransactionAccepted(myPacket.WindowID, myPacket.Actionnumber);
            else
                client.RaiseTransactionRejected(myPacket.WindowID, myPacket.Actionnumber);
        }
        public void HandleDestroyEntities(MinecraftClient client, IPacket packet) {
            var myPacket = (CBDestroyEntities)packet;

            if (client.MinecraftWorld == null)
                return;

            for (int x = 0; x < myPacket.Count; x++) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityIDs[x]);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities.RemoveAt(eIndex);

                client.RaiseEntityDestruction(myPacket.EntityIDs[x]);
            }
        }
        public void HandleDisconnect(MinecraftClient client, IPacket packet) {
            var Disconnect = (CBDisconnect)packet;

            client.RaiseInfo(this, "You were kicked! Reason: " + Disconnect.Reason);
            client.RaiseKicked(Disconnect.Reason);
            client.Disconnect();
        }
        public void HandleDisplayScoreboard(MinecraftClient client, IPacket packet) {
            var myPacket = (CBDisplayScoreboard)packet;
            client.RaiseScoreBoard(myPacket.Position, myPacket.ScoreName);
        }
        public void HandleEffects(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEffect)packet;
            //TODO: Implement this. Pull requests welcome and are encouraged for parsing the IDs and raising an event for this.
        }
        public void HandleEntityEffect(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityEffect)packet;

            if (client.MinecraftWorld == null)
                return;

            int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

            if (eIndex != -1) {
                client.MinecraftWorld.Entities[eIndex].amplifier = myPacket.Amplifier;
                client.MinecraftWorld.Entities[eIndex].duration = myPacket.Duration;
                client.MinecraftWorld.Entities[eIndex].status = myPacket.EffectID;
                client.RaiseEntityStatus(myPacket.EntityID);
            }
        }
        public void HandleEntityEquipment(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityEquipment)packet;

            if (client.ThisPlayer != null && myPacket.EntityID == client.ThisPlayer.Entity_ID) { 
                client.ThisPlayer.SetInventory(Item.ItemFromSlot(myPacket.Item), myPacket.Slot);
                return;
            }

            if (client.MinecraftWorld != null && client.MinecraftWorld.Entities != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].HandleInventory(myPacket.Slot, Item.ItemFromSlot(myPacket.Item));
            }
        }
        public void HandleEntityHeadLook(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityHeadLook)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].headPitch = myPacket.HeadYaw;
            }

            client.RaiseEntityHeadLookChanged(myPacket.EntityID, myPacket.HeadYaw);
        }

        public void HandleEntityLook(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityLook)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].pitch = myPacket.Pitch;
                    client.MinecraftWorld.Entities[eIndex].yaw = myPacket.Yaw;
                }
            }

            client.RaiseEntityLookChanged(myPacket.EntityID, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleLookEntityRelMove(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityLookandRelativeMove)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.x += (myPacket.DX * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.y += (myPacket.DY * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.z += (myPacket.DZ * 32);
                    client.MinecraftWorld.Entities[eIndex].yaw = myPacket.Yaw;
                    client.MinecraftWorld.Entities[eIndex].pitch = myPacket.Pitch;
                }
            }

            client.RaiseEntityRelMove(myPacket.EntityID, myPacket.DX * 32, myPacket.DY * 32, myPacket.DZ * 32);
            client.RaiseEntityLookChanged(myPacket.EntityID, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleEntityMetadata(MinecraftClient client, IPacket packet) {
            //TODO:
            // -- This needs to be written
        }
        public void HandleEntityProperties(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityProperties)packet;

            //TODO: This
        }
        public void HandleEntityRelMove(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityRelativeMove)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.x += (myPacket.DX * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.y += (myPacket.DY * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.z += (myPacket.DZ * 32);
                }
            }

            client.RaiseEntityRelMove(myPacket.EntityID, myPacket.DX, myPacket.DY, myPacket.DZ);
        }

        public void HandleEntityStatus(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityStatus)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].status = myPacket.EntityStatus;
            }

            client.RaiseEntityStatus(myPacket.EntityID);
        }

        public void HandleEntityTeleport(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityTeleport)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.x = myPacket.X;
                    client.MinecraftWorld.Entities[eIndex].Location.y = myPacket.Y;
                    client.MinecraftWorld.Entities[eIndex].Location.z = myPacket.Z;
                    client.MinecraftWorld.Entities[eIndex].yaw = myPacket.Yaw;
                    client.MinecraftWorld.Entities[eIndex].pitch = myPacket.Pitch;
                }
            }

            client.RaiseEntityTeleport(myPacket.EntityID, myPacket.X, myPacket.Y, myPacket.Z);
            client.RaiseEntityLookChanged(myPacket.EntityID, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleEntityVelocity(MinecraftClient client, IPacket packet) {
            var myPacket = (CBEntityVelocity)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Velocity_X = myPacket.VelocityX;
                    client.MinecraftWorld.Entities[eIndex].Velocity_Y = myPacket.VelocityY;
                    client.MinecraftWorld.Entities[eIndex].Velocity_Z = myPacket.VelocityZ;
                }
            }

            client.RaiseEntityVelocityChanged(myPacket.EntityID, myPacket.VelocityX, myPacket.VelocityY, myPacket.VelocityZ);
        }

        public void HandleExplosion(MinecraftClient client, IPacket packet) {
            var myPacket = (CBExplosion)packet;
            //TODO: Handle more of this...
            client.RaiseExplosion(myPacket.X, myPacket.Y, myPacket.Z);
        }

        public void HandleHeldItemChange(MinecraftClient client, IPacket packet) {
            var myPacket = (CBHeldItemChange)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.selectedSlot = myPacket.Slot;

            var mySend = new SBHeldItemChange();
            mySend.Slot = client.ThisPlayer.selectedSlot;
            mySend.Write(client.nh.wSock);
        }

        public void HandleJoinGame(MinecraftClient client, IPacket packet) {
            var myPacket = (CBJoinGame)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.Entity_ID = myPacket.EntityID;
            client.ThisPlayer.gameMode = myPacket.Gamemode;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.difficulty = myPacket.Difficulty;
            client.MinecraftWorld.dimension = myPacket.Dimension;
            client.MinecraftWorld.maxPlayers = myPacket.MaxPlayers;
            client.MinecraftWorld.levelType = myPacket.LevelType;

            client.RaiseDebug(this, string.Format("Entity ID: {0}", myPacket.EntityID));
            client.RaiseGameJoined();

            var b = new SBClientSettings();
            b.Locale = "en_US";
            b.Viewdistance = 5;
            b.Chatflags = 3;
            b.Chatcolours = true;
            b.Difficulty = 1;
            b.ShowCape = false;
            b.Write(client.nh.wSock);

            var c = new SBPluginMessage();
            c.Channel = "MC|Brand";
            c.Data = Encoding.UTF8.GetBytes(client.ClientBrand);
            c.Length = (short)c.Data.Length;
            c.Write(client.nh.wSock);
        }

        public void HandleKeepAlive(MinecraftClient client, IPacket packet) {
            var KA = (CBKeepAlive)packet;

            var KAS = new SBKeepAlive();
            KAS.KeepAliveID = KA.KeepAliveID;
            KAS.Write(client.nh.wSock);
        }
        public void HandleMapChunkBulk(MinecraftClient client, IPacket packet) {
            var ChunkPacket = (CBMapChunkBulk)packet;
            int Offset = 0;

            byte[] trim = new byte[ChunkPacket.Datalength - 2];
            byte[] DecompressedData;

            Chunk[] chunks = new Chunk[ChunkPacket.Chunkcolumncount];

            Buffer.BlockCopy(ChunkPacket.Data, 2, trim, 0, trim.Length);

            DecompressedData = Decompressor.Decompress(trim);

            for (int i = 0; ChunkPacket.Chunkcolumncount > i; i++) {
                int x = BitConverter.ToInt32(ChunkPacket.Metainformation, Offset);
                int z = BitConverter.ToInt32(ChunkPacket.Metainformation, Offset + 4);
                short pbitmap = ReverseBytes(BitConverter.ToInt16(ChunkPacket.Metainformation, Offset + 8));
                short abitmap = ReverseBytes(BitConverter.ToInt16(ChunkPacket.Metainformation, Offset + 10));
                Offset += 12;

                chunks[i] = new Chunk(x, z, pbitmap, abitmap, ChunkPacket.Skylightsent, true); // -- Assume true for Ground Up Continuous

                DecompressedData = chunks[i].GetData(DecompressedData); // -- Calls the chunk class to take all of the bytes it needs, and return whats left.

                if (client.MinecraftWorld == null)
                    client.MinecraftWorld = new WorldClass();

                client.MinecraftWorld.worldChunks.Add(chunks[i]);
            }
        }
        #region MapChunkBulk Helping Methods
        public static short ReverseBytes(short value) {
            return (short)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        #endregion
        public void HandleMaps(MinecraftClient client, IPacket packet) {
            var myPacket = (CBMaps)packet;
            // -- Still don't know what this is for.
        }
        public void HandleMultiBlockChange(MinecraftClient client, IPacket packet) {
            var myPacket = (CBMultiBlockChange)packet;
            int chunkID = client.MinecraftWorld.GetChunk(myPacket.ChunkX, myPacket.ChunkZ);

            if (chunkID == -1) {
                client.RaiseError(this, "Attempted to access uninitialized chunk");
                return;
            }

            var thisChunk = client.MinecraftWorld.worldChunks[chunkID];

            for (int i = 0; i < myPacket.Recordcount - 1; i++) {
                var thisRecord = myPacket.Records[i];

                thisChunk.UpdateBlock(thisRecord.X, thisRecord.Y, thisRecord.Z, thisRecord.BlockID);
                thisChunk.SetBlockData(thisRecord.X, thisRecord.Y, thisRecord.Z, thisRecord.Metadata);
            }

            client.RaiseMultiBlockChange(myPacket.ChunkX, myPacket.ChunkX);
        }
        public void HandleOpenWindow(MinecraftClient client, IPacket packet) {
            var myPacket = (CBOpenWindow)packet;
            client.RaiseOpenWindow(myPacket.Windowid, myPacket.InventoryType, myPacket.Windowtitle, myPacket.NumberofSlots, myPacket.Useprovidedwindowtitle);
            client.RaiseDebug(this, "Window opened forcibly");
        }
        //TODO: Particle
        public void HandlePlayerAbilities(MinecraftClient client, IPacket packet) {
            var myPacket = (CBPlayerAbilities)packet;
            client.ThisPlayer.flyingSpeed = myPacket.Flyingspeed;
            client.ThisPlayer.WalkingSpeed = myPacket.Walkingspeed;
        }
        public void HandlePlayerListItem(MinecraftClient client, IPacket packet) {
            var myPacket = (CBPlayerListItem)packet;

            if (myPacket.Online) {
                if (client.Players.ContainsKey(myPacket.Playername)) {
                    client.Players[myPacket.Playername] = myPacket.Ping;
                    client.RaisePlayerlistUpdate(myPacket.Playername, myPacket.Ping);
                } else {
                    client.Players.Add(myPacket.Playername, myPacket.Ping);
                    client.RaisePlayerlistAdd(myPacket.Playername, myPacket.Ping);
                }
            } else {
                if (client.Players.ContainsKey(myPacket.Playername)) {
                    client.Players.Remove(myPacket.Playername);
                    client.RaisePlayerlistRemove(myPacket.Playername);
                }
            }
        }
        public void HandlePlayerPositionAndLook(MinecraftClient client, IPacket packet) {
            var myPacket = (CBPlayerPositionAndLook)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.location.x = myPacket.X;
            client.ThisPlayer.location.y = myPacket.Y;
            client.ThisPlayer.location.z = myPacket.Z;
            client.ThisPlayer.look[0] = myPacket.Yaw;
            client.ThisPlayer.look[1] = myPacket.Pitch;
            client.ThisPlayer.onGround = myPacket.OnGround;

            client.RaiseLocationChanged();

            var sending = new SBPlayerPositionAndLook();
            sending.X = client.ThisPlayer.location.x;
            sending.FeetY = client.ThisPlayer.location.y - 1.620;
            sending.HeadY = client.ThisPlayer.location.y;
            sending.Z = client.ThisPlayer.location.z;
            sending.Yaw = client.ThisPlayer.look[0];
            sending.Pitch = client.ThisPlayer.look[1];
            sending.OnGround = client.ThisPlayer.onGround;
            sending.Write(client.nh.wSock);

        }
        public void HandlePluginMessage(MinecraftClient client, IPacket packet) {
            var myPacket = (CBPluginMessage)packet;
            client.RaisePluginMessage(myPacket.Channel, myPacket.Data);
        }
        public void HandleRemoveEntityEffect(MinecraftClient client, IPacket packet) {
            var myPacket = (CBRemoveEntityEffect)packet;

            if (client.MinecraftWorld != null) {
                int eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityID);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].status = myPacket.EffectID;
                    client.RaiseEntityStatus(myPacket.EntityID);
                }
            }
        }
        public void HandleRespawn(MinecraftClient client, IPacket packet) {
            var Respawn = (CBRespawn)packet;

            client.MinecraftWorld = new WorldClass();
            client.MinecraftWorld.dimension = (sbyte)Respawn.Dimension;
            client.MinecraftWorld.difficulty = Respawn.Difficulty;
            client.MinecraftWorld.levelType = Respawn.LevelType;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.gameMode = Respawn.Gamemode;

            client.RaisePlayerRespawn();
        }
        public void HandleScoreboardObjectvice(MinecraftClient client, IPacket packet) {
            var myPacket = (CBScoreboardObjective)packet;

            if (myPacket.Create == 0)
                client.RaiseScoreboardAdd(myPacket.Objectivename, myPacket.Objectivevalue);
            else if (myPacket.Create == 1)
                client.RaiseScoreboardUpdate(myPacket.Objectivename, myPacket.Objectivevalue);
            else if (myPacket.Create == 2)
                client.RaiseScoreboardRemove(myPacket.Objectivename);
        }
        public void HandleSetExperience(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSetExperience)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.ExpBar = myPacket.Experiencebar;
            client.ThisPlayer.level = myPacket.Level;
            client.ThisPlayer.totalExp = myPacket.TotalExperience;

            client.RaiseExperienceUpdate(myPacket.Experiencebar, myPacket.Level, myPacket.TotalExperience);
        }
        public void HandleSetSlot(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSetSlot)packet;
            var myItem = Item.ItemFromSlot(myPacket.Slotdata);

            if (myPacket.WindowID == 0) {
                client.ThisPlayer.SetInventory(myItem, myPacket.Slot);
                client.RaiseInventoryItem(myPacket.Slot, myItem);
            } else
                client.RaiseSetWindowSlot(myPacket.WindowID, myPacket.Slot, myItem);
        }
        //TODO: Sign Editor, Sound Effect, SpawnExpOrb
        public void HandleGlobalEntitySpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnGlobalEntity)packet;
            client.RaiseDebug(this, "A thunderbolt struck at " + myPacket.X + " " + myPacket.Y + " " + myPacket.Z);
        }
        public void HandleMobSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnMob)packet;
            var newMob = new Entity("Mob");

            newMob.Entity_ID = myPacket.EntityID;
            newMob.mobType = myPacket.Type;
            newMob.Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z);

            newMob.pitch = myPacket.Pitch;
            newMob.headPitch = myPacket.HeadPitch;
            newMob.yaw = myPacket.Yaw;

            newMob.Velocity_X = myPacket.VelocityX;
            newMob.Velocity_Y = myPacket.VelocityY;
            newMob.Velocity_Z = myPacket.VelocityZ;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newMob);
        }
        public void HandleObjectSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnObject)packet;
            var newObj = new ObjectEntity(myPacket.Type);

            newObj.GetFriendlyName(myPacket.Data.ObjectID);
            newObj.Speed_X = myPacket.Data.SpeedX;
            newObj.Speed_Y = myPacket.Data.SpeedY;
            newObj.Speed_Z = myPacket.Data.SpeedZ;

            newObj.ObjectID = myPacket.EntityID;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.worldObjects == null)
                client.MinecraftWorld.worldObjects = new List<ObjectEntity>();

            client.MinecraftWorld.worldObjects.Add(newObj);
        }
        public void HandlePaintingSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnPainting)packet;
            var newEntity = new Entity("Painting");

            newEntity.Entity_ID = myPacket.EntityID;
            newEntity.playerName = myPacket.Title;
            newEntity.Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z);
            newEntity.direction = myPacket.Direction;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newEntity);
        }
        public void HandlePlayerSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnPlayer)packet;
            var newPlayer = new Entity("Player");

            newPlayer.Entity_ID = myPacket.EntityID;
            newPlayer.UUID = myPacket.PlayerUUID;
            newPlayer.playerName = myPacket.PlayerName;
            newPlayer.Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z);
            newPlayer.yaw = myPacket.Yaw;
            newPlayer.pitch = myPacket.Pitch;
            newPlayer.heldItem = myPacket.CurrentItem;

            //TODO: Metadata..

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newPlayer);
        }
        public void HandleSpawnPosition(MinecraftClient client, IPacket packet) {
            var myPacket = (CBSpawnPosition)packet;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.Spawn_X = myPacket.X;
            client.MinecraftWorld.Spawn_Y = myPacket.Y;
            client.MinecraftWorld.Spawn_Z = myPacket.Z;
        }
        public void HandleStatistics(MinecraftClient client, IPacket packet) {
            //TODO: Make events for this.
        }
        public void HandleTabComplete(MinecraftClient client, IPacket packet) {
            var myPacket = (CBTabComplete)packet;

            client.RaiseTabComplete(myPacket.Matches);
        }
        public void HandleTimeUpdate(MinecraftClient client, IPacket packet) {
            var myPacket = (CBTimeUpdate)packet;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.worldAge = myPacket.Ageoftheworld;
            client.MinecraftWorld.currentTime = myPacket.Timeofday;

            var playerPacket = new SBPlayer();
            playerPacket.OnGround = client.ThisPlayer.onGround;
            playerPacket.Write(client.nh.wSock);

            if (client.nh.worldTick == null)
                client.nh.worldTick = new MinecraftWorld.TickHandler(ref client);

        }
        public void HandleUpdateHealth(MinecraftClient client, IPacket packet) {
            var myPacket = (CBUpdateHealth)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.playerHealth = myPacket.Health;
            client.ThisPlayer.playerHunger = myPacket.Food;
            client.ThisPlayer.foodSaturation = myPacket.FoodSaturation;

            client.RaisePlayerHealthUpdate(myPacket.Health, myPacket.Food, myPacket.FoodSaturation);
        }
        public void HandleUseBed(MinecraftClient client, IPacket packet) { //TODO: Track other entities entering beds.
            var myPacket = (CBUseBed)packet;

            if (client.ThisPlayer != null && myPacket.EntityID == client.ThisPlayer.Entity_ID)
                client.ThisPlayer.inBed = true;
        }
        public void HandleWindowItems(MinecraftClient client, IPacket packet) {
            var myPacket = (CBWindowItems)packet;

            for (int i = 0; i < myPacket.Count; i++) {
                if (myPacket.WindowID == 0) 
                    client.ThisPlayer.SetInventory(Item.ItemFromSlot(myPacket.Slotdata[i]), (short)i);
                 else 
                    client.RaiseSetWindowSlot((sbyte)myPacket.WindowID, (short)i, Item.ItemFromSlot(myPacket.Slotdata[i]));
                
            }
        }
        #endregion
    }
}
