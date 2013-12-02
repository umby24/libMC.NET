using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class updateHealth : Packet {
        public updateHealth(ref Minecraft mc) {
            float health = mc.nh.wSock.readFloat();
            short hunger = mc.nh.wSock.readShort();
            float saturation = mc.nh.wSock.readFloat();

            if (mc.thisPlayer == null)
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.playerHealth = health;
            mc.thisPlayer.playerHunger = hunger;
            mc.thisPlayer.foodSaturation = saturation;

            mc.raisePlayerHealthUpdate(health, hunger, saturation);
            mc.raiseDebug(this, "Player health updated.");
        }
    }
}
