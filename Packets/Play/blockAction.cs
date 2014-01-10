using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class BlockAction : Packet {
        public int x, z, type;
        public short y; // -- This being a short is a waste of a byte..
        public byte byte1, byte2;

        public BlockAction(ref Minecraft mc) {
            x = mc.nh.wSock.readInt();
            y = mc.nh.wSock.readShort();
            z = mc.nh.wSock.readInt();
            byte1 = mc.nh.wSock.readByte();
            byte2 = mc.nh.wSock.readByte();
            type = mc.nh.wSock.readVarInt();

            HandleAction(ref mc);
        }

        void HandleAction(ref Minecraft mc) {
            switch (type) {
                case 25: // -- Note block
                    mc.RaiseNoteBlockSound(byte1, byte2, x, y, z);
                    break;
                case 29: // -- Sticky piston
                    mc.RaisePistonMoved(byte1, byte2, x, y, z);
                    break;
                case 33: // -- Piston
                    mc.RaisePistonMoved(byte1, byte2, x, y, z);
                    break;
                case 54: // -- Chest
                    mc.RaiseChestStateChange(byte2, x, y, z);
                    break;
                case 146: // -- Trapped chest
                    mc.RaiseChestStateChange(byte2, x, y, z);
                    break;
                default:
                    mc.RaiseError(this, "Unknown block action received.");
                    mc.RaiseDebug(this, "Error info: Block ID: " + type);
                    break;
            }
        }
    }
}
