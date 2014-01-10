using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityVelocity : Packet {
        public EntityVelocity(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();

            short Velocity_X = mc.nh.wSock.readShort();
            short Velocity_Y = mc.nh.wSock.readShort();
            short Velocity_Z = mc.nh.wSock.readShort();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].Velocity_X = Velocity_X;
                    mc.MinecraftWorld.Entities[eIndex].Velocity_Y = Velocity_Y;
                    mc.MinecraftWorld.Entities[eIndex].Velocity_Z = Velocity_Z;
                }
            }

            mc.RaiseEntityVelocityChanged(Entity_ID, Velocity_X, Velocity_Y, Velocity_Z);
        }
    }
}
