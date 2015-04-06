using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace libMC.NET.Common
{
    public class MinecraftNetInteraction
    {

        public string[] Login(string username, string password) {
            var json = "{\"agent\": {\"name\": \"minecraft\",\"version\": 1},\"username\": \"" + username + "\",\"password\": \"" + password + "\"}";
            string accessToken = "", profileId = "", profileName = "", clientToken = "", clientName = "";

            var wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/authenticate");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (var stream = wreq.GetRequestStream()) {
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

            foreach (var app in root) {
                var appName = app.Key;

                switch (appName) {
                    case "accessToken":
                        accessToken = app.Value.ToString();
                        break;
                    case "clientToken":
                        clientToken = app.Value.ToString();
                        break;
                    case "selectedProfile":
                        profileId = app.Value.First.First.ToString();
                        profileName = (string)app.Value["name"];
                        break;
                    case "availableProfiles":
                        var input = app.ToString();
                        var name = Parser(input);
                        clientName = name;
                        break;
                }
            }

            return new string[] { accessToken, profileId, profileName, clientToken, clientName };
        }

        static string Parser(string str) {
            var regex = new Regex("\"name\": \"(.*)\"");
            var v = regex.Match(str);
            return v.Groups[1].ToString();
        }

        public bool VerifyName(string username, string accessToken, string selectedProfile, string serverHash) {
            var json = "{\"accessToken\": \"" + accessToken + "\",\"selectedProfile\": \"" + selectedProfile + "\",\"serverId\": \"" + serverHash + "\"}";

            var wreq = (HttpWebRequest)WebRequest.Create("https://sessionserver.mojang.com/session/minecraft/join");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (var stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            try {
                var response = (HttpWebResponse)wreq.GetResponse();
                var code = new StreamReader(response.GetResponseStream()).ReadToEnd();
            } catch {
                return false;
            }

            return true;
        }

        public string[] SessionRefresh(string accessToken, string clientToken) {
            var json = "{\"accessToken\": \"" + accessToken + "\",\"clientToken\": \"" + clientToken + "\"}";
            string accessTokenIn = "", clientTokenIn = "";

            var wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/refresh");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (var stream = wreq.GetRequestStream()) {
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

            foreach (var app in root) {
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
            var json = "{\"accessToken\": \"" + accessToken + "\"}";

            var wreq = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/validate");

            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = json.Length;

            using (var stream = wreq.GetRequestStream()) {
                stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            }

            try {
                var response = (HttpWebResponse)wreq.GetResponse();
                var code = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (code == "")
                    return true;

            } catch {
                return false;
            }

            return false;
        }
    }
}
