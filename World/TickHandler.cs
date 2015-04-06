using libMC.NET.Client;
using libMC.NET.Network;

namespace libMC.NET.MinecraftWorld {
    public class TickHandler {
        public MinecraftClient ThisMc;

        public TickHandler(ref MinecraftClient mc) {
            ThisMc = mc;
        }
        public void DoTick() {
            var player = new SBPlayer();
            player.OnGround = ThisMc.ThisPlayer.OnGround;
            player.Write(ThisMc.Nh.WSock);
        }
    }
}
