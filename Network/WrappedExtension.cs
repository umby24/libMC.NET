using System;
using System.Collections.Generic;
using System.Linq;
using libMC.NET.Common;
using CWrapped;

namespace libMC.NET.Network {
    public struct SlotData {
        public short Id {get; set;}
        public byte ItemCount { get; set; }
        public short ItemDamage { get; set; }
        public byte[] NbtData { get; set; }
    }

    public struct Position {
        public int X;
        public short Y;
        public int Z;

        public Position Unpack(long value) { // -- TODO: 
            return this;
        }

        public long Pack() {
            return 0L;
        }
    }

    public struct ObjectMetadata {
        public int ObjectId;
        public short SpeedX { get; set; }
        public short SpeedY { get; set; }
        public short SpeedZ { get; set; }
    }

    public struct PropertyData {
        public string Key { get; set; }
        public double Value { get; set; }
        public ModifierData[] Modifiers { get; set; }
    }

    public struct ModifierData {
        public Guid Uuid { get; set; }
        public double Amount { get; set; }
        public byte Operation { get; set; }
    }

    public struct Record {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }
        public int BlockId { get; set; }
    }

    public struct PlayerData {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Signature { get; set; }
    }

    /// <summary>
    /// Class for handling all of the additional retarded "Data Types" That Mojang decided to create.
    /// </summary>
    public class WrappedExtension {
        public static object[] ReadEntityMetadata(Wrapped wSock) {
            var metadataDict = new Dictionary<int, object>();

            do {
                var item = wSock.readByte();

                if (item == 127)
                    break;

                var index = item & 0x1F;
                var type = item >> 5;

                switch (type) {
                    case 0:
                        metadataDict.Add(index, wSock.readByte());
                        break;
                    case 1:
                        metadataDict.Add(index, wSock.readShort());
                        break;
                    case 2:
                        metadataDict.Add(index, wSock.readInt());
                        break;
                    case 3:
                        metadataDict.Add(index, wSock.readFloat());
                        break;
                    case 4:
                        metadataDict.Add(index, wSock.readString());
                        break;
                    case 5:
                        metadataDict.Add(index, ReadSlot(wSock));
                        break;
                    case 6:
                        metadataDict.Add(index, new Vector(wSock.readInt(), wSock.readInt(), wSock.readInt()));
                        break;
                }
            } while (true);

            metadataDict.OrderBy(o => o.Key); // -- Order the dictionary based on the index values
            return metadataDict.Values.ToArray(); // -- Return the data as an array.
        }

        public static void WriteEntityMetadata(Wrapped wSock, object[] metadata) {
            throw new NotImplementedException("Not quite done here yet.");
        }

        public static ObjectMetadata ReadObjectMetadata(Wrapped wSock) {
            var data = new ObjectMetadata {ObjectId = wSock.readInt()};

            if (data.ObjectId != 0) {
                data.SpeedX = wSock.readShort();
                data.SpeedY = wSock.readShort();
                data.SpeedZ = wSock.readShort();
            }

            return data;
        }

        public static void WriteObjectMetadata(Wrapped wSock, ObjectMetadata data) {
            wSock.writeInt(data.ObjectId);
            
            if (data.ObjectId != 0) {
                wSock.writeShort(data.SpeedX);
                wSock.writeShort(data.SpeedY);
                wSock.writeShort(data.SpeedZ);
            }
        }

        public static PropertyData ReadPropertyData(Wrapped wSock) {
            var data = new PropertyData
            {
                Key = wSock.readString(),
                Value = wSock.readDouble()
            };

            var items = wSock.readVarInt();

            data.Modifiers = new ModifierData[items];

            for (var x = 0; x < items; x++) 
                data.Modifiers[x] = ReadModifierData(wSock);
            
            return data;
        }

        public static void WritePropertyData(Wrapped wSock, PropertyData data) {
            wSock.writeString(data.Key);
            wSock.writeDouble(data.Value);
            wSock.writeVarInt(data.Modifiers.Length);

            foreach (var modifierData in data.Modifiers)
                WriteModifierData(wSock, modifierData);
        }

        public static ModifierData ReadModifierData(Wrapped wSock) {
            var data = new ModifierData
            {
                Uuid = new Guid(wSock.readByteArray(16)),
                Amount = wSock.readDouble(),
                Operation = wSock.readByte()
            };

            return data;
        }

        public static void WriteModifierData(Wrapped wSock, ModifierData data) {
            wSock.Send(data.Uuid.ToByteArray());
            wSock.writeDouble(data.Amount);
            wSock.writeByte(data.Operation);
        }
        
        public static SlotData ReadSlot(Wrapped wSock) {
            var data = new SlotData {Id = wSock.readShort()};

            if (data.Id == -1) {
                data.Id = 0;
                data.ItemCount = 0;
                data.ItemDamage = 0;
                return data;
            }

            data.ItemCount = wSock.readByte();
            data.ItemDamage = wSock.readShort();
            var nbtLength = wSock.readShort();

            if (nbtLength == -1)
                return data;

            data.NbtData = wSock.readByteArray(nbtLength);
            return data;
        }

        public static void WriteSlot(Wrapped wSock, SlotData data) {
            if (data.Id == -1) {
                wSock.writeShort(-1);
                return;
            }

            wSock.writeShort(data.Id);
            wSock.writeByte(data.ItemCount);
            wSock.writeShort(data.ItemDamage);

            if (data.NbtData == null) {
                wSock.writeShort(-1);
                return;
            }

            wSock.writeShort((short)data.NbtData.Length);
            wSock.Send(data.NbtData);
        }

        public static Record ReadRecord(Wrapped wSock) {
            var data = new Record();

            var horizional = wSock.readByte();
            data.X = (byte)(horizional & 0xF0);
            data.Z = (byte) (horizional & 0x0F);

            data.Y = wSock.readByte();
            data.BlockId = wSock.readVarInt();

            return data;
        }

        public static void WriteRecord(Wrapped wSock, Record data) {
            wSock.writeByte((byte)(data.X << 4 | data.Z));
            wSock.writeByte(data.Y);
            wSock.writeVarInt(data.BlockId);
        }
    }
}
