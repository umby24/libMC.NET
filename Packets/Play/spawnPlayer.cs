using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class SpawnPlayer : Packet {
        Entities.Entity newPlayer;

        public SpawnPlayer(ref Minecraft mc) {
            newPlayer = new Entities.Entity(ref mc, "Player");
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

            newPlayer.ReadEntityMetadata(ref mc);

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            if (mc.MinecraftWorld.Entities == null)
                mc.MinecraftWorld.Entities = new List<Entities.Entity>();

            mc.MinecraftWorld.Entities.Add(newPlayer);
        }
    }
}
