using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class closeWindow : Packet {
        public byte windowID;

        public closeWindow(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();

            mc.RaiseDebug(this, "Window forcably closed");
            mc.raiseWindowClosed(windowID);
        }
    }
}
