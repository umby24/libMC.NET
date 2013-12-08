using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class windowItems : Packet {
        public byte windowID;
        public short count;
        Item[] items;

        public windowItems(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            count = mc.nh.wSock.readShort();

            items = new Item[count];

            for (int i = 0; i < count; i++) {
                items[i] = new Item();
                items[i].readSlot(ref mc);

                if (windowID == 0)
                    mc.ThisPlayer.setInventory(items[i], (short) i);
            }
        }
    }
}
