using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class PluginMessage {
        public PluginMessage(ref Minecraft mc, string channel, byte[] data) {
            mc.nh.wSock.writeVarInt(0x17);
            mc.nh.wSock.writeString(channel);
            mc.nh.wSock.writeShort((short)data.Length);
            mc.nh.wSock.Send(data);

            mc.nh.wSock.Purge();
        }
    }
}
