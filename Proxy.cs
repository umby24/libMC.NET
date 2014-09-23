using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using CWrapped;

namespace libMC.NET {
    public class Proxy {
        #region Variables
        public string Ip, Outfile;
        public int SPort, CPort;

        TcpListener _cbaseListener;
        TcpClient _cbaseSock;
        NetworkStream _cbaseStream;

        TcpClient _sbaseSock;
        NetworkStream _sbaseStream;

        public Wrapped SSock;
        public Wrapped CSock;
        StreamWriter _logger;

        Thread _serverThread;
        Thread _clientThread;
        #endregion

        public Proxy(string serverIp, int serverPort, int clientPort) {
            Ip = serverIp;
            SPort = serverPort;
            CPort = clientPort;
            Outfile = "OUTPUT.LOG";
        }

        /// <summary>
        /// Starts the transparent proxy
        /// </summary>
        public void Start() {
            _cbaseListener = new TcpListener(IPAddress.Any, CPort);
            _cbaseListener.Start();
            _cbaseSock = _cbaseListener.AcceptTcpClient();

            _sbaseSock = new TcpClient(Ip, SPort);
            _sbaseStream = _sbaseSock.GetStream();

            _cbaseStream = _cbaseSock.GetStream(); // -- Client accepted successfully, woo.

            SSock = new Wrapped(_sbaseStream);
            CSock = new Wrapped(_cbaseStream);

            _serverThread = new Thread(ServerNetworkHandler);
            _clientThread = new Thread(ClientNetworkHandler);
            _serverThread.Start();
            _clientThread.Start();
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
            _clientThread.Abort();
            _serverThread.Abort();

            if (_logger != null)
                _logger.Close();
        }

        void ClientNetworkHandler() {
            try {
                int length;

                while ((length = CSock.readVarInt()) != 0) {
                    if (_cbaseSock.Connected) {
                        var packetId = CSock.readVarInt();
                        Log(false, "PACKET " + packetId.ToString());

                        if (packetId == 6) {
                            var x = CSock.readDouble();
                            var y = CSock.readDouble();
                            var stance = CSock.readDouble();
                            var z = CSock.readDouble();

                            var yaw = CSock.readFloat();
                            var pitch = CSock.readFloat();

                            var onGround = CSock.readBool();

                            Log(false, "X: " + x.ToString() + " Y: " + y.ToString() + " Stance: " + stance.ToString() + " Z: " + z.ToString() + " yaw: " + yaw.ToString() + " pitch " + pitch.ToString() + " OnGround: " + onGround.ToString());
                            SSock.writeVarInt(packetId);
                            SSock.writeDouble(x);
                            SSock.writeDouble(y);
                            SSock.writeDouble(stance);
                            SSock.writeDouble(z);

                            SSock.writeFloat(yaw);
                            SSock.writeFloat(pitch);
                            SSock.writeBool(onGround);
                            SSock.Purge();
                        } else {
                            SSock.writeVarInt(packetId);
                            SSock.Send(CSock.readByteArray(length - 1));
                            SSock.Purge();
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
                int length;

                while ((length = SSock.readVarInt()) != 0) {
                    if (_sbaseSock.Connected) {
                        var packetId = SSock.readVarInt();
                        Log(true, "PACKET " + packetId.ToString());

                        if (packetId == 8) {
                            var x = SSock.readDouble();
                            var y = SSock.readDouble();
                            var z = SSock.readDouble();

                            var yaw = SSock.readFloat();
                            var pitch = SSock.readFloat();

                            var onGround = SSock.readBool();

                            Log(true, "X: " + x.ToString() + " Y: " + y.ToString() + " Z: " + z.ToString() + " yaw: " + yaw.ToString() + " pitch " + pitch.ToString() + " OnGround: " + onGround.ToString());

                            CSock.writeVarInt(packetId);
                            CSock.writeDouble(x);
                            CSock.writeDouble(y);
                            CSock.writeDouble(z);

                            CSock.writeFloat(yaw);
                            CSock.writeFloat(pitch);
                            CSock.writeBool(onGround);
                            CSock.Purge();
                        } else {
                            CSock.writeVarInt(packetId);
                            CSock.Send(SSock.readByteArray(length - 1));
                            CSock.Purge();
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

        public delegate void LogHandler(string message);
        public void Log(bool server, string message) {
            if (_logger == null)
                _logger = new StreamWriter(Outfile);

            if (server)
                _logger.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "> [SERVER] " + message);
            else
                _logger.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "> [CLIENT] " + message);
        }
    }
}
