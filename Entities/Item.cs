using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Entities {
    public class Item {
        public int itemID;
        public byte itemCount;
        public short itemDamage;
        public byte[] nbtData;

        public void readSlot(ref Minecraft mc) {
            int blockID = mc.nh.wSock.readShort();

            if (blockID == -1) {
                itemID = 0;
                itemCount = 0;
                itemDamage = 0;
                return;
            }

            itemCount = mc.nh.wSock.readByte();
            itemDamage = mc.nh.wSock.readShort();
            int NBTLength = mc.nh.wSock.readShort();

            if (NBTLength == -1) {
                return;
            }

            nbtData = mc.nh.wSock.readByteArray(NBTLength);

            return;
        }
        public string friendlyName() {
            // -- Return the friendly name for the item we represent

            return ((Block.blockitemid)itemID).ToString();
        }

        public static void writeSlot(ref Minecraft mc, Item item) {
            if (item == null) {
                mc.nh.wSock.writeShort(-1);
                return;
            }

            mc.nh.wSock.writeShort((short)item.itemID);
            mc.nh.wSock.writeByte(item.itemCount);
            mc.nh.wSock.writeShort(item.itemDamage);
            mc.nh.wSock.writeShort(-1);
        }
    }
}
