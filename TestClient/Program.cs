using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using libMC.NET.Client;

namespace TestClient {
    class Program {
        static void Main(string[] args) {
            var MinecraftServer = new MinecraftClient(args[0], int.Parse(args[1]), "USERNAME HERE", "PASSWORD HERE", true);
            MinecraftServer.ServerState = 2;

            MinecraftServer.Message += (sender, message, name) => {
                Console.WriteLine("<" + name + "> " + message);
            };

            //MinecraftServer.DebugMessage += (sender, message) => {
            //    Console.WriteLine("[DEBUG][" + sender.ToString() + "] " + message);
            //};

            MinecraftServer.LoginFailure += (sender, message) => {
                Console.WriteLine("Login Error: " + message);
            };

            MinecraftServer.ErrorMessage += (sender, message) => {
                Console.WriteLine("[ERROR][" + sender.ToString() + "] " + message);
            };

            MinecraftServer.InfoMessage += (sender, message) => {
                Console.WriteLine("[INFO][" + sender.ToString() + "] " + message);
            };

            MinecraftServer.PlayerRespawned += () => {
                Console.WriteLine("[Info] You respawned!");
            };

            if (MinecraftServer.VerifyNames)
                MinecraftServer.Login();

            MinecraftServer.Connect();

            string command;

            do {
                command = Console.ReadLine();

                if (command.StartsWith("say ")) 
                    MinecraftServer.SendChat(command.Substring(4));

                if (command.StartsWith("respawn")) {
                    MinecraftServer.Respawn();
                }
            } while (command != "quit");

            MinecraftServer.Disconnect();

            Console.ReadKey();
        }
    }
}
