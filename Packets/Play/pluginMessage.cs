using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class pluginMessage : Packet {
        string Channel;
        byte[] Data;

        public pluginMessage(ref Minecraft mc) {
            Channel = mc.nh.wSock.readString();
            short length = mc.nh.wSock.readShort();
            Data = mc.nh.wSock.readByteArray(length);

            if (Channel == "MC|Brand") {
                ServerBound.PluginMessage PM = new ServerBound.PluginMessage(ref mc, "MC|Brand", Encoding.UTF8.GetBytes("Minebot"));
                ServerBound.playerPositionAndLook ppal = new ServerBound.playerPositionAndLook(ref mc);
            }

            mc.raisePluginMessage(Channel, Data);
        }
    }
}
