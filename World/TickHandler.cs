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
            Player.OnGround = ThisMc.ThisPlayer.OnGround;
            Player.Write(ThisMc.Nh.wSock);
        }
    }
}
