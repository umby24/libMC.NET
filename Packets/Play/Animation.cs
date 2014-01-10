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

            if (mc.ThisPlayer != null && Entity_ID == mc.ThisPlayer.Entity_ID)
                mc.ThisPlayer.Animation = Ani;

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1)
                    mc.MinecraftWorld.Entities[eIndex].animation = Ani;
            }

            mc.RaiseEntityAnimationChanged(this, Entity_ID, Ani);
        }
    }
}
