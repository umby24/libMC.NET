using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class heldItemChange : Packet {
        public byte slot;

        public heldItemChange(ref Minecraft mc) {
            slot = mc.nh.wSock.readByte();

            if (mc.thisPlayer == null)
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.selectedSlot = slot;
            ServerBound.HeldItemChange hic = new ServerBound.HeldItemChange(ref mc);

            mc.raiseHeldSlotChanged(slot);
        }
    }
}
