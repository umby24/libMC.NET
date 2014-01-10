using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Packets.Play {
    class SoundEffect : Packet {
        string soundName;
        Vector position;
        float volume;
        byte pitch;

        public SoundEffect(ref Minecraft mc) {
            soundName = mc.nh.wSock.readString();
            position = new Vector(mc.nh.wSock.readInt(), mc.nh.wSock.readInt(), mc.nh.wSock.readInt());
            volume = mc.nh.wSock.readFloat();
            pitch = mc.nh.wSock.readByte();

            mc.RaiseDebug(this, "Sound effect");
        }
    }
}
