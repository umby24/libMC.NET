using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using CWrapped;
using libMC.NET.Entities;
using libMC.NET.World;

namespace libMC.NET.Network {
    //public interface IPacket {
    //    void Read(Wrapped wSock);
    //    void Write(Wrapped wSock);
    //}

    // -- Status 0: Handshake
    public struct SBHandshake : IPacket {
        public int ProtocolVersion { get; set; }
        public string ServerAddress { get; set; }
        public ushort ServerPort { get; set; }
        public int NextState { get; set; }

        public void Read(Wrapped wSock) {
            ProtocolVersion = wSock.readVarInt();
            ServerAddress = wSock.readString();
            ServerPort = (ushort)wSock.readShort();
            NextState = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeVarInt(ProtocolVersion);
            wSock.writeString(ServerAddress);
            wSock.writeShort((short)ServerPort);
            wSock.writeVarInt(NextState);
            wSock.Purge();
        }
    }

    // -- Status 1: Login
    public struct SBLoginStart : IPacket {
        public string Name { get; set; }

        public void Read(Wrapped wSock) {
            Name = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeString(Name);
            wSock.Purge();
        }
    }

    public struct SBEncryptionResponse : IPacket {
        public int SharedSecretLength { get; set; }
        public byte[] SharedSecret { get; set; }
        public int VerifyTokenLength { get; set; }
        public byte[] VerifyToken { get; set; }

        public void Read(Wrapped wSock) {
            SharedSecretLength = wSock.readVarInt();
            SharedSecret = wSock.readByteArray(SharedSecretLength);
            VerifyTokenLength = wSock.readVarInt();
            VerifyToken = wSock.readByteArray(VerifyTokenLength);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeVarInt(SharedSecretLength);
            wSock.Send(SharedSecret);
            wSock.writeVarInt(VerifyTokenLength);
            wSock.Send(VerifyToken);
            wSock.Purge();
        }
    }

    public struct CBLoginDisconnect : IPacket {
        public string Reason { get; set; }

        public void Read(Wrapped wSock) {
            Reason = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeString(Reason);
            wSock.Purge();
        }
    }

    public struct CBEncryptionRequest : IPacket {
        public string ServerID { get; set; }
        public int PublicKeyLength { get; set; }
        public byte[] PublicKey { get; set; }
        public int VerifyTokenLength { get; set; }
        public byte[] VerifyToken { get; set; }

        public void Read(Wrapped wSock) {
            ServerID = wSock.readString();
            PublicKeyLength = wSock.readVarInt();
            PublicKey = wSock.readByteArray(PublicKeyLength);
            VerifyTokenLength = wSock.readVarInt();
            VerifyToken = wSock.readByteArray(VerifyTokenLength);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeString(ServerID);
            wSock.writeVarInt(PublicKeyLength);
            wSock.Send(PublicKey);
            wSock.writeVarInt(VerifyTokenLength);
            wSock.Send(VerifyToken);
            wSock.Purge();
        }
    }

    public struct CBLoginSuccess : IPacket {
        public string UUID { get; set; }
        public string Username { get; set; }

        public void Read(Wrapped wSock) {
            UUID = wSock.readString();
            Username = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x02);
            wSock.writeString(UUID);
            wSock.writeString(Username);
            wSock.Purge();
        }
    }

    public struct CBLoginSetCompression : IPacket {
        public int Threshold { get; set; }

        public void Read(Wrapped wSock) {
            Threshold = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x03);
            wSock.writeVarInt(Threshold);
            wSock.Purge();
        }
    }


    // -- Status 2: Status
    public struct SBRequest : IPacket {

        public void Read(Wrapped wSock) {
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.Purge();
        }
    }

    public struct SBPing : IPacket {
        public long Time { get; set; }

        public void Read(Wrapped wSock) {
            Time = wSock.readLong();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeLong(Time);
            wSock.Purge();
        }
    }

    public struct CBResponse : IPacket {
        public string JSONResponse { get; set; }

        public void Read(Wrapped wSock) {
            JSONResponse = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeString(JSONResponse);
            wSock.Purge();
        }
    }

    public struct CBPong : IPacket {
        public long Time { get; set; }

        public void Read(Wrapped wSock) {
            Time = wSock.readLong();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeLong(Time);
            wSock.Purge();
        }
    }


    // -- Status 3: Play

    #region Serverbound

    public struct SBKeepAlive : IPacket {
        public int KeepAliveID { get; set; }

        public void Read(Wrapped wSock) {
            KeepAliveID = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeVarInt(KeepAliveID);
            wSock.Purge();
        }
    }

    public struct SBChatMessage : IPacket {
        public string Message { get; set; }

        public void Read(Wrapped wSock) {
            Message = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeString(Message);
            wSock.Purge();
        }
    }

    public struct SBUseEntity : IPacket {
        // -- TODO: a
        public int Target { get; set; }
        public int Type { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float TargetZ { get; set; }

        public void Read(Wrapped wSock) {
            Target = wSock.readVarInt();
            Type = wSock.readVarInt();
            TargetX = wSock.readFloat();
            TargetY = wSock.readFloat();
            TargetZ = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x02);
            wSock.writeVarInt(Target);
            wSock.writeVarInt(Type);
            wSock.writeFloat(TargetX);
            wSock.writeFloat(TargetY);
            wSock.writeFloat(TargetZ);
            wSock.Purge();
        }
    }

    public struct SBPlayer : IPacket {
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x03);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct SBPlayerPosition : IPacket {
        public double X { get; set; }
        public double FeetY { get; set; }
        public double Z { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            FeetY = wSock.readDouble();
            Z = wSock.readDouble();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x04);
            wSock.writeDouble(X);
            wSock.writeDouble(FeetY);
            wSock.writeDouble(Z);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct SBPlayerLook : IPacket {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            Yaw = wSock.readFloat();
            Pitch = wSock.readFloat();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x05);
            wSock.writeFloat(Yaw);
            wSock.writeFloat(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct SBPlayerPositionAndLook : IPacket {
        public double X { get; set; }
        public double FeetY { get; set; }
        public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            FeetY = wSock.readDouble();
            Z = wSock.readDouble();
            Yaw = wSock.readFloat();
            Pitch = wSock.readFloat();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x06);
            wSock.writeDouble(X);
            wSock.writeDouble(FeetY);
            wSock.writeDouble(Z);
            wSock.writeFloat(Yaw);
            wSock.writeFloat(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct SBPlayerDigging : IPacket {
        public sbyte Status { get; set; }
        public Position Location { get; set; }
        public sbyte Face { get; set; }

        public void Read(Wrapped wSock) {
            Status = wSock.readSByte();
            Location = new Position().Unpack(wSock.readLong());
            Face = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x07);
            wSock.writeSByte(Status);
            wSock.writeLong(Location.Pack());
            wSock.writeSByte(Face);
            wSock.Purge();
        }
    }

    public struct SBPlayerBlockPlacement : IPacket {
        public Position Location { get; set; }
        public sbyte Face { get; set; }
        public SlotData HeldItem { get; set; }
        public sbyte CursorPositionX { get; set; }
        public sbyte CursorPositionY { get; set; }
        public sbyte CursorPositionZ { get; set; }
        public short Slot { get; set; }

        public void Read(Wrapped wSock) {
            Location = new Position().Unpack(wSock.readLong());
            Face = wSock.readSByte();
            HeldItem = WrappedExtension.ReadSlot(wSock);
            CursorPositionX = wSock.readSByte();
            CursorPositionY = wSock.readSByte();
            CursorPositionZ = wSock.readSByte();
            Slot = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x08);
            wSock.writeLong(Location.Pack());
            wSock.writeSByte(Face);
            WrappedExtension.WriteSlot(wSock, HeldItem);
            wSock.writeSByte(CursorPositionX);
            wSock.writeSByte(CursorPositionY);
            wSock.writeSByte(CursorPositionZ);
            wSock.writeShort(Slot);
            wSock.Purge();
        }
    }

    public struct SBAnimation : IPacket {
        public void Read(Wrapped wSock) {
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0A);
            wSock.Purge();
        }
    }

    public struct SBEntityAction : IPacket {
        public int ActionID { get; set; }
        public int JumpBoost { get; set; }

        public void Read(Wrapped wSock) {
            ActionID = wSock.readVarInt();
            JumpBoost = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0B);
            wSock.writeVarInt(ActionID);
            wSock.writeVarInt(JumpBoost);
            wSock.Purge();
        }
    }

    public struct SBSteerVehicle : IPacket {
        public float Forward { get; set; }
        public byte Flags { get; set; }

        public void Read(Wrapped wSock) {
            Forward = wSock.readFloat();
            Flags = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0C);
            wSock.writeFloat(Forward);
            wSock.writeByte(Flags);
            wSock.Purge();
        }
    }

    public struct SBCloseWindow : IPacket {

        public void Read(Wrapped wSock) {
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0D);
            wSock.Purge();
        }
    }

    public struct SBClickWindow : IPacket {
        public short Slot { get; set; }
        public sbyte Button { get; set; }
        public short ActionNumber { get; set; }
        public sbyte Mode { get; set; }
        public SlotData Clickeditem { get; set; }

        public void Read(Wrapped wSock) {
            Slot = wSock.readShort();
            Button = wSock.readSByte();
            ActionNumber = wSock.readShort();
            Mode = wSock.readSByte();
            Clickeditem = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0E);
            wSock.writeShort(Slot);
            wSock.writeSByte(Button);
            wSock.writeShort(ActionNumber);
            wSock.writeSByte(Mode);
            WrappedExtension.WriteSlot(wSock, Clickeditem);
            wSock.Purge();
        }
    }

    public struct SBConfirmTransaction : IPacket {
        public short ActionNumber { get; set; }
        public bool Accepted { get; set; }

        public void Read(Wrapped wSock) {
            ActionNumber = wSock.readShort();
            Accepted = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0F);
            wSock.writeShort(ActionNumber);
            wSock.writeBool(Accepted);
            wSock.Purge();
        }
    }

    public struct SBCreativeInventoryAction : IPacket {
        public short Slot { get; set; }
        public SlotData ClickedItem { get; set; }

        public void Read(Wrapped wSock) {
            Slot = wSock.readShort();
            ClickedItem = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x10);
            wSock.writeShort(Slot);
            WrappedExtension.WriteSlot(wSock, ClickedItem);
            wSock.Purge();
        }
    }

    public struct SBEnchantItem : IPacket {
        public sbyte WindowID { get; set; }
        public sbyte Enchantment { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readSByte();
            Enchantment = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x11);
            wSock.writeSByte(WindowID);
            wSock.writeSByte(Enchantment);
            wSock.Purge();
        }
    }

    public struct SBUpdateSign : IPacket {
        public Position Location { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }

        public void Read(Wrapped wSock) {
            Location = new Position().Unpack(wSock.readLong());
            Line1 = wSock.readString();
            Line2 = wSock.readString();
            Line3 = wSock.readString();
            Line4 = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x12);
            wSock.writeLong(Location.Pack());
            wSock.writeString(Line1);
            wSock.writeString(Line2);
            wSock.writeString(Line3);
            wSock.writeString(Line4);
            wSock.Purge();
        }
    }

    public struct SBPlayerAbilities : IPacket {
        public sbyte Flags { get; set; }
        public float FlyingSpeed { get; set; }
        public float WalkingSpeed { get; set; }

        public void Read(Wrapped wSock) {
            Flags = wSock.readSByte();
            FlyingSpeed = wSock.readFloat();
            WalkingSpeed = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x13);
            wSock.writeSByte(Flags);
            wSock.writeFloat(FlyingSpeed);
            wSock.writeFloat(WalkingSpeed);
            wSock.Purge();
        }
    }

    public struct SBTabComplete : IPacket {
        public string Text { get; set; }
        public bool HasPosition { get; set; }
        public Position LookedAtBlock { get; set; }

        public void Read(Wrapped wSock) {
            Text = wSock.readString();
            HasPosition = wSock.readBool();
            LookedAtBlock = new Position().Unpack(wSock.readLong());
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x14);
            wSock.writeString(Text);
            wSock.writeBool(HasPosition);
            wSock.writeLong(LookedAtBlock.Pack());
            wSock.Purge();
        }
    }

    public struct SBClientSettings : IPacket {
        public string Locale { get; set; }
        public sbyte ViewDistance { get; set; }
        public sbyte ChatMode { get; set; }
        public bool ChatColors { get; set; }
        public byte DisplayedSkinParts { get; set; }

        public void Read(Wrapped wSock) {
            Locale = wSock.readString();
            ViewDistance = wSock.readSByte();
            ChatMode = wSock.readSByte();
            ChatColors = wSock.readBool();
            DisplayedSkinParts = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x15);
            wSock.writeString(Locale);
            wSock.writeSByte(ViewDistance);
            wSock.writeSByte(ChatMode);
            wSock.writeBool(ChatColors);
            wSock.writeByte(DisplayedSkinParts);
            wSock.Purge();
        }
    }

    public struct SBClientStatus : IPacket {
        public int ActionID { get; set; }

        public void Read(Wrapped wSock) {
            ActionID = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x16);
            wSock.writeVarInt(ActionID);
            wSock.Purge();
        }
    }

    //public struct SBPluginMessage : IPacket {
    //    public string Channel { get; set; }
    //    public byte[] Data { get; set; }

    //    public void Read(Wrapped wSock) {
    //        Channel = wSock.readString();
    //        Data = wSock.readByteArray();
    //    }

    //    public void Write(Wrapped wSock) {
    //        wSock.writeVarInt(0x17);
    //        wSock.writeString(Channel);
    //        wSock.Send(Data);
    //        wSock.Purge();
    //    }
    //}

    public struct SBSpectate : IPacket {
        public Guid TargetPlayer { get; set; }

        public void Read(Wrapped wSock) {
            TargetPlayer = new Guid(wSock.readByteArray(16));
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x18);
            wSock.Send(TargetPlayer.ToByteArray());
            wSock.Purge();
        }
    }

    public struct SBResourcePackStatus : IPacket {
        public string Hash { get; set; }
        public int Result { get; set; }

        public void Read(Wrapped wSock) {
            Hash = wSock.readString();
            Result = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x19);
            wSock.writeString(Hash);
            wSock.writeVarInt(Result);
            wSock.Purge();
        }
    }

    #endregion

    #region Clientbound

    public struct CBKeepAlive : IPacket {
        public int KeepAliveID { get; set; }

        public void Read(Wrapped wSock) {
            KeepAliveID = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeVarInt(KeepAliveID);
            wSock.Purge();
        }
    }

    public struct CBJoinGame : IPacket {
        public int EntityID { get; set; }
        public byte Gamemode { get; set; }
        public sbyte Dimension { get; set; }
        public byte Difficulty { get; set; }
        public byte MaxPlayers { get; set; }
        public string LevelType { get; set; }
        public bool ReducedDebugInfo { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Gamemode = wSock.readByte();
            Dimension = wSock.readSByte();
            Difficulty = wSock.readByte();
            MaxPlayers = wSock.readByte();
            LevelType = wSock.readString();
            ReducedDebugInfo = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeInt(EntityID);
            wSock.writeByte(Gamemode);
            wSock.writeSByte(Dimension);
            wSock.writeByte(Difficulty);
            wSock.writeByte(MaxPlayers);
            wSock.writeString(LevelType);
            wSock.writeBool(ReducedDebugInfo);
            wSock.Purge();
        }
    }

    public struct CBChatMessage : IPacket {
        public string JSONData { get; set; }
        public sbyte Position { get; set; }

        public void Read(Wrapped wSock) {
            JSONData = wSock.readString();
            Position = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x02);
            wSock.writeString(JSONData);
            wSock.writeSByte(Position);
            wSock.Purge();
        }
    }

    public struct CBTimeUpdate : IPacket {
        public long WorldAge { get; set; }
        public long Timeofday { get; set; }

        public void Read(Wrapped wSock) {
            WorldAge = wSock.readLong();
            Timeofday = wSock.readLong();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x03);
            wSock.writeLong(WorldAge);
            wSock.writeLong(Timeofday);
            wSock.Purge();
        }
    }

    public struct CBEntityEquipment : IPacket {
        public int EntityID { get; set; }
        public short Slot { get; set; }
        public SlotData Item { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Slot = wSock.readShort();
            Item = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x04);
            wSock.writeVarInt(EntityID);
            wSock.writeShort(Slot);
            WrappedExtension.WriteSlot(wSock, Item);
            wSock.Purge();
        }
    }

    public struct CBSpawnPosition : IPacket {
        public Position Location { get; set; }

        public void Read(Wrapped wSock) {
            Location = new Position().Unpack(wSock.readLong());
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x05);
            wSock.writeLong(Location.Pack());
            wSock.Purge();
        }
    }

    public struct CBUpdateHealth : IPacket {
        public float Health { get; set; }
        public int Food { get; set; }
        public float FoodSaturation { get; set; }

        public void Read(Wrapped wSock) {
            Health = wSock.readFloat();
            Food = wSock.readVarInt();
            FoodSaturation = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x06);
            wSock.writeFloat(Health);
            wSock.writeVarInt(Food);
            wSock.writeFloat(FoodSaturation);
            wSock.Purge();
        }
    }

    public struct CBRespawn : IPacket {
        public int Dimension { get; set; }
        public byte Difficulty { get; set; }
        public byte Gamemode { get; set; }
        public string LevelType { get; set; }

        public void Read(Wrapped wSock) {
            Dimension = wSock.readInt();
            Difficulty = wSock.readByte();
            Gamemode = wSock.readByte();
            LevelType = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x07);
            wSock.writeInt(Dimension);
            wSock.writeByte(Difficulty);
            wSock.writeByte(Gamemode);
            wSock.writeString(LevelType);
            wSock.Purge();
        }
    }

    public struct CBPlayerPositionAndLook : IPacket {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public sbyte Flags { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            Y = wSock.readDouble();
            Z = wSock.readDouble();
            Yaw = wSock.readFloat();
            Pitch = wSock.readFloat();
            Flags = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x08);
            wSock.writeDouble(X);
            wSock.writeDouble(Y);
            wSock.writeDouble(Z);
            wSock.writeFloat(Yaw);
            wSock.writeFloat(Pitch);
            wSock.writeSByte(Flags);
            wSock.Purge();
        }
    }

    public struct CBHeldItemChange : IPacket {
        public sbyte Slot { get; set; }

        public void Read(Wrapped wSock) {
            Slot = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x09);
            wSock.writeSByte(Slot);
            wSock.Purge();
        }
    }

    public struct CBUseBed : IPacket {
        public Position Location { get; set; }

        public void Read(Wrapped wSock) {
            Location = new Position().Unpack(wSock.readLong());
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0A);
            wSock.writeLong(Location.Pack());
            wSock.Purge();
        }
    }

    public struct CBAnimation : IPacket {
        public byte Animation { get; set; }

        public void Read(Wrapped wSock) {
            Animation = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0B);
            wSock.writeByte(Animation);
            wSock.Purge();
        }
    }

    public struct CBSpawnPlayer : IPacket {
        public Guid PlayerUUID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }
        public short CurrentItem { get; set; }
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            PlayerUUID = new Guid(wSock.readByteArray(16));
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Yaw = wSock.readByte();
            Pitch = wSock.readByte();
            CurrentItem = wSock.readShort();
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0C);
            wSock.Send(PlayerUUID.ToByteArray());
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Yaw);
            wSock.writeByte(Pitch);
            wSock.writeShort(CurrentItem);
            WrappedExtension.WriteEntityMetadata(wSock, Metadata);
            wSock.Purge();
        }
    }

    public struct CBCollectItem : IPacket {
        public int CollectorEntityID { get; set; }

        public void Read(Wrapped wSock) {
            CollectorEntityID = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0D);
            wSock.writeVarInt(CollectorEntityID);
            wSock.Purge();
        }
    }

    public struct CBSpawnObject : IPacket {
        public sbyte Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public byte Pitch { get; set; }
        public byte Yaw { get; set; }
        public ObjectMetadata Data { get; set; }

        public void Read(Wrapped wSock) {
            Type = wSock.readSByte();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Pitch = wSock.readByte();
            Yaw = wSock.readByte();
            Data = WrappedExtension.ReadObjectMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0E);
            wSock.writeSByte(Type);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Pitch);
            wSock.writeByte(Yaw);
            WrappedExtension.WriteObjectMetadata(wSock, Data);
            wSock.Purge();
        }
    }

    public struct CBSpawnMob : IPacket {
        public byte Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }
        public byte HeadPitch { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            Type = wSock.readByte();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Yaw = wSock.readByte();
            Pitch = wSock.readByte();
            HeadPitch = wSock.readByte();
            VelocityX = wSock.readShort();
            VelocityY = wSock.readShort();
            VelocityZ = wSock.readShort();
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0F);
            wSock.writeByte(Type);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Yaw);
            wSock.writeByte(Pitch);
            wSock.writeByte(HeadPitch);
            wSock.writeShort(VelocityX);
            wSock.writeShort(VelocityY);
            wSock.writeShort(VelocityZ);
            WrappedExtension.WriteEntityMetadata(wSock, Metadata);
            wSock.Purge();
        }
    }

    public struct CBSpawnPainting : IPacket {
        public int EntityID { get; set; }
        public string Title { get; set; }
        public Position Location { get; set; }
        public byte Direction { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Title = wSock.readString();
            Location = new Position().Unpack(wSock.readLong());
            Direction = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x10);
            wSock.writeVarInt(EntityID);
            wSock.writeString(Title);
            wSock.writeLong(Location.Pack());
            wSock.writeByte(Direction);
            wSock.Purge();
        }
    }

    public struct CBSpawnExperienceOrb : IPacket {
        public int EntityID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public short Count { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Count = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x11);
            wSock.writeVarInt(EntityID);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeShort(Count);
            wSock.Purge();
        }
    }

    public struct CBEntityVelocity : IPacket {
        public int EntityID { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            VelocityX = wSock.readShort();
            VelocityY = wSock.readShort();
            VelocityZ = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x12);
            wSock.writeVarInt(EntityID);
            wSock.writeShort(VelocityX);
            wSock.writeShort(VelocityY);
            wSock.writeShort(VelocityZ);
            wSock.Purge();
        }
    }

    public struct CBDestroyEntities : IPacket {
        public int Count { get; set; }
        public int[] EntityIDs { get; set; }

        public void Read(Wrapped wSock) {
            Count = wSock.readVarInt();
            EntityIDs = new int[Count];

            for (int i = 0; i < Count; i++)
                EntityIDs[i] = wSock.readInt();

        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x13);
            wSock.writeVarInt(Count);
            foreach (var entityId in EntityIDs) {
                wSock.writeInt(entityId);
            }
            wSock.Purge();
        }
    }

    public struct CBEntity : IPacket {
        public int EntityID { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x14);
            wSock.writeVarInt(EntityID);
            wSock.Purge();
        }
    }

    public struct CBEntityRelativeMove : IPacket {
        public int EntityID { get; set; }
        public sbyte DeltaX { get; set; }
        public sbyte DeltaY { get; set; }
        public sbyte DeltaZ { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            DeltaX = wSock.readSByte();
            DeltaY = wSock.readSByte();
            DeltaZ = wSock.readSByte();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x15);
            wSock.writeVarInt(EntityID);
            wSock.writeSByte(DeltaX);
            wSock.writeSByte(DeltaY);
            wSock.writeSByte(DeltaZ);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct CBEntityLook : IPacket {
        public int EntityID { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Yaw = wSock.readByte();
            Pitch = wSock.readByte();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x16);
            wSock.writeVarInt(EntityID);
            wSock.writeByte(Yaw);
            wSock.writeByte(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct CBEntityLookAndRelativeMove : IPacket {
        public int EntityID { get; set; }
        public sbyte DeltaX { get; set; }
        public sbyte DeltaY { get; set; }
        public sbyte DeltaZ { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            DeltaX = wSock.readSByte();
            DeltaY = wSock.readSByte();
            DeltaZ = wSock.readSByte();
            Yaw = wSock.readByte();
            Pitch = wSock.readByte();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x17);
            wSock.writeVarInt(EntityID);
            wSock.writeSByte(DeltaX);
            wSock.writeSByte(DeltaY);
            wSock.writeSByte(DeltaZ);
            wSock.writeByte(Yaw);
            wSock.writeByte(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct CBEntityTeleport : IPacket {
        public int EntityID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public byte Yaw { get; set; }
        public byte Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Yaw = wSock.readByte();
            Pitch = wSock.readByte();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x18);
            wSock.writeVarInt(EntityID);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Yaw);
            wSock.writeByte(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct CBEntityHeadLook : IPacket {
        public int EntityID { get; set; }
        public byte HeadYaw { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            HeadYaw = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x19);
            wSock.writeVarInt(EntityID);
            wSock.writeByte(HeadYaw);
            wSock.Purge();
        }
    }

    public struct CBEntityStatus : IPacket {
        public sbyte EntityStatus { get; set; }

        public void Read(Wrapped wSock) {
            EntityStatus = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1A);
            wSock.writeSByte(EntityStatus);
            wSock.Purge();
        }
    }

    public struct CBAttachEntity : IPacket {
        public int VehicleID { get; set; }
        public bool Leash { get; set; }

        public void Read(Wrapped wSock) {
            VehicleID = wSock.readInt();
            Leash = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1B);
            wSock.writeInt(VehicleID);
            wSock.writeBool(Leash);
            wSock.Purge();
        }
    }

    public struct CBEntityMetadata : IPacket {
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1C);
            WrappedExtension.WriteEntityMetadata(wSock, Metadata);
            wSock.Purge();
        }
    }

    public struct CBEntityEffect : IPacket {
        public sbyte EffectID { get; set; }
        public sbyte Amplifier { get; set; }
        public int Duration { get; set; }
        public bool HideParticles { get; set; }

        public void Read(Wrapped wSock) {
            EffectID = wSock.readSByte();
            Amplifier = wSock.readSByte();
            Duration = wSock.readVarInt();
            HideParticles = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1D);
            wSock.writeSByte(EffectID);
            wSock.writeSByte(Amplifier);
            wSock.writeVarInt(Duration);
            wSock.writeBool(HideParticles);
            wSock.Purge();
        }
    }

    public struct CBRemoveEntityEffect : IPacket {
        public sbyte EffectID { get; set; }

        public void Read(Wrapped wSock) {
            EffectID = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1E);
            wSock.writeSByte(EffectID);
            wSock.Purge();
        }
    }

    public struct CBSetExperience : IPacket {
        public int Level { get; set; }
        public int TotalExperience { get; set; }

        public void Read(Wrapped wSock) {
            Level = wSock.readVarInt();
            TotalExperience = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1F);
            wSock.writeVarInt(Level);
            wSock.writeVarInt(TotalExperience);
            wSock.Purge();
        }
    }

    public struct CBEntityProperties : IPacket {
        public int EntityID { get; set; }
        public int Count { get; set; }
        public PropertyData[] Properties { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Count = wSock.readInt();
            Properties = new PropertyData[Count];

            for (var x = 0; x < Count; x++)
                Properties[x] = WrappedExtension.ReadPropertyData(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x20);
            wSock.writeInt(EntityID);
            wSock.writeInt(Count);

            for (var x = 0; x < Count; x++)
                WrappedExtension.WritePropertyData(wSock, Properties[x]);

            wSock.Purge();
        }

        public struct CBChunkData : IPacket {
            public int ChunkX { get; set; }
            public int ChunkZ { get; set; }
            public bool GroundUpContinuous { get; set; }
            public ushort PrimaryBitMask { get; set; }
            public int Size { get; set; }
            public byte[] Data { get; set; }

            public void Read(Wrapped wSock) {
                ChunkX = wSock.readInt();
                ChunkZ = wSock.readInt();
                GroundUpContinuous = wSock.readBool();
                PrimaryBitMask = (ushort)wSock.readShort();
                Size = wSock.readVarInt();
                Data = wSock.readByteArray(Size);
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x21);
                wSock.writeInt(ChunkX);
                wSock.writeInt(ChunkZ);
                wSock.writeBool(GroundUpContinuous);
                wSock.writeShort((short)PrimaryBitMask);
                wSock.writeVarInt(Size);
                wSock.Send(Data);
                wSock.Purge();
            }
        }

        public struct CBMultiBlockChange : IPacket {
            public int ChunkX { get; set; }
            public int ChunkZ { get; set; }
            public int RecordCount { get; set; }
            public Record[] Records { get; set; }

            public void Read(Wrapped wSock) {
                ChunkX = wSock.readInt();
                ChunkZ = wSock.readInt();
                RecordCount = wSock.readVarInt();
                Records = new Record[RecordCount];

                for (int i = 0; i < RecordCount; i++)
                    Records[i] = WrappedExtension.ReadRecord(wSock);
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x22);
                wSock.writeInt(ChunkX);
                wSock.writeInt(ChunkZ);
                wSock.writeVarInt(RecordCount);

                foreach (var record in Records)
                    WrappedExtension.WriteRecord(wSock, record);

                wSock.Purge();
            }
        }

        public struct CBBlockChange : IPacket {
            public Position Location { get; set; }
            public int BlockID { get; set; }

            public void Read(Wrapped wSock) {
                Location = new Position().Unpack(wSock.readLong());
                BlockID = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x23);
                wSock.writeLong(Location.Pack());
                wSock.writeVarInt(BlockID);
                wSock.Purge();
            }
        }

        public struct CBBlockAction : IPacket {
            public Position Location { get; set; }
            public byte Byte1 { get; set; }
            public byte Byte2 { get; set; }
            public int BlockType { get; set; }

            public void Read(Wrapped wSock) {
                Location = new Position().Unpack(wSock.readLong());
                Byte1 = wSock.readByte();
                Byte2 = wSock.readByte();
                BlockType = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x24);
                wSock.writeLong(Location.Pack());
                wSock.writeByte(Byte1);
                wSock.writeByte(Byte2);
                wSock.writeVarInt(BlockType);
                wSock.Purge();
            }
        }

        public struct CBBlockBreakAnimation : IPacket {
            public int EntityID { get; set; }
            public Position Location { get; set; }
            public sbyte DestroyStage { get; set; }

            public void Read(Wrapped wSock) {
                EntityID = wSock.readVarInt();
                Location = new Position().Unpack(wSock.readLong());
                DestroyStage = wSock.readSByte();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x25);
                wSock.writeVarInt(EntityID);
                wSock.writeLong(Location.Pack());
                wSock.writeSByte(DestroyStage);
                wSock.Purge();
            }
        }

        public struct CBMapChunkBulk : IPacket {
            public bool SkyLightSent { get; set; }
            public int ChunkColumnCount { get; set; }
            public byte[] ChunkMetadata { get; set; }

            public void Read(Wrapped wSock) {
                SkyLightSent = wSock.readBool();
                ChunkColumnCount = wSock.readVarInt();
                ChunkMetadata = wSock.readByteArray(10 * ChunkColumnCount);

            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x26);
                wSock.writeBool(SkyLightSent);
                wSock.writeVarInt(ChunkColumnCount);

                wSock.Purge();
            }
        }

        public struct CBExplosion : IPacket {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float Radius { get; set; }
            public int RecordCount { get; set; }
            public sbyte[] Records { get; set; }
            public float PlayerMotionX { get; set; }
            public float PlayerMotionY { get; set; }
            public float PlayerMotionZ { get; set; }

            public void Read(Wrapped wSock) {
                X = wSock.readFloat();
                Y = wSock.readFloat();
                Z = wSock.readFloat();
                Radius = wSock.readFloat();
                RecordCount = wSock.readInt();
                Records = new sbyte[RecordCount];
                for (int i = 0; i < RecordCount; i++) {
                    Records[i] = wSock.readSByte();
                }
                PlayerMotionX = wSock.readFloat();
                PlayerMotionY = wSock.readFloat();
                PlayerMotionZ = wSock.readFloat();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x27);
                wSock.writeFloat(X);
                wSock.writeFloat(Y);
                wSock.writeFloat(Z);
                wSock.writeFloat(Radius);
                wSock.writeInt(RecordCount);
                foreach (var record in Records) {
                    wSock.writeSByte(record);
                }
                wSock.writeFloat(PlayerMotionX);
                wSock.writeFloat(PlayerMotionY);
                wSock.writeFloat(PlayerMotionZ);
                wSock.Purge();
            }
        }

        public struct CBEffect : IPacket {
            public int EffectID { get; set; }
            public Position Location { get; set; }
            public int Data { get; set; }
            public bool DisableRelativeVolume { get; set; }

            public void Read(Wrapped wSock) {
                EffectID = wSock.readInt();
                Location = new Position().Unpack(wSock.readLong());
                Data = wSock.readInt();
                DisableRelativeVolume = wSock.readBool();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x28);
                wSock.writeInt(EffectID);
                wSock.writeLong(Location.Pack());
                wSock.writeInt(Data);
                wSock.writeBool(DisableRelativeVolume);
                wSock.Purge();
            }
        }

        public struct CBSoundEffect : IPacket {
            public string Soundname { get; set; }
            public int EffectPositionX { get; set; }
            public int EffectPositionY { get; set; }
            public int EffectPositionZ { get; set; }
            public float Volume { get; set; }
            public byte Pitch { get; set; }

            public void Read(Wrapped wSock) {
                Soundname = wSock.readString();
                EffectPositionX = wSock.readInt();
                EffectPositionY = wSock.readInt();
                EffectPositionZ = wSock.readInt();
                Volume = wSock.readFloat();
                Pitch = wSock.readByte();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x29);
                wSock.writeString(Soundname);
                wSock.writeInt(EffectPositionX);
                wSock.writeInt(EffectPositionY);
                wSock.writeInt(EffectPositionZ);
                wSock.writeFloat(Volume);
                wSock.writeByte(Pitch);
                wSock.Purge();
            }
        }

        public struct CBParticle : IPacket {
            public int ParticleId { get; set; }
            public bool LongDistance { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float OffsetX { get; set; }
            public float OffsetY { get; set; }
            public float OffsetZ { get; set; }
            public float ParticleData { get; set; }
            public int ParticleCount { get; set; }
            public int[] Data { get; set; }

            public void Read(Wrapped wSock) {
                ParticleId = wSock.readInt();
                LongDistance = wSock.readBool();
                X = wSock.readFloat();
                Y = wSock.readFloat();
                Z = wSock.readFloat();
                OffsetX = wSock.readFloat();
                OffsetY = wSock.readFloat();
                OffsetZ = wSock.readFloat();
                ParticleData = wSock.readFloat();
                ParticleCount = wSock.readInt();
                Data = new int[ParticleCount];

                for (int i = 0; i < ParticleCount; i++)
                    Data[i] = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2A);
                wSock.writeInt(ParticleCount);
                wSock.writeBool(LongDistance);
                wSock.writeFloat(X);
                wSock.writeFloat(Y);
                wSock.writeFloat(Z);
                wSock.writeFloat(OffsetX);
                wSock.writeFloat(OffsetY);
                wSock.writeFloat(OffsetZ);
                wSock.writeFloat(ParticleData);
                wSock.writeInt(ParticleCount);
                foreach (var i in Data)
                    wSock.writeVarInt(i);
                wSock.Purge();
            }
        }

        public struct CBChangeGameState : IPacket {
            public float Value { get; set; }

            public void Read(Wrapped wSock) {
                Value = wSock.readFloat();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2B);
                wSock.writeFloat(Value);
                wSock.Purge();
            }
        }

        public struct CBSpawnGlobalEntity : IPacket {
            public sbyte Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }

            public void Read(Wrapped wSock) {
                Type = wSock.readSByte();
                X = wSock.readInt();
                Y = wSock.readInt();
                Z = wSock.readInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2C);
                wSock.writeSByte(Type);
                wSock.writeInt(X);
                wSock.writeInt(Y);
                wSock.writeInt(Z);
                wSock.Purge();
            }
        }

        public struct CBOpenWindow : IPacket {
            public byte WindowId { get; set; }
            public string WindowType { get; set; }
            public string WindowTitle { get; set; }
            public byte NumberOfSlots { get; set; }
            public int EntityID { get; set; }

            public void Read(Wrapped wSock) {
                WindowId = wSock.readByte();
                WindowType = wSock.readString();
                WindowTitle = wSock.readString();
                NumberOfSlots = wSock.readByte();

                if (WindowType == "EntityHorse")
                    EntityID = wSock.readInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2D);
                wSock.writeVarInt(WindowId);
                wSock.writeString(WindowType);
                wSock.writeString(WindowTitle);
                wSock.writeByte(NumberOfSlots);

                if (WindowType == "EntityHorse")
                    wSock.writeInt(EntityID);

                wSock.Purge();
            }
        }

        public struct CBCloseWindow : IPacket {
            public byte WindowId { get; set; }

            public void Read(Wrapped wSock) {
                WindowId = wSock.readByte();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2E);
                wSock.writeByte(WindowId);
                wSock.Purge();
            }
        }

        public struct CBSetSlot : IPacket {
            public sbyte WindowId { get; set; }
            public short SlotId { get; set; }
            public SlotData Slot { get; set; }

            public void Read(Wrapped wSock) {
                WindowId = wSock.readSByte();
                SlotId = wSock.readShort();
                Slot = WrappedExtension.ReadSlot(wSock);
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x2F);
                wSock.writeSByte(WindowId);
                wSock.writeShort(SlotId);
                WrappedExtension.WriteSlot(wSock, Slot);
                wSock.Purge();
            }
        }

        public struct CBWindowItems : IPacket {
            public byte WindowID { get; set; }
            public short Count { get; set; }
            public SlotData[] Slots { get; set; }

            public void Read(Wrapped wSock) {
                WindowID = wSock.readByte();
                Count = wSock.readShort();

                for (int i = 0; i < Count; i++)
                    Slots[i] = WrappedExtension.ReadSlot(wSock);
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x30);
                wSock.writeByte(WindowID);
                wSock.writeShort(Count);
                foreach (var slotData in Slots)
                    WrappedExtension.WriteSlot(wSock, slotData);

                wSock.Purge();
            }
        }

        public struct CBWindowProperty : IPacket {
            public byte WindowID { get; set; }
            public short Property { get; set; }
            public short Value { get; set; }

            public void Read(Wrapped wSock) {
                WindowID = wSock.readByte();
                Property = wSock.readShort();
                Value = wSock.readShort();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x31);
                wSock.writeByte(WindowID);
                wSock.writeShort(Property);
                wSock.writeShort(Value);
                wSock.Purge();
            }
        }

        public struct CBConfirmTransaction : IPacket {
            public sbyte WindowID { get; set; }
            public short ActionNumber { get; set; }
            public bool Accepted { get; set; }

            public void Read(Wrapped wSock) {
                WindowID = wSock.readSByte();
                ActionNumber = wSock.readShort();
                Accepted = wSock.readBool();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x32);
                wSock.writeSByte(WindowID);
                wSock.writeShort(ActionNumber);
                wSock.writeBool(Accepted);
                wSock.Purge();
            }
        }

        public struct CBUpdateSign : IPacket {
            public Position Location { get; set; }
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string Line3 { get; set; }
            public string Line4 { get; set; }

            public void Read(Wrapped wSock) {
                Location = new Position().Unpack(wSock.readLong());
                Line1 = wSock.readString();
                Line2 = wSock.readString();
                Line3 = wSock.readString();
                Line4 = wSock.readString();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x33);
                wSock.writeLong(Location.Pack());
                wSock.writeString(Line1);
                wSock.writeString(Line2);
                wSock.writeString(Line3);
                wSock.writeString(Line4);
                wSock.Purge();
            }
        }

        public struct CBMaps : IPacket {
            public int ItemDamage { get; set; }
            public sbyte Scale { get; set; }
            public int IconCount { get; set; }
            public sbyte[] Icons { get; set; }
            public sbyte X { get; set; }
            public sbyte Z { get; set; }
            public sbyte Columns { get; set; }
            public sbyte Rows { get; set; }
            public int Length { get; set; }
            public byte[] Data { get; set; }

            public void Read(Wrapped wSock) {
                ItemDamage = wSock.readVarInt();
                Scale = wSock.readSByte();
                IconCount = wSock.readVarInt();

                for (int i = 0; i < IconCount; i++)
                    Icons[i] = wSock.readSByte();

                Columns = wSock.readSByte();

                if (Columns == 0)
                    return;

                Rows = wSock.readSByte();
                X = wSock.readSByte();
                Z = wSock.readSByte();
                Length = wSock.readVarInt();
                Data = wSock.readByteArray(Length);
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x34);
                wSock.writeVarInt(ItemDamage);
                wSock.writeSByte(Scale);
                wSock.writeVarInt(IconCount);
                wSock.write SByte(X);
                wSock.write SByte(Z);
                wSock.writeSByte(Columns);
                wSock.write SByte(Rows);
                wSock.write VarInt(Length);
                wSock.write Array of Byte(Data);
                wSock.Purge();
            }
        }

        public struct CBUpdateBlockEntity : IPacket {
            public Position Location { get; set; }
            public byte Action { get; set; }
            public <a href = "/nbt" title="nbt"> nbt tag</a> NBTData { get; set; }

            public void Read(Wrapped wSock) {
                Location = new Position().Unpack(wSock.readLong());
                Action = wSock.readByte();
                NBTData = wSock.read < a href = "/NBT" title = "NBT" > NBT Tag </ a > ();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x35);
                wSock.writeLong(Location.Pack());
                wSock.writeByte(Action);
                wSock.write < a href = "/NBT" title = "NBT" > NBT Tag </ a > (NBTData);
                wSock.Purge();
            }
        }

        public struct CBSignEditorOpen : IPacket {
            public Position Location { get; set; }

            public void Read(Wrapped wSock) {
                Location = new Position().Unpack(wSock.readLong());
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x36);
                wSock.writeLong(Location.Pack());
                wSock.Purge();
            }
        }

        public struct CBStatistics : IPacket {
            public int Count { get; set; }
            public name Statistic { get; set; }
            public int Value { get; set; }

            public void Read(Wrapped wSock) {
                Count = wSock.readVarInt();
                Statistic = wSock.readName();
                Value = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x37);
                wSock.writeVarInt(Count);
                wSock.writeName(Statistic);
                wSock.writeVarInt(Value);
                wSock.Purge();
            }
        }

        public struct CBPlayerListItem : IPacket {
            public int Action { get; set; }
            public int NumberOfPlayers { get; set; }
            public uuid Player { get; set; }

            public void Read(Wrapped wSock) {
                Action = wSock.readVarInt();
                NumberOfPlayers = wSock.readVarInt();
                Player = wSock.readUUID();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x38);
                wSock.writeVarInt(Action);
                wSock.writeVarInt(NumberOfPlayers);
                wSock.writeUUID(Player);
                wSock.Purge();
            }
        }

        public struct CBPlayerAbilities : IPacket {
            public sbyte Flags { get; set; }
            public float FlyingSpeed { get; set; }
            public float WalkingSpeed { get; set; }

            public void Read(Wrapped wSock) {
                Flags = wSock.readSByte();
                FlyingSpeed = wSock.readFloat();
                WalkingSpeed = wSock.readFloat();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x39);
                wSock.writeSByte(Flags);
                wSock.writeFloat(FlyingSpeed);
                wSock.writeFloat(WalkingSpeed);
                wSock.Purge();
            }
        }

        public struct CBTabComplete : IPacket {
            public array of string Matches { get; set; }

            public void Read(Wrapped wSock) {
                0x3A = wSock.readPlay();
                Matches = wSock.readArray of String();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3A);
                wSock.writePlay(0x3A);
                wSock.writeArray of String(Matches);
                wSock.Purge();
            }
        }

        public struct CBScoreboardObjective : IPacket {
            public sbyte Mode { get; set; }
            public string ObjectiveValue { get; set; }
            public string Type { get; set; }

            public void Read(Wrapped wSock) {
                0x3B = wSock.readPlay();
                Mode = wSock.readSByte();
                ObjectiveValue = wSock.read String();
                Type = wSock.read String();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3B);
                wSock.writePlay(0x3B);
                wSock.writeSByte(Mode);
                wSock.write String(ObjectiveValue);
                wSock.write String(Type);
                wSock.Purge();
            }
        }

        public struct CBUpdateScore : IPacket {
            public sbyte Action { get; set; }
            public string ObjectiveName { get; set; }
            public int Value { get; set; }

            public void Read(Wrapped wSock) {
                0x3C = wSock.readPlay();
                Action = wSock.readSByte();
                ObjectiveName = wSock.readString();
                Value = wSock.read VarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3C);
                wSock.writePlay(0x3C);
                wSock.writeSByte(Action);
                wSock.writeString(ObjectiveName);
                wSock.write VarInt(Value);
                wSock.Purge();
            }
        }

        public struct CBDisplayScoreboard : IPacket {
            public string ScoreName { get; set; }

            public void Read(Wrapped wSock) {
                0x3D = wSock.readPlay();
                ScoreName = wSock.readString();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3D);
                wSock.writePlay(0x3D);
                wSock.writeString(ScoreName);
                wSock.Purge();
            }
        }

        public struct CBTeams : IPacket {
            public sbyte Mode { get; set; }
            public string TeamDisplayName { get; set; }
            public string TeamPrefix { get; set; }
            public string TeamSuffix { get; set; }
            public sbyte FriendlyFire { get; set; }
            public string NameTagVisibility { get; set; }
            public sbyte Color { get; set; }
            public int PlayerCount { get; set; }
            public array of string Players { get; set; }

            public void Read(Wrapped wSock) {
                0x3E = wSock.readPlay();
                Mode = wSock.readSByte();
                TeamDisplayName = wSock.read String();
                TeamPrefix = wSock.read String();
                TeamSuffix = wSock.read String();
                FriendlyFire = wSock.read SByte();
                NameTagVisibility = wSock.read String();
                Color = wSock.read SByte();
                PlayerCount = wSock.read VarInt();
                Players = wSock.read Array of String();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3E);
                wSock.writePlay(0x3E);
                wSock.writeSByte(Mode);
                wSock.write String(TeamDisplayName);
                wSock.write String(TeamPrefix);
                wSock.write String(TeamSuffix);
                wSock.write SByte(FriendlyFire);
                wSock.write String(NameTagVisibility);
                wSock.write SByte(Color);
                wSock.write VarInt(PlayerCount);
                wSock.write Array of String(Players);
                wSock.Purge();
            }
        }

        public struct CBPluginMessage : IPacket {
            public byte[] Data { get; set; }

            public void Read(Wrapped wSock) {
                0x3F = wSock.readPlay();
                Data = wSock.readByteArray();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x3F);
                wSock.writePlay(0x3F);
                wSock.Send(Data);
                wSock.Purge();
            }
        }

        public struct CBDisconnect : IPacket {
            public string Reason { get; set; }

            public void Read(Wrapped wSock) {
                Reason = wSock.readString();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x40);
                wSock.writeString(Reason);
                wSock.Purge();
            }
        }

        public struct CBServerDifficulty : IPacket {
            public byte Difficulty { get; set; }

            public void Read(Wrapped wSock) {
                Difficulty = wSock.readByte();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x41);
                wSock.writeByte(Difficulty);
                wSock.Purge();
            }
        }

        public struct CBCombatEvent : IPacket {
            public int Event { get; set; }
            public int Duration { get; set; }
            public int PlayerID { get; set; }
            public int EntityID { get; set; }
            public string Message { get; set; }

            public void Read(Wrapped wSock) {
                Event = wSock.readVarInt();
                Duration = wSock.readVarInt();
                PlayerID = wSock.readVarInt();
                EntityID = wSock.readInt();
                Message = wSock.readString();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x42);
                wSock.writeVarInt(Event);
                wSock.writeVarInt(Duration);
                wSock.writeVarInt(PlayerID);
                wSock.writeVarInt(EntityID);
                wSock.writeString(Message);
                wSock.Purge();
            }
        }

        public struct CBCamera : IPacket {
            public int CameraID { get; set; }

            public void Read(Wrapped wSock) {
                CameraID = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x43);
                wSock.writeVarInt(CameraID);
                wSock.Purge();
            }
        }

        public struct CBWorldBorder : IPacket {
            public int Action { get; set; }

            public void Read(Wrapped wSock) {
                Action = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x44);
                wSock.writeVarInt(Action);
                wSock.Purge();
            }
        }

        public struct CBTitle : IPacket {
            public int Action { get; set; }

            public void Read(Wrapped wSock) {
                Action = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x45);
                wSock.writeVarInt(Action);
                wSock.Purge();
            }
        }

        public struct CBSetCompression : IPacket {
            public int Threshold { get; set; }

            public void Read(Wrapped wSock) {
                Threshold = wSock.readVarInt();
            }

            public void Write(Wrapped wSock) {
                wSock.writeVarInt(0x46);
                wSock.writeVarInt(Threshold);
                wSock.Purge();
            }
        }

        public struct CBPlayerListHeader/Footer : IPacket {
        public string Header { get; set; }
        public string Footer { get; set; }

        public void Read(Wrapped wSock) {
            Header = wSock.readString();
            Footer = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x47);
            wSock.writeString(Header);
            wSock.writeString(Footer);
            wSock.Purge();
        }
    }

    public struct CBResourcePackSend : IPacket {
        public string URL { get; set; }
        public string Hash { get; set; }

        public void Read(Wrapped wSock) {
            URL = wSock.readString();
            Hash = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x48);
            wSock.writeString(URL);
            wSock.writeString(Hash);
            wSock.Purge();
        }
    }

    public struct CBUpdateEntityNBT : IPacket {
        public int EntityID { get; set; }
        public <a href = "/nbt" title="nbt">nbt tag</a> Tag { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Tag = wSock.read < a href = "/NBT" title = "NBT" > NBT Tag </ a > ();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x49);
            wSock.writeVarInt(EntityID);
            wSock.write < a href = "/NBT" title = "NBT" > NBT Tag </ a > (Tag);
            wSock.Purge();
        }
    }
#endregion

