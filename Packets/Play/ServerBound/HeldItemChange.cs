using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class HeldItemChange {
        public HeldItemChange(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(9);
            mc.nh.wSock.writeShort((short)mc.thisPlayer.selectedSlot);
            mc.nh.wSock.Purge();
        }
    }
}
