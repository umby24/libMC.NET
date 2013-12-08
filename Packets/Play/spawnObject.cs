using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libMC.NET.Entities;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class spawnObject : Packet {
        public int Entity_ID;
        public byte type, pitch, yaw;
        public Vector location;
        ObjectEntity newObj;

        //TODO: Implement 'Object' Class.
        public spawnObject(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readVarInt();
            type = mc.nh.wSock.readByte();

            location = new Vector();
            location.x = mc.nh.wSock.readInt();
            location.y = mc.nh.wSock.readInt();
            location.z = mc.nh.wSock.readInt();

            pitch = mc.nh.wSock.readByte();
            yaw = mc.nh.wSock.readByte();

            newObj = new ObjectEntity(type);
            newObj.readObjectData(ref mc);

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            if (mc.MinecraftWorld.worldObjects == null)
                mc.MinecraftWorld.worldObjects = new List<ObjectEntity>();

            mc.MinecraftWorld.worldObjects.Add(newObj);
        }
    }
}
