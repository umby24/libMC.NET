using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using CWrapped;

namespace libMC.NET {
    public class Proxy {
        #region Variables
        public string IP, Outfile;
        public int SPort, CPort;

        TcpListener cbaseListener;
        TcpClient cbaseSock;
        NetworkStream cbaseStream;

        TcpClient sbaseSock;
        NetworkStream sbaseStream;

        public Wrapped sSock;
        public Wrapped cSock;
        StreamWriter Logger;

        Thread ServerThread;
        Thread ClientThread;
        #endregion

        public Proxy(string ServerIP, int ServerPort, int ClientPort) {
            IP = ServerIP;
            SPort = ServerPort;
            CPort = ClientPort;
            Outfile = "OUTPUT.LOG";
        }

        /// <summary>
        /// Starts the transparent proxy
        /// </summary>
        public void Start() {
            cbaseListener = new TcpListener(IPAddress.Any, CPort);
            cbaseListener.Start();
            cbaseSock = cbaseListener.AcceptTcpClient();

            sbaseSock = new TcpClient(IP, SPort);
            sbaseStream = sbaseSock.GetStream();

            cbaseStream = cbaseSock.GetStream(); // -- Client accepted successfully, woo.

            sSock = new Wrapped(sbaseStream);
            cSock = new Wrapped(cbaseStream);

            ServerThread = new Thread(ServerNetworkHandler);
            ClientThread = new Thread(ClientNetworkHandler);
            ServerThread.Start();
            ClientThread.Start();
        }

        //public void ClientConnected(IAsyncResult b) {
        //    cbaseListener.EndAcceptTcpClient(b);
        //    cbaseSock = cbaseListener.AcceptTcpClient();

        //    sbaseSock = new TcpClient(IP, SPort);
        //    sbaseStream = sbaseSock.GetStream();

        //    cbaseStream = cbaseSock.GetStream(); // -- Client accepted successfully, woo.

        //    sSock = new Wrapped.Wrapped(sbaseStream);
        //    cSock = new Wrapped.Wrapped(cbaseStream);

        //    ServerThread = new Thread(ServerNetworkHandler);
        //    ClientThread = new Thread(ClientNetworkHandler);
        //    ServerThread.Start();
        //    ClientThread.Start();
        //}

        public void Stop() {
            ClientThread.Abort();
            ServerThread.Abort();

            if (Logger != null)
                Logger.Close();
        }

        void ClientNetworkHandler() {
            try {
                int length = 0;

                while ((length = cSock.readVarInt()) != 0) {
                    if (cbaseSock.Connected) {
                        int packetID = cSock.readVarInt();
                        Log(false, "PACKET " + packetID.ToString());

                        if (packetID == 6) {
                            double x = cSock.readDouble();
                            double y = cSock.readDouble();
                            double Stance = cSock.readDouble();
                            double z = cSock.readDouble();

                            float yaw = cSock.readFloat();
                            float pitch = cSock.readFloat();

                            bool onGround = cSock.readBool();

                            Log(false, "X: " + x.ToString() + " Y: " + y.ToString() + " Stance: " + Stance.ToString() + " Z: " + z.ToString() + " yaw: " + yaw.ToString() + " pitch " + pitch.ToString() + " OnGround: " + onGround.ToString());
                            sSock.writeVarInt(packetID);
                            sSock.writeDouble(x);
                            sSock.writeDouble(y);
                            sSock.writeDouble(Stance);
                            sSock.writeDouble(z);

                            sSock.writeFloat(yaw);
                            sSock.writeFloat(pitch);
                            sSock.writeBool(onGround);
                            sSock.Purge();
                        } else {
                            sSock.writeVarInt(packetID);
                            sSock.Send(cSock.readByteArray(length - 1));
                            sSock.Purge();
                        }
                    }
                }
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) {
                    Log(false, "Error occured, stopping proxy.");
                    Stop();
                }
            }
        }

        void ServerNetworkHandler() {
            try {
                int length = 0;

                while ((length = sSock.readVarInt()) != 0) {
                    if (sbaseSock.Connected) {
                        int packetID = sSock.readVarInt();
                        Log(true, "PACKET " + packetID.ToString());

                        if (packetID == 8) {
                            double x = sSock.readDouble();
                            double y = sSock.readDouble();
                            double z = sSock.readDouble();

                            float yaw = sSock.readFloat();
                            float pitch = sSock.readFloat();

                            bool onGround = sSock.readBool();

                            Log(true, "X: " + x.ToString() + " Y: " + y.ToString() + " Z: " + z.ToString() + " yaw: " + yaw.ToString() + " pitch " + pitch.ToString() + " OnGround: " + onGround.ToString());

                            cSock.writeVarInt(packetID);
                            cSock.writeDouble(x);
                            cSock.writeDouble(y);
                            cSock.writeDouble(z);

                            cSock.writeFloat(yaw);
                            cSock.writeFloat(pitch);
                            cSock.writeBool(onGround);
                            cSock.Purge();
                        } else {
                            cSock.writeVarInt(packetID);
                            cSock.Send(sSock.readByteArray(length - 1));
                            cSock.Purge();
                        }
                    }
                }
            } catch (Exception e) {
                if (e.GetType() != typeof(ThreadAbortException)) {
                    Log(true, "Error occured, stopping proxy.");
                    Stop();
                }
            }
        }

        public delegate void T_LogHandler(string Message);
        public void Log(bool Server, string Message) {
            if (Logger == null)
                Logger = new StreamWriter(Outfile);

            if (Server)
                Logger.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "> [SERVER] " + Message);
            else
                Logger.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "> [CLIENT] " + Message);
        }
    }
}
