using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Teams : Packet {
        public string teamName, teamDisplayName, teamPrefix, teamSuffix;
        public string[] players;
        public byte mode, friendlyFire;
        public short playerCount;

        public Teams(ref Minecraft mc) {
            teamName = mc.nh.wSock.readString();
            mode = mc.nh.wSock.readByte();

            switch (mode) {
                case 0:
                    teamDisplayName = mc.nh.wSock.readString();
                    teamPrefix = mc.nh.wSock.readString();
                    teamSuffix = mc.nh.wSock.readString();
                    friendlyFire = mc.nh.wSock.readByte();
                    playerCount = mc.nh.wSock.readShort();

                    players = new string[playerCount];

                    for (int i = 0; i < playerCount; i++) {
                        players[i] = mc.nh.wSock.readString();
                    }

                    break;
                case 1:
                    // Delete team
                    break;
                case 2: 
                    teamDisplayName = mc.nh.wSock.readString();
                    teamPrefix = mc.nh.wSock.readString();
                    teamSuffix = mc.nh.wSock.readString();
                    friendlyFire = mc.nh.wSock.readByte();
                    break;
                case 3: 
                    playerCount = mc.nh.wSock.readShort();

                    players = new string[playerCount];

                    for (int i = 0; i < playerCount; i++) {
                        players[i] = mc.nh.wSock.readString();
                    }
                    break;
                case 4:
                    playerCount = mc.nh.wSock.readShort();

                    players = new string[playerCount];

                    for (int i = 0; i < playerCount; i++) {
                        players[i] = mc.nh.wSock.readString();
                    }
                    break;
            }
        }
    }
}
