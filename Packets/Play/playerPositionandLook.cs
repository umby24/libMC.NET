using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            if (mc.thisPlayer == null)  // -- Update player's location.
                mc.thisPlayer = new Classes.Player();

                mc.thisPlayer.vector[0] = x; mc.thisPlayer.vector[1] = y; mc.thisPlayer.vector[2] = z;
                mc.thisPlayer.look[0] = yaw; mc.thisPlayer.look[1] = pitch;
                mc.thisPlayer.onGround = onGround;

                if (mc.thisPlayer.vector[0] != x)
                    mc.thisPlayer.vector[0] = x;

                mc.raiseLocationChanged();

                Packets.Play.ServerBound.playerPositionAndLook c = new Packets.Play.ServerBound.playerPositionAndLook(ref mc);
                mc.First = true;
                
        }
    }
}
