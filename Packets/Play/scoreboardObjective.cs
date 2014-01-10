using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class ScoreboardObjective : Packet {
        public string objectiveName, objectiveValue;
        public byte create;

        public ScoreboardObjective(ref Minecraft mc) {
            objectiveName = mc.nh.wSock.readString();
            objectiveValue = mc.nh.wSock.readString();
            create = mc.nh.wSock.readByte();

            if (create == 0) 
                mc.RaiseScoreboardAdd(objectiveName, objectiveValue);
             else if (create == 1) 
                mc.RaiseScoreboardUpdate(objectiveName, objectiveValue);
             else if (create == 2) 
                mc.RaiseScoreboardRemove(objectiveName);
            
        }
    }
}
