using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class setSlot : Packet {
        byte windowID;
        short slot;
        Item slotData;

        public setSlot(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            slot = mc.nh.wSock.readShort();

            slotData = new Item();
            slotData.readSlot(ref mc);

            if (windowID == 0) {
                mc.ThisPlayer.setInventory(slotData, slot);
                mc.raiseInventoryItem(slot, slotData);
            } else
                mc.raiseSetWindowSlot(windowID, slot, slotData);

            
        }
    }
}
