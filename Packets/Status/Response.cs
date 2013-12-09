using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;

using Newtonsoft.Json.Linq;

namespace libMC.NET.Packets.Status {
    public class Response : Packet {
        public string versionName;
        public int ProtocolVersion;
        public int MaxPlayers;
        public int OnlinePlayers;
        public List<string> players;

        public string MOTD;
        public Image favicon;

        public Response(ref Minecraft mc) {
            string jsonResponse = mc.nh.wSock.readString();
            JToken jsonObj = JToken.Parse(jsonResponse);

            versionName = jsonObj["version"]["name"].Value<string>();
            ProtocolVersion = jsonObj["version"]["protocol"].Value<int>();

            MaxPlayers = jsonObj["players"]["max"].Value<int>(); ;
            OnlinePlayers = jsonObj["players"]["online"].Value<int>();

            var tempPlayers = jsonObj["players"]["sample"];

            if (tempPlayers != null) {
                players = new List<string>();

                foreach (JObject b in tempPlayers) {
                    players.Add(b.Last.First.ToString());
                }
            }

            MOTD = jsonObj["description"].Value<string>();
            string imageString = jsonObj["favicon"].Value<string>();

            // -- Below is a test favicon image.
            //string imageString = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAIAAAAlC+aJAAAEL0lEQVR42u1ay40UMRTcbMiBE8QwIZAF4rB3EuBIKHtHpARYetJTUVXvtd3THj5aqzXanXHbVX6/srufnl7bP99efm+X938E6NFut1v89HzUotvor4P8AfT5ZSC7zbXs3wy4F32u98/nd+OK75cIRP+8HW2yHX2uX0yfBJDD6GA/Ez0SiBFizL0ccmiLILk1BBJlxR9n2dIyEA8J6I09ARx8O3o1gnZ+8/YTXtrBLv8uDpgEKRAt3Ljef/ianw0fDB6a6/qFzxjNiSvc+feXbz/G58fP38cf41OZoO9RPbkSPebB/NKCrlYdyRCHGFBrxQUcED0Z18Id4Ma/cVV+FQSwAzmqrXf3Lj+mkQo6fhNAk4z+Oj6HQdQUOdd5I1BS692GgOKviTI8h8gkJTKFGgFDbrncYmXVkK3Q21w0emZn7BD0kANV7oUirZ3s2iOsCE0LvQriNAi6H0Y22qGH54XauJPkIRLAFdWskjapchEyyQ6UZDF8E0nw6Tjob0iDENhkomm+cifkj+krjUmzTxmBJAoKBJo+VosWONwgEFA020KhBcHmVoXkCRAzukcn1pxDsYj9q7qWhHEQdCS7mt4I5P2NLLNYCYpSVb/HxEpFsJJMGAml1tc9HmE6zCHWxSsj0FCUFaoNZ0kA5VS1/AQ3Zs1vSLdVichmLdufcroKYUOgqrtYU1XbqOc08i7pZXVrCJDUK+O4EQ7ox41yRpJVwsUyovx1ZCsujoVn9u6Tj03wpOeCknodSamqmCikYwIom8mFmqWy9tGUhTmn0lFW3qFEnbKAoq9mqhyMLIYSGkve4XaCUM1aYIkAuTV5EeEjV9E00MTxGQvk6uqmBFdLM0+TMVGPEGF11GssQHVedXwje1QmIGddfhrhGgvY1G7TqNqnkj3qbzbLnbQA1QGVBvPSH8nHv2kNKhG6TOezEBnBerlVoNWVt6P0pxXRAc/UgSYRUZVRI+gBFu3lUTtpElMXmq3EM1rIzmHP2zA1qR1UDjZ5bFYLzahRjdpmK9MkUw0b3RucVKOH+4Fm4zKJfum0Ym0/kJ7T7MiqdSIdSqHZ7PrRefrlj+1YtyM73BOj8lF8GCR4FHfoTpWqW9sTz5xKVAXL5px+Q4N9rMpaPpVYPRdqKDURouZq0J85F5o5mbPOTSU2RU51+NPv+k+ezM2fjWow0GVPvqgMay2792z0xOl0leOrxJ+nJk2I33U6vfp8oNqgkA4l0I3pLng+sPSExp7JWTez22KL/oInNL3CS79EZBnBfVVuoGO8XfaMzD6ltGcWekZNWWjm6IoU2/VPWiefE6tObuJ++3PiySf1979DsvFJff+uROq/1YbpZfu7EvZtFXpBaLXp60Pb0f8/ryne4zm9Lz2OxmnPaXzptU0H8cue9oggvtBzXn3pr/GcR/jSVs+5x5d+Ac6CkhN/jsjpAAAAAElFTkSuQmCC";
            
            if (imageString != null) {
                try {
                    byte[] imageBytes = Convert.FromBase64String(imageString.Replace("data:image/png;base64,", ""));

                    MemoryStream ms = new MemoryStream(imageBytes);
                    favicon = Image.FromStream(ms, false, true);
                } catch {
                    favicon = null;
                }
            }

            mc.RaisePingResponse(versionName, ProtocolVersion, MaxPlayers, OnlinePlayers, players.ToArray(), MOTD, favicon);
            ClientPing cp = new ClientPing(ref mc); // -- Send a ping.
        }
    }
}
