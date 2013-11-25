﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class useBed : Packet {
        public int Entity_ID, X, Z;
        public byte Y;

        public useBed(ref Minecraft mc) {
            Entity_ID = mc.nh.wSock.readInt();
            X = mc.nh.wSock.readInt();
            Y = mc.nh.wSock.readByte();
            Z = mc.nh.wSock.readInt();

            if (mc.thisPlayer != null && Entity_ID == mc.thisPlayer.Entity_ID)
                mc.thisPlayer.inBed = true;

            //TODO: Track other entities entering beds.

            mc.raiseDebug(this, "Player entered a bed");
        }
    }
}