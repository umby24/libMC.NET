using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class DisplayScoreboard : Packet {
        public byte position;
        public string scoreName;

        public DisplayScoreboard(ref Minecraft mc) {
            position = mc.nh.wSock.readByte();
            scoreName = mc.nh.wSock.readString();

            mc.RaiseScoreBoard(position, scoreName);
        }
    }
}
