using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class playerListItem : Packet {
        public string playerName;
        public bool online;
        public short ping;

        public playerListItem(ref Minecraft mc) {
            playerName = mc.nh.wSock.readString();
            online = mc.nh.wSock.readBool(); // -- If false, remove them.
            ping = mc.nh.wSock.readShort();

            if (online == true) {
                if (mc.Players.ContainsKey(playerName)) {
                    mc.Players[playerName] = ping;
                    mc.raisePlayerlistUpdate(playerName, ping);
                } else {
                    mc.Players.Add(playerName, ping);
                    mc.raisePlayerlistAdd(playerName, ping);
                }
            } else {
                mc.Players.Remove(playerName);
                mc.raisePlayerlistRemove(playerName);
            }
        }
    }
}
