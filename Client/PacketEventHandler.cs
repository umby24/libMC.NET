using System;
using System.Collections.Generic;
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
        public string Translate, Color, Text, Unknown;
        public bool Italic, Bold, Strikethrough, Obfs;
        public JArray With;
        public List<string> Names;
    } 

    class PacketEventHandler {
        public PacketEventHandler(NetworkHandler nh) {
            // -- Login packets 
            nh.RegisterLoginHandler(0, HandleLoginDisconnect);
            nh.RegisterLoginHandler(1, HandleEncryptionRequest);
            nh.RegisterLoginHandler(2, HandleLoginSuccess);

            // -- Status Packets
            nh.RegisterStatusHandler(0, HandleStatusResponse);
            nh.RegisterStatusHandler(1, HandleStatusPing);

            // -- Play packets
            nh.RegisterPlayHandler(0, HandleKeepAlive);
            nh.RegisterPlayHandler(0x01, HandleJoinGame);
            nh.RegisterPlayHandler(0x02, HandleChat);
            nh.RegisterPlayHandler(0x04, HandleEntityEquipment);
            nh.RegisterPlayHandler(0x07, HandleRespawn);
            nh.RegisterPlayHandler(0x09, HandleHeldItemChange);
            nh.RegisterPlayHandler(0x0B, HandleAnimation);
            nh.RegisterPlayHandler(0x0D, HandleCollectItem);
            nh.RegisterPlayHandler(0x12, HandleEntityVelocity);
            nh.RegisterPlayHandler(0x13, HandleDestroyEntities);
            nh.RegisterPlayHandler(0x15, HandleEntityRelMove);
            nh.RegisterPlayHandler(0x16, HandleEntityLook);
            nh.RegisterPlayHandler(0x17, HandleLookEntityRelMove);
            nh.RegisterPlayHandler(0x18, HandleEntityTeleport);
            nh.RegisterPlayHandler(0x19, HandleEntityHeadLook);
            nh.RegisterPlayHandler(0x1A, HandleEntityStatus);
            nh.RegisterPlayHandler(0x1B, AttachEntity);
            nh.RegisterPlayHandler(0x1D, HandleEntityEffect);
            nh.RegisterPlayHandler(0x21, HandleChunkData);
            nh.RegisterPlayHandler(0x23, BlockChange);
            nh.RegisterPlayHandler(0x24, BlockAction);
            nh.RegisterPlayHandler(0x25, BlockBreakAnimation);
            nh.RegisterPlayHandler(0x26, HandleMapChunkBulk);
            nh.RegisterPlayHandler(0x27, HandleExplosion);
            nh.RegisterPlayHandler(0x28, HandleEffects);
            nh.RegisterPlayHandler(0x2B, ChangeGameState);
            nh.RegisterPlayHandler(0x2E, HandleCloseWindow);
            nh.RegisterPlayHandler(0x32, HandleConfirmTransaction);
            nh.RegisterPlayHandler(0x3D, HandleDisplayScoreboard);
            nh.RegisterPlayHandler(0x40, HandleDisconnect);
            
        }

        #region Login Packets
        public void HandleLoginDisconnect(MinecraftClient client, IPacket packet) {
            var disconnect = (CbLoginDisconnect)packet;

            client.RaiseLoginFailure(this, disconnect.JsonData);
            client.Disconnect();
        }

        public void HandleEncryptionRequest(MinecraftClient client, IPacket packet) {
            var er = (CbEncryptionRequest)packet;
            var sharedKey = new byte[16];

            var random = RandomNumberGenerator.Create(); // -- Generate a random shared key.
            random.GetBytes(sharedKey);

            if (er.ServerId == "" && client.VerifyNames) {
                // -- Verify with Minecraft.net.
                // -- At this point, the server requires a hash containing the server id,
                // -- shared key, and original public key. So we make this, and then pass to Minecraft.net

                var hashList = new List<byte>();
                hashList.AddRange(Encoding.ASCII.GetBytes(er.ServerId));
                hashList.AddRange(sharedKey);
                hashList.AddRange(er.PublicKey);

                var hashData = hashList.ToArray();
                var hash = JavaHexDigest(hashData);

                var verify = new MinecraftNetInteraction();

                if (!verify.VerifyName(client.ClientName, client.AccessToken, client.SelectedProfile, hash)) {
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

            var keyParser = new AsnKeyParser(er.PublicKey);
            var dekey = keyParser.ParseRSAPublicKey();

            // -- Now we create an encrypter, and encrypt the token sent to us by the server
            // -- as well as our newly made shared key (Which can then only be decrypted with the server's private key)
            // -- and we send it to the server.

            var cryptoService = new RSACryptoServiceProvider(); // -- RSA Encryption class
            cryptoService.ImportParameters(dekey); // -- Import the Server's public key to use as the RSA encryption key.

            var encryptedSecret = cryptoService.Encrypt(sharedKey, false); // -- Encrypt the Secret key and verification token.
            var encryptedVerify = cryptoService.Encrypt(er.VerifyToken, false);

            client.Nh.WSock.InitEncryption(sharedKey); // -- Give the shared secret key to the socket

            var response = new SbEncryptionResponse
            {
                SharedLength = (short) encryptedSecret.Length,
                SharedSecret = encryptedSecret,
                VerifyLength = (short) encryptedVerify.Length,
                VerifyToken = encryptedVerify
            }; // -- Respond to the server

            response.Write(client.Nh.WSock);

            client.Nh.WSock.EncEnabled = true;
            client.Nh.RaiseSocketInfo(this, "Encryption Enabled.");
        }

        #region Encryption Helping Functions
        private static string GetHexString(IEnumerable<byte> p) {
            var result = "";
            foreach (var t in p)
            {
                if (t < 0x10)
                    result += "0";
                result += t.ToString("x"); // Converts to hex string
            }
            return result;
        }

        private static byte[] TwosCompliment(byte[] p) // little endian
        {
            int i;
            var carry = true;
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
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(data);
            var negative = (hash[0] & 0x80) == 0x80;
            if (negative) // check for negative hashes
                hash = TwosCompliment(hash);
            // Create the string and trim away the zeroes
            var digest = GetHexString(hash).TrimStart('0');
            if (negative)
                digest = "-" + digest;
            return digest;
        }
        #endregion

        public void HandleLoginSuccess(MinecraftClient client, IPacket packet) {
            var success = (CbLoginSuccess)packet;
            client.RaiseLoginSuccess(this);
            client.RaiseDebug(this, "UUID: " + success.Uuid + " Username: " + success.Username);

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.PlayerName = success.Username;
            client.ServerState = 3;
            client.RaiseDebug(this, "The server state is now 3 (Play)");
        }
        #endregion
        #region Status Packets
        public void HandleStatusResponse(MinecraftClient client, IPacket packet) {
            List<string> players = null;
            Image favicon = null;

            var response = (CbResponse)packet;
            var jsonObj = JToken.Parse(response.JsonResponse);

            var versionName = jsonObj["version"]["name"].Value<string>();
            var protocolVersion = jsonObj["version"]["protocol"].Value<int>();

            var maxPlayers = jsonObj["players"]["max"].Value<int>();
            var onlinePlayers = jsonObj["players"]["online"].Value<int>();

            var tempPlayers = jsonObj["players"]["sample"];

            if (tempPlayers != null) {
                players = new List<string>();

                foreach (var jToken in tempPlayers) {
                    var b = (JObject) jToken;
                    players.Add(b.Last.First.ToString());
                }
            }

            var motd = jsonObj["description"].Value<string>();
            var imageString = jsonObj["favicon"].Value<string>();

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

            if (players != null)
                client.RaisePingResponse(versionName, protocolVersion, maxPlayers, onlinePlayers, players.ToArray(), motd, favicon);

            var ping = new SbPing {Time = DateTime.UtcNow.Ticks};
            ping.Write(client.Nh.WSock);
        }
        public void HandleStatusPing(MinecraftClient client, IPacket packet) {
            var ping = (CbPing)packet;
            client.RaisePingMs((int)(DateTime.UtcNow.Ticks - ping.Time) / 10000); // -- 10,000 ticks per millisecond.
            client.Nh.RaiseSocketDebug(this, "Server ping complete.");
        }
        #endregion
        #region Play Packets
        public void HandleAnimation(MinecraftClient client, IPacket packet) {
            var animation = (CbAnimation)packet;

            if (client.ThisPlayer != null && animation.EntityId == client.ThisPlayer.EntityId)
                client.ThisPlayer.Animation = animation.Animation;

            if (client.MinecraftWorld != null) {
                var index = client.MinecraftWorld.GetEntityById(animation.EntityId);
                if (index != -1)
                    client.MinecraftWorld.Entities[index].Animation = (sbyte)animation.Animation;
            }

            client.RaiseEntityAnimationChanged(this, animation.EntityId, animation.Animation);
        }

        public void AttachEntity(MinecraftClient client, IPacket packet) {
            var attach = (CbAttachEntity)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(attach.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Attached = true;
                    client.MinecraftWorld.Entities[eIndex].VehicleId = attach.VehicleId;
                    client.MinecraftWorld.Entities[eIndex].Leashed = attach.Leash;
                }
            }

            client.RaiseEntityAttached(attach.EntityId, attach.VehicleId, attach.Leash);
        }

        public void BlockAction(MinecraftClient client, IPacket packet) {
            var blockPacket = (CbBlockAction)packet;

            switch (blockPacket.BlockType) {
                case 25: // -- Note block
                    client.RaiseNoteBlockSound(blockPacket.Byte1, blockPacket.Byte2, blockPacket.X, blockPacket.Y, blockPacket.Z);
                    break;
                case 29: // -- Sticky Piston
                    client.RaisePistonMoved(blockPacket.Byte1, blockPacket.Byte2, blockPacket.X, blockPacket.Y, blockPacket.Z);
                    break;
                case 33: // -- Piston
                    client.RaisePistonMoved(blockPacket.Byte1, blockPacket.Byte2, blockPacket.X, blockPacket.Y, blockPacket.Z);
                    break;
                case 54: // -- Chest
                    client.RaiseChestStateChange(blockPacket.Byte2, blockPacket.X, blockPacket.Y, blockPacket.Z);
                    break;
                case 146: // -- Trapped chest
                    client.RaiseChestStateChange(blockPacket.Byte2, blockPacket.X, blockPacket.Y, blockPacket.Z);
                    break;
                default:
                    client.RaiseError(this, "Unknown block action received: " + blockPacket.BlockType);
                    break;
            }
        }

        public void BlockBreakAnimation(MinecraftClient client, IPacket packet) {
            var blockPacket = (CbBlockBreakAnimation)packet;
            client.RaiseBlockBreakingEvent(new Vector(blockPacket.X, blockPacket.Y, blockPacket.Z), blockPacket.EntityId, blockPacket.Stage);
        }

        public void BlockChange(MinecraftClient client, IPacket packet) {
            var blockPacket = (CbBlockChange)packet;

            var chunkX = decimal.Divide(blockPacket.X, 16);
            var chunkZ = decimal.Divide(blockPacket.Z, 16);

            chunkX = Math.Floor(chunkX);
            chunkZ = Math.Floor(chunkZ);

            var myIndex = client.MinecraftWorld.GetChunk(int.Parse(chunkX.ToString()), int.Parse(chunkZ.ToString()));

            if (myIndex == -1)
                return;

            var myChunk = client.MinecraftWorld.WorldChunks[myIndex];
            myChunk.UpdateBlock(blockPacket.X, blockPacket.Y, blockPacket.Z, blockPacket.BlockId);
            myChunk.SetBlockData(blockPacket.X, blockPacket.Y, blockPacket.Z, blockPacket.BlockMetadata);

            client.RaiseBlockChangedEvent(blockPacket.X, blockPacket.Y, blockPacket.Z, blockPacket.BlockId, blockPacket.BlockMetadata);
        }

        public void ChangeGameState(MinecraftClient client, IPacket packet) {
            var gamePacket = (CbChangeGameState)packet;
            var eventName = "";

            switch (gamePacket.Reason) {
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

            client.RaiseGameStateChanged(eventName, gamePacket.Value);
        }

        public void HandleChat(MinecraftClient client, IPacket packet) {
            var chat = (CbChatMessage)packet;

            //string parsedMessage = ParseJsonChat(Chat.JsonData);//ParseJsonChat(Chat.JsonData, ref sender);

            client.RaiseMc(this, chat.JsonData, chat.JsonData);
        }
        #region Chat Message Helping Functions
        ChatObject ParseElement(JObject jsonObj) {
            var chat = new ChatObject();

            foreach (var prop in jsonObj.Properties()) {
                switch (prop.Name) {
                    case "translate":
                        chat.Translate = (string)prop.Value;
                        break;
                    case "italic":
                        chat.Italic = (bool)prop.Value;
                        break;
                    case "color":
                        chat.Color = (string)prop.Value;
                        break;
                    case "text":
                        chat.Text = (string)prop.Value;
                        break;
                    case "with":
                        chat.With = (JArray)prop.Value;
                        break;
                    default:
                        chat.Unknown = prop.Name;
                        break;
                }
            }

            return chat;
        }

        ChatObject ParseSecondaryElement(JObject jsonObj, ChatObject chatObj) {
            foreach (var prop in jsonObj.Properties()) {
                switch (prop.Name) {
                    case "translate":
                        chatObj.Translate = (string)prop.Value;
                        break;
                    case "with":
                        chatObj.Names.Add((string)((JValue)((JArray)prop.Value)[0]).Value);
                        break;
                    case "text":
                        chatObj.Names.Add((string)prop.Value);
                        break;
                    case "extra":
                        foreach (var jToken in (JArray)prop.Value) {
                            var b = (JValue) jToken;
                            if (b.Type == JTokenType.String)
                                chatObj.Text += (string)b.Value + " ";
                        }
                        break;
                }
            }

            return chatObj;
        }

        string ParseModifiers(ChatObject mainObj, string text) {
            if (mainObj.Italic)
                text += "§o";

            if (mainObj.Bold)
                text += "§l";

            if (mainObj.Strikethrough)
                text += "§m";

            if (mainObj.Obfs)
                text += "§k";

            if (mainObj.Color != null)
                text += Color_To_Code(mainObj.Color);

            return text;
        }

        ChatObject ParseWith(IEnumerable<JToken> with, ChatObject chatObj)
        {
            foreach (JToken token in with)
            {
                switch (token.Type) {
                    case JTokenType.String: // -- Add a name
                        if (chatObj.Translate == "chat.type.text")
                            chatObj.Text += (string)((JValue)token).Value;
                        else
                            chatObj.Names.Add((string)((JValue)token).Value);
                            
                        break;
                    case JTokenType.Object:
                        var myObj = (JObject)token;
                        chatObj = ParseSecondaryElement(myObj, chatObj);
                        break;
                }
            }

            return chatObj;
        }

        string ParseBaseMessage(string type) {
            switch (type) {
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
            foreach (var jToken in myObj.Children()) {
                var prop = (JProperty) jToken;
                switch (prop.Name) {
                    case "color":
                        chatObj.Text += Color_To_Code((string)prop.Value);
                        break;
                    case "text":
                        chatObj.Text += (string)prop.Value;
                        break;
                }
            }

            return chatObj;
        }

        string ParseJsonChat(string raw) {
            var final = "";
            var mainObj = ParseElement(JObject.Parse(raw));
            mainObj.Names = new List<string>();

            final = ParseModifiers(mainObj, final);
            mainObj = ParseWith(mainObj.With, mainObj);

            if (JObject.Parse(raw)["extra"] != null) {
                foreach (var jToken in (JArray)JObject.Parse(raw)["extra"]) {
                    var c = (JObject) jToken;
                    mainObj = DoExtra(mainObj, c);
                }
            }

            mainObj.Names.RemoveAll(string.IsNullOrWhiteSpace);
            return string.Format(ParseBaseMessage(mainObj.Translate), mainObj.Names.ToArray());
        }

        public string Color_To_Code(string color) {
            var code = "";

            switch (color) {
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
            var chunkData = (CbChunkData)packet;

            var trim = new byte[chunkData.Compressedsize - 2];

            if (chunkData.Primarybitmap == 0) {
                // -- Unload chunk.
                var cIndex = -1;

                if (client.MinecraftWorld != null)
                    cIndex = client.MinecraftWorld.GetChunk(chunkData.ChunkX, chunkData.ChunkZ);

                if (cIndex != -1)
                    if (client.MinecraftWorld != null) 
                        client.MinecraftWorld.WorldChunks.RemoveAt(cIndex);

                client.RaiseChunkUnload(chunkData.ChunkX, chunkData.ChunkZ);
                return;
            }

            // -- Remove GZip Header
            Buffer.BlockCopy(chunkData.Compresseddata, 2, trim, 0, trim.Length);

            // -- Decompress the data
            byte[] decompressedData = Decompressor.Decompress(trim);

            // -- Create new chunk
            var newChunk = new Chunk(chunkData.ChunkX, chunkData.ChunkZ, (short)chunkData.Primarybitmap, (short)chunkData.Addbitmap, true, chunkData.GroundUpcontinuous); // -- Skylight assumed true
            newChunk.GetData(decompressedData);

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            // -- Add the chunk to the world
            client.MinecraftWorld.WorldChunks.Add(newChunk);

            client.RaiseChunkLoad(chunkData.ChunkX, chunkData.ChunkZ);
        }
        public void HandleCloseWindow(MinecraftClient client, IPacket packet) {
            var myWindow = (CbCloseWindow)packet;
            client.RaiseWindowClosed(myWindow.WindowId);
        }
        public void HandleCollectItem(MinecraftClient client, IPacket packet) {
            var myCollection = (CbCollectItem)packet;
            client.RaiseItemCollected(myCollection.CollectedEntityId, myCollection.CollectorEntityId);
        }
        public void HandleConfirmTransaction(MinecraftClient client, IPacket packet) {
            var myPacket = (CbConfirmTransaction)packet;

            if (myPacket.Accepted)
                client.RaiseTransactionAccepted(myPacket.WindowId, myPacket.Actionnumber);
            else
                client.RaiseTransactionRejected(myPacket.WindowId, myPacket.Actionnumber);
        }
        public void HandleDestroyEntities(MinecraftClient client, IPacket packet) {
            var myPacket = (CbDestroyEntities)packet;

            if (client.MinecraftWorld == null)
                return;

            for (var x = 0; x < myPacket.Count; x++) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityIDs[x]);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities.RemoveAt(eIndex);

                client.RaiseEntityDestruction(myPacket.EntityIDs[x]);
            }
        }
        public void HandleDisconnect(MinecraftClient client, IPacket packet) {
            var disconnect = (CbDisconnect)packet;

            client.RaiseInfo(this, "You were kicked! Reason: " + disconnect.Reason);
            client.RaiseKicked(disconnect.Reason);
            client.Disconnect();
        }
        public void HandleDisplayScoreboard(MinecraftClient client, IPacket packet) {
            var myPacket = (CbDisplayScoreboard)packet;
            client.RaiseScoreBoard(myPacket.Position, myPacket.ScoreName);
        }
        public void HandleEffects(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEffect)packet;
            //TODO: Implement this. Pull requests welcome and are encouraged for parsing the IDs and raising an event for this.
        }
        public void HandleEntityEffect(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityEffect)packet;

            if (client.MinecraftWorld == null)
                return;

            var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

            if (eIndex != -1) {
                client.MinecraftWorld.Entities[eIndex].Amplifier = myPacket.Amplifier;
                client.MinecraftWorld.Entities[eIndex].Duration = myPacket.Duration;
                client.MinecraftWorld.Entities[eIndex].Status = myPacket.EffectId;
                client.RaiseEntityStatus(myPacket.EntityId);
            }
        }
        public void HandleEntityEquipment(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityEquipment)packet;

            if (client.ThisPlayer != null && myPacket.EntityId == client.ThisPlayer.EntityId) { 
                client.ThisPlayer.SetInventory(Item.ItemFromSlot(myPacket.Item), myPacket.Slot);
                return;
            }

            if (client.MinecraftWorld != null && client.MinecraftWorld.Entities != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].HandleInventory(myPacket.Slot, Item.ItemFromSlot(myPacket.Item));
            }
        }
        public void HandleEntityHeadLook(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityHeadLook)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].HeadPitch = myPacket.HeadYaw;
            }

            client.RaiseEntityHeadLookChanged(myPacket.EntityId, myPacket.HeadYaw);
        }

        public void HandleEntityLook(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityLook)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Pitch = myPacket.Pitch;
                    client.MinecraftWorld.Entities[eIndex].Yaw = myPacket.Yaw;
                }
            }

            client.RaiseEntityLookChanged(myPacket.EntityId, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleLookEntityRelMove(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityLookandRelativeMove)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.X += (myPacket.Dx * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.Y += (myPacket.Dy * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.Z += (myPacket.Dz * 32);
                    client.MinecraftWorld.Entities[eIndex].Yaw = myPacket.Yaw;
                    client.MinecraftWorld.Entities[eIndex].Pitch = myPacket.Pitch;
                }
            }

            client.RaiseEntityRelMove(myPacket.EntityId, myPacket.Dx * 32, myPacket.Dy * 32, myPacket.Dz * 32);
            client.RaiseEntityLookChanged(myPacket.EntityId, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleEntityMetadata(MinecraftClient client, IPacket packet) {
            //TODO:
            // -- This needs to be written
        }
        public void HandleEntityProperties(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityProperties)packet;

            //TODO: This
        }
        public void HandleEntityRelMove(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityRelativeMove)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.X += (myPacket.Dx * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.Y += (myPacket.Dy * 32);
                    client.MinecraftWorld.Entities[eIndex].Location.Z += (myPacket.Dz * 32);
                }
            }

            client.RaiseEntityRelMove(myPacket.EntityId, myPacket.Dx, myPacket.Dy, myPacket.Dz);
        }

        public void HandleEntityStatus(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityStatus)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1)
                    client.MinecraftWorld.Entities[eIndex].Status = myPacket.EntityStatus;
            }

            client.RaiseEntityStatus(myPacket.EntityId);
        }

        public void HandleEntityTeleport(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityTeleport)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Location.X = myPacket.X;
                    client.MinecraftWorld.Entities[eIndex].Location.Y = myPacket.Y;
                    client.MinecraftWorld.Entities[eIndex].Location.Z = myPacket.Z;
                    client.MinecraftWorld.Entities[eIndex].Yaw = myPacket.Yaw;
                    client.MinecraftWorld.Entities[eIndex].Pitch = myPacket.Pitch;
                }
            }

            client.RaiseEntityTeleport(myPacket.EntityId, myPacket.X, myPacket.Y, myPacket.Z);
            client.RaiseEntityLookChanged(myPacket.EntityId, myPacket.Yaw, myPacket.Pitch);
        }

        public void HandleEntityVelocity(MinecraftClient client, IPacket packet) {
            var myPacket = (CbEntityVelocity)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].VelocityX = myPacket.VelocityX;
                    client.MinecraftWorld.Entities[eIndex].VelocityY = myPacket.VelocityY;
                    client.MinecraftWorld.Entities[eIndex].VelocityZ = myPacket.VelocityZ;
                }
            }

            client.RaiseEntityVelocityChanged(myPacket.EntityId, myPacket.VelocityX, myPacket.VelocityY, myPacket.VelocityZ);
        }

        public void HandleExplosion(MinecraftClient client, IPacket packet) {
            var myPacket = (CbExplosion)packet;
            //TODO: Handle more of this...
            client.RaiseExplosion(myPacket.X, myPacket.Y, myPacket.Z);
        }

        public void HandleHeldItemChange(MinecraftClient client, IPacket packet) {
            var myPacket = (CbHeldItemChange)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.SelectedSlot = myPacket.Slot;

            var mySend = new SbHeldItemChange {Slot = client.ThisPlayer.SelectedSlot};
            mySend.Write(client.Nh.WSock);
        }

        public void HandleJoinGame(MinecraftClient client, IPacket packet) {
            var myPacket = (CbJoinGame)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.EntityId = myPacket.EntityId;
            client.ThisPlayer.GameMode = myPacket.Gamemode;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.Difficulty = myPacket.Difficulty;
            client.MinecraftWorld.Dimension = myPacket.Dimension;
            client.MinecraftWorld.MaxPlayers = myPacket.MaxPlayers;
            client.MinecraftWorld.LevelType = myPacket.LevelType;

            client.RaiseDebug(this, string.Format("Entity ID: {0}", myPacket.EntityId));
            client.RaiseGameJoined();

            var b = new SbClientSettings
            {
                Locale = "en_US",
                Viewdistance = 5,
                Chatflags = 3,
                Chatcolours = true,
                Difficulty = 1,
                ShowCape = false
            };
            b.Write(client.Nh.WSock);

            var c = new SbPluginMessage {Channel = "MC|Brand", Data = Encoding.UTF8.GetBytes(client.ClientBrand)};
            c.Length = (short)c.Data.Length;
            c.Write(client.Nh.WSock);
        }

        public void HandleKeepAlive(MinecraftClient client, IPacket packet) {
            var ka = (CbKeepAlive)packet;

            var kas = new SbKeepAlive {KeepAliveId = ka.KeepAliveId};
            kas.Write(client.Nh.WSock);
        }
        public void HandleMapChunkBulk(MinecraftClient client, IPacket packet) {
            var chunkPacket = (CbMapChunkBulk)packet;
            var offset = 0;

            var trim = new byte[chunkPacket.Datalength - 2];

            var chunks = new Chunk[chunkPacket.Chunkcolumncount];

            Buffer.BlockCopy(chunkPacket.Data, 2, trim, 0, trim.Length);

            byte[] decompressedData = Decompressor.Decompress(trim);

            for (var i = 0; chunkPacket.Chunkcolumncount > i; i++) {
                var x = BitConverter.ToInt32(chunkPacket.Metainformation, offset);
                var z = BitConverter.ToInt32(chunkPacket.Metainformation, offset + 4);
                var pbitmap = ReverseBytes(BitConverter.ToInt16(chunkPacket.Metainformation, offset + 8));
                var abitmap = ReverseBytes(BitConverter.ToInt16(chunkPacket.Metainformation, offset + 10));
                offset += 12;

                chunks[i] = new Chunk(x, z, pbitmap, abitmap, chunkPacket.Skylightsent, true); // -- Assume true for Ground Up Continuous

                decompressedData = chunks[i].GetData(decompressedData); // -- Calls the chunk class to take all of the bytes it needs, and return whats left.

                if (client.MinecraftWorld == null)
                    client.MinecraftWorld = new WorldClass();

                client.MinecraftWorld.WorldChunks.Add(chunks[i]);
            }
        }
        #region MapChunkBulk Helping Methods
        public static short ReverseBytes(short value) {
            return (short)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        #endregion
        public void HandleMaps(MinecraftClient client, IPacket packet) {
            var myPacket = (CbMaps)packet;
            // -- Still don't know what this is for.
        }
        public void HandleMultiBlockChange(MinecraftClient client, IPacket packet) {
            var myPacket = (CbMultiBlockChange)packet;
            var chunkId = client.MinecraftWorld.GetChunk(myPacket.ChunkX, myPacket.ChunkZ);

            if (chunkId == -1) {
                client.RaiseError(this, "Attempted to access uninitialized chunk");
                return;
            }

            var thisChunk = client.MinecraftWorld.WorldChunks[chunkId];

            for (var i = 0; i < myPacket.Recordcount - 1; i++) {
                var thisRecord = myPacket.Records[i];

                thisChunk.UpdateBlock(thisRecord.X, thisRecord.Y, thisRecord.Z, thisRecord.BlockId);
                thisChunk.SetBlockData(thisRecord.X, thisRecord.Y, thisRecord.Z, thisRecord.Metadata);
            }

            client.RaiseMultiBlockChange(myPacket.ChunkX, myPacket.ChunkX);
        }
        public void HandleOpenWindow(MinecraftClient client, IPacket packet) {
            var myPacket = (CbOpenWindow)packet;
            client.RaiseOpenWindow(myPacket.Windowid, myPacket.InventoryType, myPacket.Windowtitle, myPacket.NumberofSlots, myPacket.Useprovidedwindowtitle);
            client.RaiseDebug(this, "Window opened forcibly");
        }
        //TODO: Particle
        public void HandlePlayerAbilities(MinecraftClient client, IPacket packet) {
            var myPacket = (CbPlayerAbilities)packet;
            client.ThisPlayer.FlyingSpeed = myPacket.Flyingspeed;
            client.ThisPlayer.WalkingSpeed = myPacket.Walkingspeed;
        }
        public void HandlePlayerListItem(MinecraftClient client, IPacket packet) {
            var myPacket = (CbPlayerListItem)packet;

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
            var myPacket = (CbPlayerPositionAndLook)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.Location.X = myPacket.X;
            client.ThisPlayer.Location.Y = myPacket.Y;
            client.ThisPlayer.Location.Z = myPacket.Z;
            client.ThisPlayer.Look[0] = myPacket.Yaw;
            client.ThisPlayer.Look[1] = myPacket.Pitch;
            client.ThisPlayer.OnGround = myPacket.OnGround;

            client.RaiseLocationChanged();

            var sending = new SbPlayerPositionAndLook
            {
                X = client.ThisPlayer.Location.X,
                FeetY = client.ThisPlayer.Location.Y - 1.620,
                HeadY = client.ThisPlayer.Location.Y,
                Z = client.ThisPlayer.Location.Z,
                Yaw = client.ThisPlayer.Look[0],
                Pitch = client.ThisPlayer.Look[1],
                OnGround = client.ThisPlayer.OnGround
            };
            sending.Write(client.Nh.WSock);

        }
        public void HandlePluginMessage(MinecraftClient client, IPacket packet) {
            var myPacket = (CbPluginMessage)packet;
            client.RaisePluginMessage(myPacket.Channel, myPacket.Data);
        }
        public void HandleRemoveEntityEffect(MinecraftClient client, IPacket packet) {
            var myPacket = (CbRemoveEntityEffect)packet;

            if (client.MinecraftWorld != null) {
                var eIndex = client.MinecraftWorld.GetEntityById(myPacket.EntityId);

                if (eIndex != -1) {
                    client.MinecraftWorld.Entities[eIndex].Status = myPacket.EffectId;
                    client.RaiseEntityStatus(myPacket.EntityId);
                }
            }
        }
        public void HandleRespawn(MinecraftClient client, IPacket packet) {
            var respawn = (CbRespawn)packet;

            client.MinecraftWorld = new WorldClass
            {
                Dimension = (sbyte) respawn.Dimension,
                Difficulty = respawn.Difficulty,
                LevelType = respawn.LevelType
            };

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.GameMode = respawn.Gamemode;

            client.RaisePlayerRespawn();
        }
        public void HandleScoreboardObjectvice(MinecraftClient client, IPacket packet) {
            var myPacket = (CbScoreboardObjective)packet;

            if (myPacket.Create == 0)
                client.RaiseScoreboardAdd(myPacket.Objectivename, myPacket.Objectivevalue);
            else if (myPacket.Create == 1)
                client.RaiseScoreboardUpdate(myPacket.Objectivename, myPacket.Objectivevalue);
            else if (myPacket.Create == 2)
                client.RaiseScoreboardRemove(myPacket.Objectivename);
        }
        public void HandleSetExperience(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSetExperience)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.ExpBar = myPacket.Experiencebar;
            client.ThisPlayer.Level = myPacket.Level;
            client.ThisPlayer.TotalExp = myPacket.TotalExperience;

            client.RaiseExperienceUpdate(myPacket.Experiencebar, myPacket.Level, myPacket.TotalExperience);
        }
        public void HandleSetSlot(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSetSlot)packet;
            var myItem = Item.ItemFromSlot(myPacket.Slotdata);

            if (myPacket.WindowId == 0) {
                client.ThisPlayer.SetInventory(myItem, myPacket.Slot);
                client.RaiseInventoryItem(myPacket.Slot, myItem);
            } else
                client.RaiseSetWindowSlot(myPacket.WindowId, myPacket.Slot, myItem);
        }
        //TODO: Sign Editor, Sound Effect, SpawnExpOrb
        public void HandleGlobalEntitySpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnGlobalEntity)packet;
            client.RaiseDebug(this, "A thunderbolt struck at " + myPacket.X + " " + myPacket.Y + " " + myPacket.Z);
        }
        public void HandleMobSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnMob)packet;
            var newMob = new Entity("Mob")
            {
                Entity_ID = myPacket.EntityId,
                MobType = myPacket.Type,
                Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z),
                Pitch = myPacket.Pitch,
                HeadPitch = myPacket.HeadPitch,
                Yaw = myPacket.Yaw,
                VelocityX = myPacket.VelocityX,
                VelocityY = myPacket.VelocityY,
                VelocityZ = myPacket.VelocityZ
            };

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newMob);
        }
        public void HandleObjectSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnObject)packet;
            var newObj = new ObjectEntity(myPacket.Type);

            newObj.GetFriendlyName(myPacket.Data.ObjectId);
            newObj.SpeedX = myPacket.Data.SpeedX;
            newObj.SpeedY = myPacket.Data.SpeedY;
            newObj.SpeedZ = myPacket.Data.SpeedZ;

            newObj.ObjectId = myPacket.EntityId;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.WorldObjects == null)
                client.MinecraftWorld.WorldObjects = new List<ObjectEntity>();

            client.MinecraftWorld.WorldObjects.Add(newObj);
        }
        public void HandlePaintingSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnPainting)packet;
            var newEntity = new Entity("Painting")
            {
                Entity_ID = myPacket.EntityId,
                PlayerName = myPacket.Title,
                Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z),
                Direction = myPacket.Direction
            };

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newEntity);
        }
        public void HandlePlayerSpawn(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnPlayer)packet;
            var newPlayer = new Entity("Player")
            {
                Entity_ID = myPacket.EntityId,
                Uuid = myPacket.PlayerUuid,
                PlayerName = myPacket.PlayerName,
                Location = new Vector(myPacket.X, myPacket.Y, myPacket.Z),
                Yaw = myPacket.Yaw,
                Pitch = myPacket.Pitch,
                HeldItem = myPacket.CurrentItem
            };

            //TODO: Metadata..

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            if (client.MinecraftWorld.Entities == null)
                client.MinecraftWorld.Entities = new List<Entity>();

            client.MinecraftWorld.Entities.Add(newPlayer);
        }
        public void HandleSpawnPosition(MinecraftClient client, IPacket packet) {
            var myPacket = (CbSpawnPosition)packet;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.SpawnX = myPacket.X;
            client.MinecraftWorld.SpawnY = myPacket.Y;
            client.MinecraftWorld.SpawnZ = myPacket.Z;
        }
        public void HandleStatistics(MinecraftClient client, IPacket packet) {
            //TODO: Make events for this.
        }
        public void HandleTabComplete(MinecraftClient client, IPacket packet) {
            var myPacket = (CbTabComplete)packet;

            client.RaiseTabComplete(myPacket.Matches);
        }
        public void HandleTimeUpdate(MinecraftClient client, IPacket packet) {
            var myPacket = (CbTimeUpdate)packet;

            if (client.MinecraftWorld == null)
                client.MinecraftWorld = new WorldClass();

            client.MinecraftWorld.WorldAge = myPacket.Ageoftheworld;
            client.MinecraftWorld.CurrentTime = myPacket.Timeofday;

            var playerPacket = new SbPlayer {OnGround = client.ThisPlayer.OnGround};
            playerPacket.Write(client.Nh.WSock);

            if (client.Nh.WorldTick == null)
                client.Nh.WorldTick = new MinecraftWorld.TickHandler(ref client);

        }
        public void HandleUpdateHealth(MinecraftClient client, IPacket packet) {
            var myPacket = (CbUpdateHealth)packet;

            if (client.ThisPlayer == null)
                client.ThisPlayer = new Player();

            client.ThisPlayer.PlayerHealth = myPacket.Health;
            client.ThisPlayer.PlayerHunger = myPacket.Food;
            client.ThisPlayer.FoodSaturation = myPacket.FoodSaturation;

            client.RaisePlayerHealthUpdate(myPacket.Health, myPacket.Food, myPacket.FoodSaturation);
        }
        public void HandleUseBed(MinecraftClient client, IPacket packet) { //TODO: Track other entities entering beds.
            var myPacket = (CbUseBed)packet;

            if (client.ThisPlayer != null && myPacket.EntityId == client.ThisPlayer.EntityId)
                client.ThisPlayer.InBed = true;
        }
        public void HandleWindowItems(MinecraftClient client, IPacket packet) {
            var myPacket = (CbWindowItems)packet;

            for (var i = 0; i < myPacket.Count; i++) {
                if (myPacket.WindowId == 0) 
                    client.ThisPlayer.SetInventory(Item.ItemFromSlot(myPacket.Slotdata[i]), (short)i);
                 else 
                    client.RaiseSetWindowSlot((sbyte)myPacket.WindowId, (short)i, Item.ItemFromSlot(myPacket.Slotdata[i]));
                
            }
        }
        #endregion
    }
}
