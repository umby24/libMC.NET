using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using libMC.NET;

namespace TestClient {
    class Program {
        static void Main(string[] args) {
            var MinecraftServer = new Minecraft(args[0], int.Parse(args[1]), "Testbot", "", false);
            MinecraftServer.Connect();

            MinecraftServer.Message += (sender, message, name) => {
                Console.WriteLine("<" + name + "> " + message);
            };

            string command;

            do {
                command = Console.ReadLine();

                if (command.StartsWith("say ")) 
                    libMC.NET.Packets.Play.ServerBound.ChatMessage.SendChat(MinecraftServer, command.Substring(4));
                
            } while (command != "quit");

            MinecraftServer.Disconnect();
        }
    }
}
