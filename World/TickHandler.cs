using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using libMC.NET.Client;
using libMC.NET.Network;

namespace libMC.NET.MinecraftWorld {
    public class TickHandler {
        public MinecraftClient ThisMc;

        public TickHandler(ref MinecraftClient mc) {
            ThisMc = mc;
        }
        public void DoTick() {
            var Player = new SBPlayer();
            Player.OnGround = ThisMc.ThisPlayer.onGround;
            Player.Write(ThisMc.nh.wSock);
        }
    }
}
