using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Animation : Packet {
        public int Entity_ID;
        public byte Ani;

        public Animation(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readVarInt();
            Ani = mc.nh.wSock.readByte();

            if (mc.thisPlayer != null && Entity_ID == mc.thisPlayer.Entity_ID)
                mc.thisPlayer.Animation = Ani;

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1)
                    mc.minecraftWorld.Entities[eIndex].animation = Ani;
            }

            mc.RaiseEntityAnimationChanged(this, Entity_ID, Ani);
        }
    }
}
