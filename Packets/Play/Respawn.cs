using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            mc.minecraftWorld = new Classes.World(); // -- We *should* be receiving a new world, so completely redefine it!

            mc.minecraftWorld.dimension = (sbyte)dimension;
            mc.minecraftWorld.difficulty = difficulty;
            mc.minecraftWorld.levelType = levelType;

            if (mc.thisPlayer == null)
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.gameMode = gameMode;

            mc.raisePlayerRespawn();
        }
    }
}
