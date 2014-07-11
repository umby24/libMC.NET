using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CWrapped;
using libMC.NET.Entities;

namespace libMC.NET.Network {
    public interface IPacket {
        void Read(Wrapped wSock);
        void Write(Wrapped wSock);
    }

    // -- Status 0: Handshake
    public struct SBHandshake : IPacket {
        public int ProtocolVersion { get; set; }
        public string ServerAddress { get; set; }
        public short ServerPort { get; set; }
        public int NextState { get; set; }

        public void Read(Wrapped wSock) {
            ProtocolVersion = wSock.readVarInt();
            ServerAddress = wSock.readString();
            ServerPort = wSock.readShort();
            NextState = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeVarInt(ProtocolVersion);
            wSock.writeString(ServerAddress);
            wSock.writeShort(ServerPort);
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
        public short SharedLength { get; set; }
        public byte[] SharedSecret { get; set; }
        public short VerifyLength { get; set; }
        public byte[] VerifyToken { get; set; }

        public void Read(Wrapped wSock) {
            SharedLength = wSock.readShort();
            SharedSecret = wSock.readByteArray(SharedLength);
            VerifyLength = wSock.readShort();
            VerifyToken = wSock.readByteArray(VerifyLength);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeShort(SharedLength);
            wSock.Send(SharedSecret);
            wSock.writeShort(VerifyLength);
            wSock.Send(VerifyToken);
            wSock.Purge();
        }
    }

    public struct CBLoginDisconnect : IPacket {
        public string JSONData { get; set; }

        public void Read(Wrapped wSock) {
            JSONData = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeString(JSONData);
            wSock.Purge();
        }
    }

    public struct CBEncryptionRequest : IPacket {
        public string ServerID { get; set; }
        public short PublicLength { get; set; }
        public byte[] PublicKey { get; set; }
        public short VerifyLength { get; set; }
        public byte[] VerifyToken { get; set; }

        public void Read(Wrapped wSock) {
            ServerID = wSock.readString();
            PublicLength = wSock.readShort();
            PublicKey = wSock.readByteArray(PublicLength);
            VerifyLength = wSock.readShort();
            VerifyToken = wSock.readByteArray(VerifyLength);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeString(ServerID);
            wSock.writeShort(PublicLength);
            wSock.Send(PublicKey);
            wSock.writeShort(VerifyLength);
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

    public struct CBPing : IPacket {
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
    public struct SBKeepAlive : IPacket {
        public int KeepAliveID { get; set; }

        public void Read(Wrapped wSock) {
            KeepAliveID = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeInt(KeepAliveID);
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
        public int Target { get; set; }
        public sbyte Mouse { get; set; }

        public void Read(Wrapped wSock) {
            Target = wSock.readInt();
            Mouse = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x02);
            wSock.writeInt(Target);
            wSock.writeSByte(Mouse);
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
        public double HeadY { get; set; }
        public double Z { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            FeetY = wSock.readDouble();
            HeadY = wSock.readDouble();
            Z = wSock.readDouble();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x04);
            wSock.writeDouble(X);
            wSock.writeDouble(FeetY);
            wSock.writeDouble(HeadY);
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
        public double HeadY { get; set; }
        public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            FeetY = wSock.readDouble();
            HeadY = wSock.readDouble();
            Z = wSock.readDouble();
            Yaw = wSock.readFloat();
            Pitch = wSock.readFloat();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x06);
            wSock.writeDouble(X);
            wSock.writeDouble(FeetY);
            wSock.writeDouble(HeadY);
            wSock.writeDouble(Z);
            wSock.writeFloat(Yaw);
            wSock.writeFloat(Pitch);
            wSock.writeBool(OnGround);
            wSock.Purge();
        }
    }

    public struct SBPlayerDigging : IPacket {
        public sbyte Status { get; set; }
        public int X { get; set; }
        public byte Y { get; set; }
        public int Z { get; set; }
        public sbyte Face { get; set; }

        public void Read(Wrapped wSock) {
            Status = wSock.readSByte();
            X = wSock.readInt();
            Y = wSock.readByte();
            Z = wSock.readInt();
            Face = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x07);
            wSock.writeSByte(Status);
            wSock.writeInt(X);
            wSock.writeByte(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Face);
            wSock.Purge();
        }
    }

    public struct SBPlayerBlockPlacement : IPacket {
        public int X { get; set; }
        public byte Y { get; set; }
        public int Z { get; set; }
        public sbyte Direction { get; set; }
        public SlotData Helditem { get; set; }
        public sbyte CursorpositionX { get; set; }
        public sbyte CursorpositionY { get; set; }
        public sbyte CursorpositionZ { get; set; }
        public short Slot { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readByte();
            Z = wSock.readInt();
            Direction = wSock.readSByte();
            Helditem = WrappedExtension.ReadSlot(wSock);
            CursorpositionX = wSock.readSByte();
            CursorpositionY = wSock.readSByte();
            CursorpositionZ = wSock.readSByte();
            Slot = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x08);
            wSock.writeInt(X);
            wSock.writeByte(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Direction);
            WrappedExtension.WriteSlot(wSock, Helditem);
            wSock.writeSByte(CursorpositionX);
            wSock.writeSByte(CursorpositionY);
            wSock.writeSByte(CursorpositionZ);
            wSock.writeShort(Slot);
            wSock.Purge();
        }
    }

    public struct SBHeldItemChange : IPacket {
        public short Slot { get; set; }

        public void Read(Wrapped wSock) {
            Slot = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x09);
            wSock.writeShort(Slot);
            wSock.Purge();
        }
    }
    public struct SBAnimation : IPacket {
        public int EntityID { get; set; }
        public sbyte Animation { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Animation = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0A);
            wSock.writeInt(EntityID);
            wSock.writeSByte(Animation);
            wSock.Purge();
        }
    }

    public struct SBEntityAction : IPacket {
        public int EntityID { get; set; }
        public sbyte ActionID { get; set; }
        public int JumpBoost { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            ActionID = wSock.readSByte();
            JumpBoost = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0B);
            wSock.writeInt(EntityID);
            wSock.writeSByte(ActionID);
            wSock.writeInt(JumpBoost);
            wSock.Purge();
        }
    }

    public struct SBSteerVehicle : IPacket {
        public float Sideways { get; set; }
        public float Forward { get; set; }
        public bool Jump { get; set; }
        public bool Unmount { get; set; }

        public void Read(Wrapped wSock) {
            Sideways = wSock.readFloat();
            Forward = wSock.readFloat();
            Jump = wSock.readBool();
            Unmount = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0C);
            wSock.writeFloat(Sideways);
            wSock.writeFloat(Forward);
            wSock.writeBool(Jump);
            wSock.writeBool(Unmount);
            wSock.Purge();
        }
    }

    public struct SBCloseWindow : IPacket {
        public sbyte Windowid { get; set; }

        public void Read(Wrapped wSock) {
            Windowid = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0D);
            wSock.writeSByte(Windowid);
            wSock.Purge();
        }
    }

    public struct SBClickWindow : IPacket {
        public sbyte WindowID { get; set; }
        public short Slot { get; set; }
        public sbyte Button { get; set; }
        public short Actionnumber { get; set; }
        public sbyte Mode { get; set; }
        public SlotData Clickeditem { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readSByte();
            Slot = wSock.readShort();
            Button = wSock.readSByte();
            Actionnumber = wSock.readShort();
            Mode = wSock.readSByte();
            Clickeditem = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0E);
            wSock.writeSByte(WindowID);
            wSock.writeShort(Slot);
            wSock.writeSByte(Button);
            wSock.writeShort(Actionnumber);
            wSock.writeSByte(Mode);
            WrappedExtension.WriteSlot(wSock, Clickeditem);
            wSock.Purge();
        }
    }

    public struct SBConfirmTransaction : IPacket {
        public sbyte WindowID { get; set; }
        public short Actionnumber { get; set; }
        public bool Accepted { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readSByte();
            Actionnumber = wSock.readShort();
            Accepted = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0F);
            wSock.writeSByte(WindowID);
            wSock.writeShort(Actionnumber);
            wSock.writeBool(Accepted);
            wSock.Purge();
        }
    }

    public struct SBCreativeInventoryAction : IPacket {
        public short Slot { get; set; }
        public SlotData Clickeditem { get; set; }

        public void Read(Wrapped wSock) {
            Slot = wSock.readShort();
            Clickeditem = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x10);
            wSock.writeShort(Slot);
            WrappedExtension.WriteSlot(wSock, Clickeditem);
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
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readShort();
            Z = wSock.readInt();
            Line1 = wSock.readString();
            Line2 = wSock.readString();
            Line3 = wSock.readString();
            Line4 = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x12);
            wSock.writeInt(X);
            wSock.writeShort(Y);
            wSock.writeInt(Z);
            wSock.writeString(Line1);
            wSock.writeString(Line2);
            wSock.writeString(Line3);
            wSock.writeString(Line4);
            wSock.Purge();
        }
    }

    public struct SBPlayerAbilities : IPacket {
        public sbyte Flags { get; set; }

        public void Read(Wrapped wSock) {
            Flags = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x13);
            wSock.writeSByte(Flags);
            wSock.Purge();
        }
    }

    public struct SBTabComplete : IPacket {
        public string Text { get; set; }

        public void Read(Wrapped wSock) {
            Text = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x14);
            wSock.writeString(Text);
            wSock.Purge();
        }
    }

    public struct SBClientSettings : IPacket {
        public string Locale { get; set; }
        public sbyte Viewdistance { get; set; }
        public sbyte Chatflags { get; set; }
        public bool Chatcolours { get; set; }
        public sbyte Difficulty { get; set; }
        public bool ShowCape { get; set; }

        public void Read(Wrapped wSock) {
            Locale = wSock.readString();
            Viewdistance = wSock.readSByte();
            Chatflags = wSock.readSByte();
            Chatcolours = wSock.readBool();
            Difficulty = wSock.readSByte();
            ShowCape = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x15);
            wSock.writeString(Locale);
            wSock.writeSByte(Viewdistance);
            wSock.writeSByte(Chatflags);
            wSock.writeBool(Chatcolours);
            wSock.writeSByte(Difficulty);
            wSock.writeBool(ShowCape);
            wSock.Purge();
        }
    }

    public struct SBClientStatus : IPacket {
        public sbyte ActionID { get; set; }

        public void Read(Wrapped wSock) {
            ActionID = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x16);
            wSock.writeSByte(ActionID);
            wSock.Purge();
        }
    }

    public struct SBPluginMessage : IPacket {
        public string Channel { get; set; }
        public short Length { get; set; }
        public byte[] Data { get; set; }

        public void Read(Wrapped wSock) {
            Channel = wSock.readString();
            Length = wSock.readShort();
            Data = wSock.readByteArray(Length);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x17);
            wSock.writeString(Channel);
            wSock.writeShort(Length);
            wSock.Send(Data);
            wSock.Purge();
        }
    }

    public struct CBKeepAlive : IPacket {
        public int KeepAliveID { get; set; }

        public void Read(Wrapped wSock) {
            KeepAliveID = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x00);
            wSock.writeInt(KeepAliveID);
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

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Gamemode = wSock.readByte();
            Dimension = wSock.readSByte();
            Difficulty = wSock.readByte();
            MaxPlayers = wSock.readByte();
            LevelType = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x01);
            wSock.writeInt(EntityID);
            wSock.writeByte(Gamemode);
            wSock.writeSByte(Dimension);
            wSock.writeByte(Difficulty);
            wSock.writeByte(MaxPlayers);
            wSock.writeString(LevelType);
            wSock.Purge();
        }
    }

    public struct CBChatMessage : IPacket {
        public string JSONData { get; set; }

        public void Read(Wrapped wSock) {
            JSONData = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x02);
            wSock.writeString(JSONData);
            wSock.Purge();
        }
    }

    public struct CBTimeUpdate : IPacket {
        public long Ageoftheworld { get; set; }
        public long Timeofday { get; set; }

        public void Read(Wrapped wSock) {
            Ageoftheworld = wSock.readLong();
            Timeofday = wSock.readLong();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x03);
            wSock.writeLong(Ageoftheworld);
            wSock.writeLong(Timeofday);
            wSock.Purge();
        }
    }

    public struct CBEntityEquipment : IPacket {
        public int EntityID { get; set; }
        public short Slot { get; set; }
        public SlotData Item { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Slot = wSock.readShort();
            Item = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x04);
            wSock.writeInt(EntityID);
            wSock.writeShort(Slot);
            WrappedExtension.WriteSlot(wSock, Item);
            wSock.Purge();
        }
    }

    public struct CBSpawnPosition : IPacket {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x05);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.Purge();
        }
    }

    public struct CBUpdateHealth : IPacket {
        public float Health { get; set; }
        public short Food { get; set; }
        public float FoodSaturation { get; set; }

        public void Read(Wrapped wSock) {
            Health = wSock.readFloat();
            Food = wSock.readShort();
            FoodSaturation = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x06);
            wSock.writeFloat(Health);
            wSock.writeShort(Food);
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
            Dimension = wSock.readInt(); // -- Only possible values for this are -1, 0, and 1.. No reason for it to be an int and waste 4 bytes. Thanks mojang.
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
        public bool OnGround { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readDouble();
            Y = wSock.readDouble();
            Z = wSock.readDouble();
            Yaw = wSock.readFloat();
            Pitch = wSock.readFloat();
            OnGround = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x08);
            wSock.writeDouble(X);
            wSock.writeDouble(Y);
            wSock.writeDouble(Z);
            wSock.writeFloat(Yaw);
            wSock.writeFloat(Pitch);
            wSock.writeBool(OnGround);
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
        public int EntityID { get; set; }
        public int X { get; set; }
        public byte Y { get; set; }
        public int Z { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            X = wSock.readInt();
            Y = wSock.readByte();
            Z = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0A);
            wSock.writeInt(EntityID);
            wSock.writeInt(X);
            wSock.writeByte(Y);
            wSock.writeInt(Z);
            wSock.Purge();
        }
    }

    public struct CBAnimation : IPacket {
        public int EntityID { get; set; }
        public byte Animation { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Animation = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0B);
            wSock.writeVarInt(EntityID);
            wSock.writeByte(Animation);
            wSock.Purge();
        }
    }

    public struct CBSpawnPlayer : IPacket {
        public int EntityID { get; set; }
        public string PlayerUUID { get; set; }
        public string PlayerName { get; set; }
        public int Datacount { get; set; }
        public PlayerData[] Data;
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }
        public short CurrentItem { get; set; }
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            PlayerUUID = wSock.readString();
            PlayerName = wSock.readString();
            Datacount = wSock.readVarInt();
            Data = new PlayerData[Datacount];

            for (int i = 0; i < Datacount; i++) {
                Data[i].Name = wSock.readString();
                Data[i].Value = wSock.readString();
                Data[i].Signature = wSock.readString();
            }

            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Yaw = wSock.readSByte();
            Pitch = wSock.readSByte();
            CurrentItem = wSock.readShort();
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0C);
            wSock.writeVarInt(EntityID);
            wSock.writeString(PlayerUUID);
            wSock.writeString(PlayerName);
            wSock.writeVarInt(Data.Length);

            for (int i = 0; i < Data.Length; i++) {
                wSock.writeString(Data[i].Name);
                wSock.writeString(Data[i].Value);
                wSock.writeString(Data[i].Signature);
            }

            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Yaw);
            wSock.writeSByte(Pitch);
            wSock.writeShort(CurrentItem);
            WrappedExtension.WriteEntityMetadata(wSock, Metadata);
            wSock.Purge();
        }
    }

    public struct CBCollectItem : IPacket {
        public int CollectedEntityID { get; set; }
        public int CollectorEntityID { get; set; }

        public void Read(Wrapped wSock) {
            CollectedEntityID = wSock.readInt();
            CollectorEntityID = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0D);
            wSock.writeInt(CollectedEntityID);
            wSock.writeInt(CollectorEntityID);
            wSock.Purge();
        }
    }

    public struct CBSpawnObject : IPacket {
        public int EntityID { get; set; }
        public sbyte Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public sbyte Pitch { get; set; }
        public sbyte Yaw { get; set; }
        public ObjectMetadata Data { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Type = wSock.readSByte();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Pitch = wSock.readSByte();
            Yaw = wSock.readSByte();
            Data = WrappedExtension.ReadObjectMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0E);
            wSock.writeVarInt(EntityID);
            wSock.writeSByte(Type);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Pitch);
            wSock.writeSByte(Yaw);
            WrappedExtension.WriteObjectMetadata(wSock, Data);
            wSock.Purge();
        }
    }

    public struct CBSpawnMob : IPacket {
        public int EntityID { get; set; }
        public byte Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public sbyte Pitch { get; set; }
        public sbyte HeadPitch { get; set; }
        public sbyte Yaw { get; set; }
        public short VelocityX { get; set; }
        public short VelocityY { get; set; }
        public short VelocityZ { get; set; }
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Type = wSock.readByte();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Pitch = wSock.readSByte();
            HeadPitch = wSock.readSByte();
            Yaw = wSock.readSByte();
            VelocityX = wSock.readShort();
            VelocityY = wSock.readShort();
            VelocityZ = wSock.readShort();
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x0F);
            wSock.writeVarInt(EntityID);
            wSock.writeByte(Type);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Pitch);
            wSock.writeSByte(HeadPitch);
            wSock.writeSByte(Yaw);
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
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Direction { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Title = wSock.readString();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Direction = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x10);
            wSock.writeVarInt(EntityID);
            wSock.writeString(Title);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeInt(Direction);
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
            EntityID = wSock.readInt();
            VelocityX = wSock.readShort();
            VelocityY = wSock.readShort();
            VelocityZ = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x12);
            wSock.writeInt(EntityID);
            wSock.writeShort(VelocityX);
            wSock.writeShort(VelocityY);
            wSock.writeShort(VelocityZ);
            wSock.Purge();
        }
    }

    public struct CBDestroyEntities : IPacket {
        public sbyte Count { get; set; }
        public int[] EntityIDs { get; set; }

        public void Read(Wrapped wSock) {
            Count = wSock.readSByte();
            EntityIDs = new int[Count];

            for (int i = 0; i < Count; i++)
                EntityIDs[i] = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x13);
            wSock.writeSByte(Count);

            for (int i = 0; i < Count; i++)
                wSock.writeInt(EntityIDs[i]);

            wSock.Purge();
        }
    }

    public struct CBEntity : IPacket {
        public int EntityID { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x14);
            wSock.writeInt(EntityID);
            wSock.Purge();
        }
    }

    public struct CBEntityRelativeMove : IPacket {
        public int EntityID { get; set; }
        public sbyte DX { get; set; }
        public sbyte DY { get; set; }
        public sbyte DZ { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            DX = wSock.readSByte();
            DY = wSock.readSByte();
            DZ = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x15);
            wSock.writeInt(EntityID);
            wSock.writeSByte(DX);
            wSock.writeSByte(DY);
            wSock.writeSByte(DZ);
            wSock.Purge();
        }
    }

    public struct CBEntityLook : IPacket {
        public int EntityID { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Yaw = wSock.readSByte();
            Pitch = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x16);
            wSock.writeInt(EntityID);
            wSock.writeSByte(Yaw);
            wSock.writeSByte(Pitch);
            wSock.Purge();
        }
    }

    public struct CBEntityLookandRelativeMove : IPacket {
        public int EntityID { get; set; }
        public sbyte DX { get; set; }
        public sbyte DY { get; set; }
        public sbyte DZ { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            DX = wSock.readSByte();
            DY = wSock.readSByte();
            DZ = wSock.readSByte();
            Yaw = wSock.readSByte();
            Pitch = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x17);
            wSock.writeInt(EntityID);
            wSock.writeSByte(DX);
            wSock.writeSByte(DY);
            wSock.writeSByte(DZ);
            wSock.writeSByte(Yaw);
            wSock.writeSByte(Pitch);
            wSock.Purge();
        }
    }

    public struct CBEntityTeleport : IPacket {
        public int EntityID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public sbyte Yaw { get; set; }
        public sbyte Pitch { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Yaw = wSock.readSByte();
            Pitch = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x18);
            wSock.writeInt(EntityID);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Yaw);
            wSock.writeSByte(Pitch);
            wSock.Purge();
        }
    }

    public struct CBEntityHeadLook : IPacket {
        public int EntityID { get; set; }
        public sbyte HeadYaw { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            HeadYaw = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x19);
            wSock.writeInt(EntityID);
            wSock.writeSByte(HeadYaw);
            wSock.Purge();
        }
    }

    public struct CBEntityStatus : IPacket {
        public int EntityID { get; set; }
        public sbyte EntityStatus { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            EntityStatus = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1A);
            wSock.writeInt(EntityID);
            wSock.writeSByte(EntityStatus);
            wSock.Purge();
        }
    }

    public struct CBAttachEntity : IPacket {
        public int EntityID { get; set; }
        public int VehicleID { get; set; }
        public bool Leash { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            VehicleID = wSock.readInt();
            Leash = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1B);
            wSock.writeInt(EntityID);
            wSock.writeInt(VehicleID);
            wSock.writeBool(Leash);
            wSock.Purge();
        }
    }

    public struct CBEntityMetadata : IPacket {
        public int EntityID { get; set; }
        public object[] Metadata { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            Metadata = WrappedExtension.ReadEntityMetadata(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1C);
            wSock.writeInt(EntityID);
            WrappedExtension.WriteEntityMetadata(wSock, Metadata);
            wSock.Purge();
        }
    }

    public struct CBEntityEffect : IPacket {
        public int EntityID { get; set; }
        public sbyte EffectID { get; set; }
        public sbyte Amplifier { get; set; }
        public short Duration { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            EffectID = wSock.readSByte();
            Amplifier = wSock.readSByte();
            Duration = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1D);
            wSock.writeInt(EntityID);
            wSock.writeSByte(EffectID);
            wSock.writeSByte(Amplifier);
            wSock.writeShort(Duration);
            wSock.Purge();
        }
    }

    public struct CBRemoveEntityEffect : IPacket {
        public int EntityID { get; set; }
        public sbyte EffectID { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readInt();
            EffectID = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1E);
            wSock.writeInt(EntityID);
            wSock.writeSByte(EffectID);
            wSock.Purge();
        }
    }

    public struct CBSetExperience : IPacket {
        public float Experiencebar { get; set; }
        public short Level { get; set; }
        public short TotalExperience { get; set; }

        public void Read(Wrapped wSock) {
            Experiencebar = wSock.readFloat();
            Level = wSock.readShort();
            TotalExperience = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x1F);
            wSock.writeFloat(Experiencebar);
            wSock.writeShort(Level);
            wSock.writeShort(TotalExperience);
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

            for (int x = 0; x < Count; x++ )
                Properties[x] = WrappedExtension.ReadPropertyData(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x20);
            wSock.writeInt(EntityID);
            wSock.writeInt(Count);

            for (int x = 0; x < Count; x++)
                WrappedExtension.WritePropertyData(wSock, Properties[x]);

            wSock.Purge();
        }
    }

    public struct CBChunkData : IPacket {
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }
        public bool GroundUpcontinuous { get; set; }
        public ushort Primarybitmap { get; set; }
        public ushort Addbitmap { get; set; }
        public int Compressedsize { get; set; }
        public byte[] Compresseddata { get; set; }

        public void Read(Wrapped wSock) {
            ChunkX = wSock.readInt();
            ChunkZ = wSock.readInt();
            GroundUpcontinuous = wSock.readBool();
            Primarybitmap = (ushort)wSock.readShort();
            Addbitmap = (ushort)wSock.readShort();
            Compressedsize = wSock.readInt();
            Compresseddata = wSock.readByteArray(Compressedsize);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x21);
            wSock.writeInt(ChunkX);
            wSock.writeInt(ChunkZ);
            wSock.writeBool(GroundUpcontinuous);
            wSock.writeShort((short)Primarybitmap);
            wSock.writeShort((short)Addbitmap);
            wSock.writeInt(Compressedsize);
            wSock.Send(Compresseddata);
            wSock.Purge();
        }
    }

    public struct CBMultiBlockChange : IPacket {
        public int ChunkX { get; set; }
        public int ChunkZ { get; set; }
        public short Recordcount { get; set; }
        public int Datasize { get; set; }
        public Record[] Records { get; set; }

        public void Read(Wrapped wSock) {
            ChunkX = wSock.readInt();
            ChunkZ = wSock.readInt();
            Recordcount = wSock.readShort();
            Datasize = wSock.readInt();
            Records = new Record[Recordcount];

            for (int x = 0; x < Recordcount; x++)
                Records[x] = WrappedExtension.ReadRecord(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x22);
            wSock.writeInt(ChunkX);
            wSock.writeInt(ChunkZ);
            wSock.writeShort(Recordcount);
            wSock.writeInt(Datasize);

            for (int x = 0; x < Recordcount; x++)
                WrappedExtension.WriteRecord(wSock, Records[x]);

            wSock.Purge();
        }
    }

    public struct CBBlockChange : IPacket {
        public int X { get; set; }
        public byte Y { get; set; }
        public int Z { get; set; }
        public int BlockID { get; set; }
        public byte BlockMetadata { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readByte();
            Z = wSock.readInt();
            BlockID = wSock.readVarInt();
            BlockMetadata = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x23);
            wSock.writeInt(X);
            wSock.writeByte(Y);
            wSock.writeInt(Z);
            wSock.writeVarInt(BlockID);
            wSock.writeByte(BlockMetadata);
            wSock.Purge();
        }
    }

    public struct CBBlockAction : IPacket {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public byte Byte1 { get; set; }
        public byte Byte2 { get; set; }
        public int BlockType { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readShort();
            Z = wSock.readInt();
            Byte1 = wSock.readByte();
            Byte2 = wSock.readByte();
            BlockType = wSock.readVarInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x24);
            wSock.writeInt(X);
            wSock.writeShort(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Byte1);
            wSock.writeByte(Byte2);
            wSock.writeVarInt(BlockType);
            wSock.Purge();
        }
    }

    public struct CBBlockBreakAnimation : IPacket {
        public int EntityID { get; set; }
        public int X { get; set; }
        public int Y { get; set; } // -- This shouldn't be an int..
        public int Z { get; set; }
        public sbyte Stage { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
            Stage = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x25);
            wSock.writeVarInt(EntityID);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.writeSByte(Stage);
            wSock.Purge();
        }
    }

    public struct CBMapChunkBulk : IPacket {
        public short Chunkcolumncount { get; set; }
        public int Datalength { get; set; }
        public bool Skylightsent { get; set; }
        public byte[] Data { get; set; }
        public byte[] Metainformation { get; set; }

        public void Read(Wrapped wSock) {
            Chunkcolumncount = wSock.readShort();
            Datalength = wSock.readInt();
            Skylightsent = wSock.readBool();
            Data = wSock.readByteArray(Datalength);
            Metainformation = wSock.readByteArray(Chunkcolumncount * 12);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x26);
            wSock.writeShort(Chunkcolumncount);
            wSock.writeInt(Datalength);
            wSock.writeBool(Skylightsent);
            wSock.Send(Data);
            wSock.Send(Metainformation);
            wSock.Purge();
        }
    }

    public struct CBExplosion : IPacket {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Radius { get; set; }
        public int Recordcount { get; set; }
        public sbyte[] Records { get; set; }
        public float PlayerMotionX { get; set; }
        public float PlayerMotionY { get; set; }
        public float PlayerMotionZ { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readFloat();
            Y = wSock.readFloat();
            Z = wSock.readFloat();
            Radius = wSock.readFloat();
            Recordcount = wSock.readInt();
            Records = new sbyte[Recordcount * 3];

            for (int x = 0; x < Records.Length - 1; x++) {
                Records[x] = wSock.readSByte();
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
            wSock.writeInt(Recordcount);

            for (int x = 0; x < Records.Length - 1; x++) {
                wSock.writeSByte(Records[x]);
            }

            wSock.writeFloat(PlayerMotionX);
            wSock.writeFloat(PlayerMotionY);
            wSock.writeFloat(PlayerMotionZ);
            wSock.Purge();
        }
    }

    public struct CBEffect : IPacket {
        public int EffectID { get; set; }
        public int X { get; set; }
        public sbyte Y { get; set; }
        public int Z { get; set; }
        public int Data { get; set; }
        public bool Disablerelativevolume { get; set; }

        public void Read(Wrapped wSock) {
            EffectID = wSock.readInt();
            X = wSock.readInt();
            Y = wSock.readSByte();
            Z = wSock.readInt();
            Data = wSock.readInt();
            Disablerelativevolume = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x28);
            wSock.writeInt(EffectID);
            wSock.writeInt(X);
            wSock.writeSByte(Y);
            wSock.writeInt(Z);
            wSock.writeInt(Data);
            wSock.writeBool(Disablerelativevolume);
            wSock.Purge();
        }
    }

    public struct CBSoundEffect : IPacket {
        public string SoundName { get; set; }
        public int EffectpositionX { get; set; }
        public int EffectpositionY { get; set; }
        public int EffectpositionZ { get; set; }
        public float Volume { get; set; }
        public byte Pitch { get; set; }

        public void Read(Wrapped wSock) {
            SoundName = wSock.readString();
            EffectpositionX = wSock.readInt();
            EffectpositionY = wSock.readInt();
            EffectpositionZ = wSock.readInt();
            Volume = wSock.readFloat();
            Pitch = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x29);
            wSock.writeString(SoundName);
            wSock.writeInt(EffectpositionX);
            wSock.writeInt(EffectpositionY);
            wSock.writeInt(EffectpositionZ);
            wSock.writeFloat(Volume);
            wSock.writeByte(Pitch);
            wSock.Purge();
        }
    }

    public struct CBParticle : IPacket {
        public string Particlename { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float OffsetZ { get; set; }
        public float Particledata { get; set; }
        public int Numberofparticles { get; set; }

        public void Read(Wrapped wSock) {
            Particlename = wSock.readString();
            X = wSock.readFloat();
            Y = wSock.readFloat();
            Z = wSock.readFloat();
            OffsetX = wSock.readFloat();
            OffsetY = wSock.readFloat();
            OffsetZ = wSock.readFloat();
            Particledata = wSock.readFloat();
            Numberofparticles = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2A);
            wSock.writeString(Particlename);
            wSock.writeFloat(X);
            wSock.writeFloat(Y);
            wSock.writeFloat(Z);
            wSock.writeFloat(OffsetX);
            wSock.writeFloat(OffsetY);
            wSock.writeFloat(OffsetZ);
            wSock.writeFloat(Particledata);
            wSock.writeInt(Numberofparticles);
            wSock.Purge();
        }
    }

    public struct CBChangeGameState : IPacket {
        public byte Reason { get; set; }
        public float Value { get; set; }

        public void Read(Wrapped wSock) {
            Reason = wSock.readByte();
            Value = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2B);
            wSock.writeByte(Reason);
            wSock.writeFloat(Value);
            wSock.Purge();
        }
    }

    public struct CBSpawnGlobalEntity : IPacket {
        public int EntityID { get; set; }
        public sbyte Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public void Read(Wrapped wSock) {
            EntityID = wSock.readVarInt();
            Type = wSock.readSByte();
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2C);
            wSock.writeVarInt(EntityID);
            wSock.writeSByte(Type);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.Purge();
        }
    }

    public struct CBOpenWindow : IPacket {
        public byte Windowid { get; set; }
        public byte InventoryType { get; set; }
        public string Windowtitle { get; set; }
        public byte NumberofSlots { get; set; }
        public bool Useprovidedwindowtitle { get; set; }
        public int EntityID { get; set; }

        public void Read(Wrapped wSock) {
            Windowid = wSock.readByte();
            InventoryType = wSock.readByte();
            Windowtitle = wSock.readString();
            NumberofSlots = wSock.readByte();
            Useprovidedwindowtitle = wSock.readBool();
            EntityID = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2D);
            wSock.writeByte(Windowid);
            wSock.writeByte(InventoryType);
            wSock.writeString(Windowtitle);
            wSock.writeByte(NumberofSlots);
            wSock.writeBool(Useprovidedwindowtitle);
            wSock.writeInt(EntityID);
            wSock.Purge();
        }
    }

    public struct CBCloseWindow : IPacket {
        public byte WindowID { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2E);
            wSock.writeByte(WindowID);
            wSock.Purge();
        }
    }

    public struct CBSetSlot : IPacket {
        public sbyte WindowID { get; set; }
        public short Slot { get; set; }
        public SlotData Slotdata { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readSByte();
            Slot = wSock.readShort();
            Slotdata = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x2F);
            wSock.writeSByte(WindowID);
            wSock.writeShort(Slot);
            WrappedExtension.WriteSlot(wSock, Slotdata);
            wSock.Purge();
        }
    }

    public struct CBWindowItems : IPacket {
        public byte WindowID { get; set; }
        public short Count { get; set; }
        public SlotData[] Slotdata { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readByte();
            Count = wSock.readShort();
            Slotdata = new SlotData[Count];

            for (int x = 0; x < Count; x++)
                Slotdata[x] = WrappedExtension.ReadSlot(wSock);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x30);
            wSock.writeByte(WindowID);
            wSock.writeShort(Count);

            for (int x = 0; x < Count; x++)
                WrappedExtension.WriteSlot(wSock, Slotdata[x]);

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
        public byte WindowID { get; set; }
        public short Actionnumber { get; set; }
        public bool Accepted { get; set; }

        public void Read(Wrapped wSock) {
            WindowID = wSock.readByte();
            Actionnumber = wSock.readShort();
            Accepted = wSock.readBool();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x32);
            wSock.writeByte(WindowID);
            wSock.writeShort(Actionnumber);
            wSock.writeBool(Accepted);
            wSock.Purge();
        }
    }

    public struct CBUpdateSign : IPacket {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readShort();
            Z = wSock.readInt();
            Line1 = wSock.readString();
            Line2 = wSock.readString();
            Line3 = wSock.readString();
            Line4 = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x33);
            wSock.writeInt(X);
            wSock.writeShort(Y);
            wSock.writeInt(Z);
            wSock.writeString(Line1);
            wSock.writeString(Line2);
            wSock.writeString(Line3);
            wSock.writeString(Line4);
            wSock.Purge();
        }
    }

    public struct CBMaps : IPacket {
        public int ItemDamage { get; set; }
        public short Length { get; set; }
        public byte[] Data { get; set; }

        public void Read(Wrapped wSock) {
            ItemDamage = wSock.readVarInt();
            Length = wSock.readShort();
            Data = wSock.readByteArray(Length);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x34);
            wSock.writeVarInt(ItemDamage);
            wSock.writeShort(Length);
            wSock.Send(Data);
            wSock.Purge();
        }
    }

    public struct CBUpdateBlockEntity : IPacket {
        public int X { get; set; }
        public short Y { get; set; }
        public int Z { get; set; }
        public byte Action { get; set; }
        public short Datalength { get; set; }
        public byte[] NBTData { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readShort();
            Z = wSock.readInt();
            Action = wSock.readByte();
            Datalength = wSock.readShort();
            NBTData = wSock.readByteArray(Datalength);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x35);
            wSock.writeInt(X);
            wSock.writeShort(Y);
            wSock.writeInt(Z);
            wSock.writeByte(Action);
            wSock.writeShort(Datalength);
            wSock.Send(NBTData);
            wSock.Purge();
        }
    }

    public struct CBSignEditorOpen : IPacket {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public void Read(Wrapped wSock) {
            X = wSock.readInt();
            Y = wSock.readInt();
            Z = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x36);
            wSock.writeInt(X);
            wSock.writeInt(Y);
            wSock.writeInt(Z);
            wSock.Purge();
        }
    }

    public struct CBStatistics : IPacket {
        public int Count { get; set; }
        public Dictionary<string, int> Entries { get; set; }
        public string Statisticsname { get; set; }
        public int Value { get; set; }
        public string Playername { get; set; }
        public bool Online { get; set; }
        public short Ping { get; set; }

        public void Read(Wrapped wSock) {
            Entries = new Dictionary<string, int>();
            Count = wSock.readVarInt();

            for (int i = 0; i < Count; i++) {
                string name = wSock.readString();
                int value = wSock.readVarInt();

                Entries.Add(name, value);
            }
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x37);
            wSock.writeVarInt(Count);

            foreach (string s in Entries.Keys) {
                wSock.writeString(s);
                wSock.writeVarInt(Entries[s]);
            }

            wSock.Purge();
        }
    }

    public struct CBPlayerListItem : IPacket {
        public string Playername { get; set; }
        public bool Online { get; set; }
        public short Ping { get; set; }

        public void Read(Wrapped wSock) {
            Playername = wSock.readString();
            Online = wSock.readBool();
            Ping = wSock.readShort();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x38);
            wSock.writeString(Playername);
            wSock.writeBool(Online);
            wSock.writeShort(Ping);
            wSock.Purge();
        }
    }

    public struct CBPlayerAbilities : IPacket {
        public sbyte Flags { get; set; }
        public float Flyingspeed { get; set; }
        public float Walkingspeed { get; set; }

        public void Read(Wrapped wSock) {
            Flags = wSock.readSByte();
            Flyingspeed = wSock.readFloat();
            Walkingspeed = wSock.readFloat();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x39);
            wSock.writeSByte(Flags);
            wSock.writeFloat(Flyingspeed);
            wSock.writeFloat(Walkingspeed);
            wSock.Purge();
        }
    }

    public struct CBTabComplete : IPacket {
        public int Count { get; set; }
        public string[] Matches { get; set; }

        public void Read(Wrapped wSock) {
            Count = wSock.readVarInt();
            Matches = new string[Count];

            for (int i = 0; i < Count; i++)
                Matches[i] = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3A);
            wSock.writeVarInt(Count);

            for (int i = 0; i < Count; i++)
                wSock.writeString(Matches[i]);

            wSock.Purge();
        }
    }

    public struct CBScoreboardObjective : IPacket {
        public string Objectivename { get; set; }
        public string Objectivevalue { get; set; }
        public sbyte Create { get; set; }

        public void Read(Wrapped wSock) {
            Objectivename = wSock.readString();
            Objectivevalue = wSock.readString();
            Create = wSock.readSByte();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3B);
            wSock.writeString(Objectivename);
            wSock.writeString(Objectivevalue);
            wSock.writeSByte(Create);
            wSock.Purge();
        }
    }

    public struct CBUpdateScore : IPacket {
        public string ItemName { get; set; }
        public sbyte Update { get; set; }
        public string ScoreName { get; set; }
        public int Value { get; set; }

        public void Read(Wrapped wSock) {
            ItemName = wSock.readString();
            Update = wSock.readSByte();
            ScoreName = wSock.readString();
            Value = wSock.readInt();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3C);
            wSock.writeString(ItemName);
            wSock.writeSByte(Update);
            wSock.writeString(ScoreName);
            wSock.writeInt(Value);
            wSock.Purge();
        }
    }

    public struct CBDisplayScoreboard : IPacket {
        public sbyte Position { get; set; }
        public string ScoreName { get; set; }

        public void Read(Wrapped wSock) {
            Position = wSock.readSByte();
            ScoreName = wSock.readString();
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3D);
            wSock.writeSByte(Position);
            wSock.writeString(ScoreName);
            wSock.Purge();
        }
    }

    public struct CBTeams : IPacket {
        public string TeamName { get; set; }
        public sbyte Mode { get; set; }
        public string TeamDisplayName { get; set; }
        public string TeamPrefix { get; set; }
        public string TeamSuffix { get; set; }
        public sbyte Friendlyfire { get; set; }
        public short Playercount { get; set; }
        public string[] Players { get; set; }

        public void Read(Wrapped wSock) {
            TeamName = wSock.readString();
            Mode = wSock.readSByte();

            switch (Mode) {
                case 0:
                    TeamDisplayName = wSock.readString();
                    TeamPrefix = wSock.readString();
                    TeamSuffix = wSock.readString();
                    Friendlyfire = wSock.readSByte();
                    Playercount = wSock.readShort();

                    for (int x = 0; x < Playercount - 1; x++)
                        Players[x] = wSock.readString();
                    break;
                case 2:
                    TeamDisplayName = wSock.readString();
                    TeamPrefix = wSock.readString();
                    TeamSuffix = wSock.readString();
                    Friendlyfire = wSock.readSByte();
                    break;
                case 3:
                    Playercount = wSock.readShort();

                    for (int x = 0; x < Playercount - 1; x++)
                        Players[x] = wSock.readString();
                    break;
                case 4:
                    Playercount = wSock.readShort();

                    for (int x = 0; x < Playercount - 1; x++)
                        Players[x] = wSock.readString();
                    break;
            }
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3E);
            wSock.writeString(TeamName);
            wSock.writeSByte(Mode);
            wSock.writeString(TeamDisplayName);
            wSock.writeString(TeamPrefix);
            wSock.writeString(TeamSuffix);
            wSock.writeSByte(Friendlyfire);
            wSock.writeShort(Playercount);

            for (int x = 0; x < Playercount - 1; x++)
                wSock.writeString(Players[x]);

            wSock.Purge();
        }
    }

    public struct CBPluginMessage : IPacket {
        public string Channel { get; set; }
        public short Length { get; set; }
        public byte[] Data { get; set; }

        public void Read(Wrapped wSock) {
            Channel = wSock.readString();
            Length = wSock.readShort();
            Data = wSock.readByteArray(Length);
        }

        public void Write(Wrapped wSock) {
            wSock.writeVarInt(0x3F);
            wSock.writeString(Channel);
            wSock.writeShort(Length);
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
}
