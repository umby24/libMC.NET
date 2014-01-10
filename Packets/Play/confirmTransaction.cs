using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class ConfirmTransaction : Packet {
        public byte windowID;
        public short actionID;
        public bool accepted;

        public ConfirmTransaction(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            actionID = mc.nh.wSock.readShort();
            accepted = mc.nh.wSock.readBool();

            if (accepted == false)
                mc.RaiseTransactionRejected(windowID, actionID);
            else
                mc.RaiseTransactionAccepted(windowID, actionID);
        }
    }
}
