using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class playerAbilities : Packet {
        byte flags;
        float flyingSpeed;
        float walkingSpeed;

        public playerAbilities(ref Minecraft mc) {
            flags = mc.nh.wSock.readByte();
            flyingSpeed = mc.nh.wSock.readFloat();
            walkingSpeed = mc.nh.wSock.readFloat();

            if (mc.thisPlayer == null)
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.flyingSpeed = flyingSpeed;
            mc.thisPlayer.WalkingSpeed = walkingSpeed;

        }
    }
}
