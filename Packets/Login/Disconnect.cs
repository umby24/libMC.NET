using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Login {
    class Disconnect : Packet {
        public Disconnect(ref Minecraft mc) {
            string reason = mc.nh.wSock.readString();

            mc.raiseLoginFailure(this, reason);
            mc.Disconnect();
        }
    }
}
