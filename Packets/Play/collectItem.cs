using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class CollectItem : Packet {
        public int collectedEID, collectorEID;

        public CollectItem(ref Minecraft mc) {
            collectedEID = mc.nh.wSock.readInt();
            collectorEID = mc.nh.wSock.readInt();

            mc.RaiseDebug(this, "Item collected by " + collectorEID);
            mc.RaiseItemCollected(collectedEID, collectorEID);
        }
    }
}
