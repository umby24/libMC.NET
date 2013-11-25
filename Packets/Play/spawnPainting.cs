using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Classes;

namespace libMC.NET.Packets.Play {
    class spawnPainting : Packet {
        Classes.Entity thisPainting;

        public spawnPainting(ref Minecraft mc) {
            thisPainting = new Classes.Entity(ref mc, "Painting");

            thisPainting.Entity_ID = mc.nh.wSock.readVarInt();
            thisPainting.playerName = mc.nh.wSock.readString();
            thisPainting.Location = new Vector();

            thisPainting.Location.x = mc.nh.wSock.readInt();
            thisPainting.Location.y = mc.nh.wSock.readInt();
            thisPainting.Location.z = mc.nh.wSock.readInt();

            thisPainting.direction = mc.nh.wSock.readInt();

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new World();

            if (mc.minecraftWorld.Entities == null)
                mc.minecraftWorld.Entities = new List<Classes.Entity>();

            mc.minecraftWorld.Entities.Add(thisPainting);
        }
    }
}
