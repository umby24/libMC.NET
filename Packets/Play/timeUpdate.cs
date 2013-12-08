using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class timeUpdate : Packet {

        public timeUpdate(ref Minecraft mc) {
            long worldAge = mc.nh.wSock.readLong();
            long worldTime = mc.nh.wSock.readLong();

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            mc.MinecraftWorld.worldAge = worldAge;
            mc.MinecraftWorld.currentTime = worldTime;

            var Player = new ServerBound.Player(ref mc);

            if (mc.nh.worldTick == null)
                mc.nh.worldTick = new MinecraftWorld.TickHandler(ref mc);

            mc.RaiseDebug(this, "World time updated");
        }
    }
}
