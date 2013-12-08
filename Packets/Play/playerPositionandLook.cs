using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libMC.NET.Entities;
namespace libMC.NET.Packets.Play {
    class playerPositionandLook : Packet {
        public double x, y, z;
        public float yaw, pitch;
        public bool onGround;

        public playerPositionandLook(ref Minecraft mc) {
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

                mc.raiseLocationChanged();

                Packets.Play.ServerBound.playerPositionAndLook c = new Packets.Play.ServerBound.playerPositionAndLook(ref mc);
                mc.First = true;
                
        }
    }
}
