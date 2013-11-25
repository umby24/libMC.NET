using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class scoreboardObjective : Packet {
        public string objectiveName, objectiveValue;
        public byte create;

        public scoreboardObjective(ref Minecraft mc) {
            objectiveName = mc.nh.wSock.readString();
            objectiveValue = mc.nh.wSock.readString();
            create = mc.nh.wSock.readByte();

            if (create == 0) 
                mc.raiseScoreboardAdd(objectiveName, objectiveValue);
             else if (create == 1) 
                mc.raiseScoreboardUpdate(objectiveName, objectiveValue);
             else if (create == 2) 
                mc.raiseScoreboardRemove(objectiveName);
            
        }
    }
}
