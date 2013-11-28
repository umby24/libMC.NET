using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class joinGame : Packet {
        public int Entity_ID;
        public byte gameMode, difficulty, maxPlayers;
        public sbyte dimension;
        public string levelType;

        public joinGame(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readInt();
            gameMode = mc.nh.wSock.readByte();
            dimension = mc.nh.wSock.readSByte();
            difficulty = mc.nh.wSock.readByte();
            maxPlayers = mc.nh.wSock.readByte();
            levelType = mc.nh.wSock.readString();

            if (mc.thisPlayer == null) 
                mc.thisPlayer = new Classes.Player();

            mc.thisPlayer.Entity_ID = Entity_ID;
            mc.thisPlayer.gameMode = gameMode;

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new Classes.World();

            mc.minecraftWorld.dimension = dimension;
            mc.minecraftWorld.difficulty = difficulty;
            mc.minecraftWorld.maxPlayers = maxPlayers;
            mc.minecraftWorld.levelType = levelType;

            mc.raiseDebug(this, string.Format("Entity ID: {0}", Entity_ID));
            mc.raiseGameJoined();

            ServerBound.clientSettings b = new ServerBound.clientSettings(ref mc);
            //ServerBound.PlayerPosition c = new ServerBound.PlayerPosition(ref mc);
        }
    }
}
