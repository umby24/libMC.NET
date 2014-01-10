using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class SetSlot : Packet {
        byte windowID;
        short slot;
        Item slotData;

        public SetSlot(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            slot = mc.nh.wSock.readShort();

            slotData = new Item();
            slotData.ReadSlot(ref mc);

            if (windowID == 0) {
                mc.ThisPlayer.SetInventory(slotData, slot);
                mc.RaiseInventoryItem(slot, slotData);
            } else
                mc.RaiseSetWindowSlot(windowID, slot, slotData);

            
        }
    }
}
