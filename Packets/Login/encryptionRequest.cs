using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using SMProxy;

namespace libMC.NET.Packets.Login {
    class EncryptionRequest : Packet {
        public EncryptionRequest(ref Minecraft mc) {
            // -- Receive from server.
            string serverID = mc.nh.wSock.readString();
            int pkeyLength = mc.nh.wSock.readShort();
            byte[] publicKey = mc.nh.wSock.readByteArray(pkeyLength);
            int tokenLength = mc.nh.wSock.readShort();
            byte[] token = mc.nh.wSock.readByteArray(tokenLength);

            byte[] sharedKey = new byte[16];

            // -- First, Generate a shared key for use in encryption between the client and server.

            RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(sharedKey);


            if (serverID == "" && mc.VerifyNames) {
                // -- Verify with MinecraftClient.net
                // -- At this point, the server requires a hash containing the server id,
                // -- shared key, and original public key. So we make this, and then pass to MinecraftClient.net

                List<byte> hashlist = new List<byte>();
                hashlist.AddRange(System.Text.Encoding.ASCII.GetBytes(serverID));
                hashlist.AddRange(sharedKey);
                hashlist.AddRange(publicKey);

                byte[] hashData = hashlist.ToArray();

                string hash = JavaHexDigest(hashData);

                Minecraft_Net_Interaction verify = new Minecraft_Net_Interaction();

                if (!verify.VerifyName(mc.ClientName, mc.AccessToken, mc.SelectedProfile, hash)) {
                    mc.RaiseLoginFailure(this, "Failed to verify name with MinecraftClient session server.");
                    mc.Disconnect();
                    return;
                }
            } else {
                mc.RaiseInfo(this, "Name verification off, Skipping authentication.");
            }

            // -- AsnKeyParser is a part of the cryptography.dll, which is simply a compiled version
            // -- of SMProxy's Cryptography.cs, with the server side parts stripped out.
            // -- You pass it the key data and ask it to parse, and it will 
            // -- Extract the server's public key, then parse that into RSA for us.

            AsnKeyParser keyParser = new AsnKeyParser(publicKey);
            RSAParameters Dekey = keyParser.ParseRSAPublicKey();

            // -- Now we create an encrypter, and encrypt the token sent to us by the server
            // -- as well as our newly made shared key (Which can then only be decrypted with the server's private key)
            // -- and we send it to the server.

            RSACryptoServiceProvider cryptoService = new RSACryptoServiceProvider();
            cryptoService.ImportParameters(Dekey);

            byte[] EncryptedSecret = cryptoService.Encrypt(sharedKey, false);
            byte[] EncryptedVerify = cryptoService.Encrypt(token, false);
            mc.nh.wSock.InitEncryption(sharedKey);
            EncryptionResponse er = new EncryptionResponse(ref mc, EncryptedSecret, EncryptedVerify);
        }

        private static string GetHexString(byte[] p) {
            string result = "";
            for (int i = 0; i < p.Length; i++) {
                if (p[i] < 0x10)
                    result += "0";
                result += p[i].ToString("x"); // Converts to hex string
            }
            return result;
        }

        private static byte[] TwosCompliment(byte[] p) // little endian
        {
            int i;
            bool carry = true;
            for (i = p.Length - 1; i >= 0; i--) {
                p[i] = unchecked((byte)~p[i]);
                if (carry) {
                    carry = p[i] == 0xFF;
                    p[i]++;
                }
            }
            return p;
        }

        public static string JavaHexDigest(byte[] data) {
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(data);
            bool negative = (hash[0] & 0x80) == 0x80;
            if (negative) // check for negative hashes
                hash = TwosCompliment(hash);
            // Create the string and trim away the zeroes
            string digest = GetHexString(hash).TrimStart('0');
            if (negative)
                digest = "-" + digest;
            return digest;
        }
    }
}
