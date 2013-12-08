using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class blockAction : Packet {
        public int x, z, type;
        public short y; // -- This being a short is a waste of a byte..
        public byte byte1, byte2;

        public blockAction(ref Minecraft mc) {
            x = mc.nh.wSock.readInt();
            y = mc.nh.wSock.readShort();
            z = mc.nh.wSock.readInt();
            byte1 = mc.nh.wSock.readByte();
            byte2 = mc.nh.wSock.readByte();
            type = mc.nh.wSock.readVarInt();

            handleAction(ref mc);
        }

        void handleAction(ref Minecraft mc) {
            switch (type) {
                case 25: // -- Note block
                    mc.raiseNoteBlockSound(byte1, byte2, x, y, z);
                    break;
                case 29: // -- Sticky piston
                    mc.raisePistonMoved(byte1, byte2, x, y, z);
                    break;
                case 33: // -- Piston
                    mc.raisePistonMoved(byte1, byte2, x, y, z);
                    break;
                case 54: // -- Chest
                    mc.raiseChestStateChange(byte2, x, y, z);
                    break;
                case 146: // -- Trapped chest
                    mc.raiseChestStateChange(byte2, x, y, z);
                    break;
                default:
                    mc.RaiseError(this, "Unknown block action received.");
                    mc.RaiseDebug(this, "Error info: Block ID: " + type);
                    break;
            }
        }
    }
}
