using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class spawnMob : Packet {
        public Entities.Entity thisMob;

        public spawnMob(ref Minecraft mc) {
            thisMob = new Entities.Entity(ref mc, "Mob");

            thisMob.Entity_ID = mc.nh.wSock.readVarInt();
            thisMob.mobType = mc.nh.wSock.readByte();

            thisMob.Location = new Vector();
            thisMob.Location.x = mc.nh.wSock.readInt();
            thisMob.Location.y = mc.nh.wSock.readInt();
            thisMob.Location.z = mc.nh.wSock.readInt();

            thisMob.pitch = mc.nh.wSock.readByte();
            thisMob.headPitch = mc.nh.wSock.readByte();
            thisMob.yaw = mc.nh.wSock.readByte();

            thisMob.Velocity_X = mc.nh.wSock.readShort();
            thisMob.Velocity_Y = mc.nh.wSock.readShort();
            thisMob.Velocity_Z = mc.nh.wSock.readShort();

            thisMob.readEntityMetadata(ref mc);

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            if (mc.MinecraftWorld.Entities == null)
                mc.MinecraftWorld.Entities = new List<Entities.Entity>();

            mc.MinecraftWorld.Entities.Add(thisMob);
        }
    }
}
