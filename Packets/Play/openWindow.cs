using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class openWindow : Packet {
        byte windowID, inventoryType, slots;
        string windowTitle;
        bool useTitle;
        int EntityID; // -- Only if type == 11 (Horse)

        public openWindow(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            inventoryType = mc.nh.wSock.readByte();
            windowTitle = mc.nh.wSock.readString();
            slots = mc.nh.wSock.readByte();
            useTitle = mc.nh.wSock.readBool();

            if (inventoryType == 11)
                EntityID = mc.nh.wSock.readInt();

            mc.raiseOpenWindow(windowID, inventoryType, windowTitle, slots, useTitle);
            mc.raiseDebug(this, "Window opened forcably");
        }
    }
}
