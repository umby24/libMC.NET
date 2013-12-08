using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class entityEquipment : Packet {
        public int Entity_ID;
        public short slot;
        public Item slotItem;

        public entityEquipment(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readInt();
            slot = mc.nh.wSock.readShort();

            slotItem = new Item();
            slotItem.readSlot(ref mc); // -- read the slot data.

            if (mc.ThisPlayer != null && Entity_ID == mc.ThisPlayer.Entity_ID) {
                mc.ThisPlayer.setInventory(slotItem, slot);
            }

            if (mc.MinecraftWorld != null && mc.MinecraftWorld.Entities != null) {
                int eIndex = mc.MinecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1)
                    mc.MinecraftWorld.Entities[eIndex].handleInventory(slot, slotItem);
            }
            

            mc.RaiseDebug(this, String.Format("Entity Equipment update.\n EID: {0}\nSlot: {1}\nFriendly name: {2}", Entity_ID, slot.ToString(), slotItem.friendlyName()));
        }
    }
}
