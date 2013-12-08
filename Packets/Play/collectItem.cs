using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class collectItem : Packet {
        public int collectedEID, collectorEID;

        public collectItem(ref Minecraft mc) {
            collectedEID = mc.nh.wSock.readInt();
            collectorEID = mc.nh.wSock.readInt();

            mc.RaiseDebug(this, "Item collected by " + collectorEID);
            mc.raiseItemCollected(collectedEID, collectorEID);
        }
    }
}
