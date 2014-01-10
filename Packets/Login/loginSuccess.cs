using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.Packets.Login {
    class LoginSuccess : Packet {
        public LoginSuccess(ref Minecraft mc) {
            string UUID = mc.nh.wSock.readString();
            string Username = mc.nh.wSock.readString();

            mc.RaiseLoginSuccess(this);
            mc.RaiseDebug(this, "UUID: " + UUID + " Username: " + Username);

            if (mc.ThisPlayer == null)
                mc.ThisPlayer = new Player();

            mc.ThisPlayer.playerName = Username;

            mc.ServerState = 3;
            mc.RaiseDebug(this, "The server state is now 3 (Play)");
        }
    }
}
