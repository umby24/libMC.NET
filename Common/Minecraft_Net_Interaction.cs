using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace libMC.NET.Common
{
    public class Minecraft_Net_Interaction
    {

        public string[] Login(string username, string password) {
            string json = "{\"agent\": {\"name\": \"minecraft\",\"version\": 1},\"username\": \"" + username + "\",\"password\": \"" + password + "\"}";
            string accessToken = "", profileID = "", profileName = "", clientToken = "", clientName = "";

            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/authenticate");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (Stream stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            HttpWebResponse response;
            String code;

            try {
                response = (HttpWebResponse)wreq.GetResponse();
                code = new StreamReader(response.GetResponseStream()).ReadToEnd();
            } catch {
                // -- Error occured possible incorrect password.
                return new[] { "", "", "", "" };
            }

            var root = JObject.Parse(code);

            foreach (KeyValuePair<string, JToken> app in root) {
                var appName = app.Key;

                switch (appName) {
                    case "accessToken":
                        accessToken = app.Value.ToString();
                        break;
                    case "clientToken":
                        clientToken = app.Value.ToString();
                        break;
                    case "selectedProfile":
                        profileID = app.Value.First.First.ToString();
                        profileName = (string)app.Value["name"];
                        break;
                    case "availableProfiles":
                        string input = app.ToString();
                        string name = Parser(input);
                        clientName = name;
                        break;
                }
            }

            return new string[] { accessToken, profileID, profileName, clientToken, clientName };
        }

        static string Parser(string str) {
            Regex regex = new Regex("\"name\": \"(.*)\"");
            var v = regex.Match(str);
            return v.Groups[1].ToString();
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

        public string[] SessionRefresh(string accessToken, string clientToken) {
            string json = "{\"accessToken\": \"" + accessToken + "\",\"clientToken\": \"" + clientToken + "\"}";
            string accessTokenIn = "", clientTokenIn = "";

            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/refresh");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (Stream stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            HttpWebResponse response;
            String code;

            try {
                response = (HttpWebResponse)wreq.GetResponse();
                code = new StreamReader(response.GetResponseStream()).ReadToEnd();
            } catch {
                // -- Error occured possible incorrect credentials provided.
                return new[] { "", "", "" };
            }

            var root = JObject.Parse(code);

            foreach (KeyValuePair<string, JToken> app in root) {
                var appName = app.Key;

                switch (appName) {
                    case "accessToken":
                        accessTokenIn = app.Value.ToString();
                        break;
                    case "clientToken":
                        clientTokenIn = app.Value.ToString();
                        break;
                }
            }

            return new[] { accessTokenIn, clientTokenIn};
        }

        public bool ValidateSession(string accessToken) {
            string json = "{\"accessToken\": \"" + accessToken + "\"}";

            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/validate");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (Stream stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            try {
                HttpWebResponse response = (HttpWebResponse)wreq.GetResponse();
                string code = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (code == "")
                    return true;

            } catch {
                return false;
            }

            return false;
        }
    }
}
