using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Packets.Play {
    class SignEditorOpen : Packet {
        Vector Location;

        public SignEditorOpen(ref Minecraft mc) {
            Location = new Vector(mc.nh.wSock.readInt(), mc.nh.wSock.readInt(), mc.nh.wSock.readInt());
        }
    }
}
