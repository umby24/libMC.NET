using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class SpawnPainting : Packet {
        Entities.Entity thisPainting;

        public SpawnPainting(ref Minecraft mc) {
            thisPainting = new Entities.Entity(ref mc, "Painting");

            thisPainting.Entity_ID = mc.nh.wSock.readVarInt();
            thisPainting.playerName = mc.nh.wSock.readString();
            thisPainting.Location = new Vector();

            thisPainting.Location.x = mc.nh.wSock.readInt();
            thisPainting.Location.y = mc.nh.wSock.readInt();
            thisPainting.Location.z = mc.nh.wSock.readInt();

            thisPainting.direction = mc.nh.wSock.readInt();

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            if (mc.MinecraftWorld.Entities == null)
                mc.MinecraftWorld.Entities = new List<Entities.Entity>();

            mc.MinecraftWorld.Entities.Add(thisPainting);
        }
    }
}
