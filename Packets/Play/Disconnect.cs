using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Disconnect : Packet {
        public string reason;

        public Disconnect(ref Minecraft mc) {
            reason = mc.nh.wSock.readString();

            mc.raiseKicked(reason);
            mc.Disconnect();
        }
    }
}
