using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class displayScoreboard : Packet {
        public byte position;
        public string scoreName;

        public displayScoreboard(ref Minecraft mc) {
            position = mc.nh.wSock.readByte();
            scoreName = mc.nh.wSock.readString();

            mc.raiseScoreBoard(position, scoreName);
        }
    }
}
