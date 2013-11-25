using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Login {
    class encryptionResponse : Packet {
        public encryptionResponse(ref Minecraft mc, byte[] encryptedKey, byte[] encryptedToken) {
            mc.nh.wSock.writeVarInt(1);
            mc.nh.wSock.writeShort((short)encryptedKey.Length);
            mc.nh.wSock.Send(encryptedKey);
            mc.nh.wSock.writeShort((short)encryptedToken.Length);
            mc.nh.wSock.Send(encryptedToken);
            mc.nh.wSock.Purge();

            // -- Packet sent, now enable encryption.
            mc.nh.wSock.EncEnabled = true;
            mc.nh.raiseSocketInfo(this, "Encryption Enabled");
        }
    }
}
