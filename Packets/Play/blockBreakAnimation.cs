using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Packets.Play {
    class BlockBreakAnimation : Packet {
        public int Entity_ID;
        public Vector Location;
        public byte stage;

        public BlockBreakAnimation(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readVarInt();
            Location = new Vector(mc.nh.wSock.readInt(), mc.nh.wSock.readInt(), mc.nh.wSock.readInt());
            stage = mc.nh.wSock.readByte();

            mc.RaiseBlockBreakingEvent(Location, Entity_ID, stage);
        }
    }
}
