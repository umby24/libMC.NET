using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play.ServerBound {
    public class PlayerPosition {
        public PlayerPosition(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(4);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.x);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.y - 1.620);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.y);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.z);
            mc.nh.wSock.writeBool(mc.ThisPlayer.onGround);
            mc.nh.wSock.Purge();
        }
    }
}
