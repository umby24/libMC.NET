using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class PluginMessage : Packet {
        string Channel;
        byte[] Data;

        public PluginMessage(ref Minecraft mc) {
            Channel = mc.nh.wSock.readString();
            short length = mc.nh.wSock.readShort();
            Data = mc.nh.wSock.readByteArray(length);

            mc.RaisePluginMessage(Channel, Data);
        }
    }
}
