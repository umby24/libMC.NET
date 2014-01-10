using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class CloseWindow : Packet {
        public byte windowID;

        public CloseWindow(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();

            mc.RaiseDebug(this, "Window forcably closed");
            mc.RaiseWindowClosed(windowID);
        }
    }
}
