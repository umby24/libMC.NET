using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class setExperience : Packet {
        public setExperience(ref Minecraft mc) {
            float ExpBar = mc.nh.wSock.readFloat();
            short level = mc.nh.wSock.readShort();
            short totalExp = mc.nh.wSock.readShort();

            if (mc.thisPlayer == null)
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.ExpBar = ExpBar;
            mc.thisPlayer.level = level;
            mc.thisPlayer.totalExp = totalExp;

            mc.raiseExperienceUpdate(ExpBar, level, totalExp);
        }
    }
}
