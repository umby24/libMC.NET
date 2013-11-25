using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json.Linq;

namespace libMC.NET
{
    public class Minecraft_Net_Interaction
    {

        public string[] Login(string username, string password) {
            string json = "{\"agent\": {\"name\": \"minecraft\",\"version\": 1},\"username\": \"" + username + "\",\"password\": \"" + password + "\"}";
            string accessToken = "";
            string profileID = "";

            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/authenticate");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (Stream stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            HttpWebResponse response = (HttpWebResponse)wreq.GetResponse();
            string code = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var root = JObject.Parse(code);

            foreach (KeyValuePair<string, JToken> app in root) {
                var appName = app.Key;

                switch (appName) {
                    case "accessToken":
                        accessToken = app.Value.ToString();
                        break;
                    case "selectedProfile":
                        profileID = app.Value.First.First.ToString();
                        break;
                }
            }

            return new string[] { accessToken, profileID };
        }

        public bool VerifyName(string username, string accessToken, string selectedProfile, string ServerHash) {
            string json = "{\"accessToken\": \"" + accessToken + "\",\"selectedProfile\": \"" + selectedProfile + "\",\"serverId\": \"" + ServerHash + "\"}";

            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("https://sessionserver.mojang.com/session/minecraft/join");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (Stream stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            try {
                HttpWebResponse response = (HttpWebResponse)wreq.GetResponse();
                string code = new StreamReader(response.GetResponseStream()).ReadToEnd();
            } catch {
                return false;
            }

            return true;
        }
    }
}
