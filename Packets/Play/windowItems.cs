using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class WindowItems : Packet {
        public byte windowID;
        public short count;
        Item[] items;

        public WindowItems(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            count = mc.nh.wSock.readShort();

            items = new Item[count];

            for (int i = 0; i < count; i++) {
                items[i] = new Item();
                items[i].ReadSlot(ref mc);

                if (windowID == 0)
                    mc.ThisPlayer.SetInventory(items[i], (short) i);
            }
        }
    }
}
