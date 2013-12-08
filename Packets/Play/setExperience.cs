using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class setExperience : Packet {
        public setExperience(ref Minecraft mc) {
            float ExpBar = mc.nh.wSock.readFloat();
            short level = mc.nh.wSock.readShort();
            short totalExp = mc.nh.wSock.readShort();

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.ExpBar = ExpBar;
            mc.ThisPlayer.level = level;
            mc.ThisPlayer.totalExp = totalExp;

            mc.raiseExperienceUpdate(ExpBar, level, totalExp);
        }
    }
}
