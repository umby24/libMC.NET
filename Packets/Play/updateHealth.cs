using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class updateHealth : Packet {
        public updateHealth(ref Minecraft mc) {
            float health = mc.nh.wSock.readFloat();
            short hunger = mc.nh.wSock.readShort();
            float saturation = mc.nh.wSock.readFloat();

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.playerHealth = health;
            mc.ThisPlayer.playerHunger = hunger;
            mc.ThisPlayer.foodSaturation = saturation;

            mc.raisePlayerHealthUpdate(health, hunger, saturation);
            mc.RaiseDebug(this, "Player health updated.");
        }
    }
}
