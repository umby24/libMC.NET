using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libMC.NET.Entities;
using libMC.NET.World;

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

            if (mc.ThisPlayer == null) 
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.Entity_ID = Entity_ID;
            mc.ThisPlayer.gameMode = gameMode;

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            mc.MinecraftWorld.dimension = dimension;
            mc.MinecraftWorld.difficulty = difficulty;
            mc.MinecraftWorld.maxPlayers = maxPlayers;
            mc.MinecraftWorld.levelType = levelType;

            mc.RaiseDebug(this, string.Format("Entity ID: {0}", Entity_ID));
            mc.RaiseGameJoined();

            // -- Vanilla client at this point sends client settings, and plugin message designating the 
            // -- modpack that the client is using.

            ServerBound.clientSettings b = new ServerBound.clientSettings(ref mc);
            ServerBound.PluginMessage c = new ServerBound.PluginMessage(ref mc, "MC|Brand", Encoding.UTF8.GetBytes(mc.ClientBrand));
        }
    }
}
