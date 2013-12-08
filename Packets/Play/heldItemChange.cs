using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class heldItemChange : Packet {
        public byte slot;

        public heldItemChange(ref Minecraft mc) {
            slot = mc.nh.wSock.readByte();

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.selectedSlot = slot;
            ServerBound.HeldItemChange hic = new ServerBound.HeldItemChange(ref mc);

            mc.raiseHeldSlotChanged(slot);
        }
    }
}
