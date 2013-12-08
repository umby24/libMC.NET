using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class timeUpdate : Packet {

        public timeUpdate(ref Minecraft mc) {
            long worldAge = mc.nh.wSock.readLong();
            long worldTime = mc.nh.wSock.readLong();

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new Classes.World();

            mc.minecraftWorld.worldAge = worldAge;
            mc.minecraftWorld.currentTime = worldTime;

            var Player = new ServerBound.Player(ref mc);

            if (mc.nh.worldTick == null)
                mc.nh.worldTick = new MinecraftWorld.TickHandler(ref mc);

            mc.RaiseDebug(this, "World time updated");
        }
    }
}
