using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Classes {
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
    }
}
