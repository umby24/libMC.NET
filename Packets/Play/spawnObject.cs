using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace libMC.NET.Packets.Play {
    class spawnObject : Packet {
        public int Entity_ID;
        public byte type, pitch, yaw;
        public Classes.Vector location;
        Classes.Object newObj;

        //TODO: Implement 'Object' Class.
        public spawnObject(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readVarInt();
            type = mc.nh.wSock.readByte();

            location = new Classes.Vector();
            location.x = mc.nh.wSock.readInt();
            location.y = mc.nh.wSock.readInt();
            location.z = mc.nh.wSock.readInt();

            pitch = mc.nh.wSock.readByte();
            yaw = mc.nh.wSock.readByte();

            newObj = new Classes.Object(type);
            newObj.readObjectData(ref mc);

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new Classes.World();

            if (mc.minecraftWorld.worldObjects == null)
                mc.minecraftWorld.worldObjects = new List<Classes.Object>();

            mc.minecraftWorld.worldObjects.Add(newObj);
        }
    }
}
