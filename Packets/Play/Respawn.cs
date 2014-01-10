using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.World;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Play {
    class Respawn : Packet {
        public int dimension;
        public byte difficulty, gameMode;
        public string levelType;

        public Respawn(ref Minecraft mc) {
            dimension = mc.nh.wSock.readInt();
            difficulty = mc.nh.wSock.readByte();
            gameMode = mc.nh.wSock.readByte();
            levelType = mc.nh.wSock.readString();

            mc.MinecraftWorld = new WorldClass(); // -- We *should* be receiving a new world, so completely redefine it!

            mc.MinecraftWorld.dimension = (sbyte)dimension;
            mc.MinecraftWorld.difficulty = difficulty;
            mc.MinecraftWorld.levelType = levelType;

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.gameMode = gameMode;

            mc.RaisePlayerRespawn();
        }
    }
}
