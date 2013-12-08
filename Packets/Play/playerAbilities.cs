using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class playerAbilities : Packet {
        byte flags;
        float flyingSpeed;
        float walkingSpeed;

        public playerAbilities(ref Minecraft mc) {
            flags = mc.nh.wSock.readByte();
            flyingSpeed = mc.nh.wSock.readFloat();
            walkingSpeed = mc.nh.wSock.readFloat();

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.flyingSpeed = flyingSpeed;
            mc.ThisPlayer.WalkingSpeed = walkingSpeed;

        }
    }
}
