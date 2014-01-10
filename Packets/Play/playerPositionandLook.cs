using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libMC.NET.Entities;
namespace libMC.NET.Packets.Play {
    class PlayerPositionandLook : Packet {
        public double x, y, z;
        public float yaw, pitch;
        public bool onGround;

        public PlayerPositionandLook(ref Minecraft mc) {
            x = mc.nh.wSock.readDouble();
            y = mc.nh.wSock.readDouble();
            z = mc.nh.wSock.readDouble();

            yaw = mc.nh.wSock.readFloat();
            pitch = mc.nh.wSock.readFloat();

            onGround = mc.nh.wSock.readBool();

            if (mc.ThisPlayer == null)  // -- Update player's location.
                mc.ThisPlayer = new Player();

                mc.ThisPlayer.location.x = x; mc.ThisPlayer.location.y = y; mc.ThisPlayer.location.z = z;
                mc.ThisPlayer.look[0] = yaw; mc.ThisPlayer.look[1] = pitch;
                mc.ThisPlayer.onGround = onGround;

                mc.RaiseLocationChanged();

                Packets.Play.ServerBound.PlayerPositionAndLook c = new Packets.Play.ServerBound.PlayerPositionAndLook(ref mc);
                mc.First = true;
                
        }
    }
}
