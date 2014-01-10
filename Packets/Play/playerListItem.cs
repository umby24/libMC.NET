using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class PlayerListItem : Packet {
        public string playerName;
        public bool online;
        public short ping;

        public PlayerListItem(ref Minecraft mc) {
            playerName = mc.nh.wSock.readString();
            online = mc.nh.wSock.readBool(); // -- If false, remove them.
            ping = mc.nh.wSock.readShort();

            if (online == true) {
                if (mc.Players.ContainsKey(playerName)) {
                    mc.Players[playerName] = ping;
                    mc.RaisePlayerlistUpdate(playerName, ping);
                } else {
                    mc.Players.Add(playerName, ping);
                    mc.RaisePlayerlistAdd(playerName, ping);
                }
            } else {
                mc.Players.Remove(playerName);
                mc.RaisePlayerlistRemove(playerName);
            }
        }
    }
}
