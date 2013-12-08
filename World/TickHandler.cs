using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using libMC.NET.Packets.Play.ServerBound;

namespace libMC.NET.MinecraftWorld {
    public class TickHandler {
        public Minecraft ThisMc;

        public TickHandler(ref Minecraft mc) {
            ThisMc = mc;
        }
        public void DoTick() {
            var Player = new Player(ref ThisMc); // -- Send a player packet.
        }
    }
}
