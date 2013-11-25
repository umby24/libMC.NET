using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Classes;

namespace libMC.NET.Packets.Play {
    class spawnPlayer : Packet {
        Classes.Entity newPlayer;

        public spawnPlayer(ref Minecraft mc) {
            newPlayer = new Classes.Entity(ref mc, "Player");
            newPlayer.Entity_ID = mc.nh.wSock.readVarInt();
            newPlayer.UUID = mc.nh.wSock.readString();
            newPlayer.playerName = mc.nh.wSock.readString();

            newPlayer.Location = new Vector();
            newPlayer.Location.x = mc.nh.wSock.readInt();
            newPlayer.Location.y = mc.nh.wSock.readInt();
            newPlayer.Location.z = mc.nh.wSock.readInt();

            newPlayer.yaw = mc.nh.wSock.readByte();
            newPlayer.pitch = mc.nh.wSock.readByte();
            newPlayer.heldItem = mc.nh.wSock.readShort();

            newPlayer.readEntityMetadata(ref mc);

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new World();

            if (mc.minecraftWorld.Entities == null)
                mc.minecraftWorld.Entities = new List<Classes.Entity>();

            mc.minecraftWorld.Entities.Add(newPlayer);
        }
    }
}
