using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Status {
    public class ServerPing : Packet {
        public int msPing; // -- Ping in milliseconds

        public ServerPing(ref Minecraft mc) {
            long time = mc.nh.wSock.readLong();
            msPing = (int)(DateTime.Now.Ticks - time) / 10000; // -- 10,000 ticks per millisecond.

            mc.RaisePingMs(msPing);
            mc.nh.RaiseSocketInfo(this, "Server Ping Completed.");
        }
    }
}
