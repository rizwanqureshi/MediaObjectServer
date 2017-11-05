using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MOS.Entities;
using MOS.Utilities;
using SimpleTCP;
using System.Net;
using System.Net.Sockets;

namespace MOS.Middleware
{
    enum MessageType
    {
        REPLY,
        UPDATE
    }

    public class NcsServerException : Exception
    {
        public NcsServerException() : base() { }
        public NcsServerException(string message) : base(message) { }
        public NcsServerException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected NcsServerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public class NcsServer
    {
        public NcsServer()
        {

        }
        public NcsServer(int port, string ncsID, List<MosClient> mosClients)
        {
            Port = port;
            NcsID = ncsID;
            MosClients = mosClients;
        }

        public int Port { get; set; }
        public string NcsID { get; set; }
        public List<MosClient> MosClients { get; set; }

        public void Start()
        {
            try
            {
                StartServer();
                ConnectMosClients();
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);

                }
            }

            catch (Exception ex)
            {
                throw new NcsServerException("error starting ncs server!", ex);
            }
        }

        public event EventHandler<MosClient> clientConnected;
        public event EventHandler<NcsServer> lowerPortsServiceStarted;

        public event EventHandler<mos> replyReceivedfromClient;
        public event EventHandler<mos> updateReceivedfromClient;


        public event EventHandler<roAck> roAckReceived;
        public event EventHandler<heartbeat> heartbeatReceived;
        public event EventHandler<mosObj> mosObjReceived;
        public event EventHandler<roReqAll> roReqAllReceived;
        public event EventHandler<mos> mosReceived;

        private void ConnectMosClients()
        {
            SimpleTcpClient client = null;

            if (MosClients == null) return;
            if (MosClients.Count < 1) throw new NcsServerException("no mos client exist");
            foreach (var mosClient in MosClients)
            {
                try
                {
                    client = new SimpleTcpClient();
                    client.DataReceived += Client_DataReceived;
                    client.DelimiterDataReceived += Client_DelimiterDataReceived;
                    client.StringEncoder = Encoding.BigEndianUnicode;
                    client.AutoTrimStrings = true;
                    client.Connect(mosClient.HostName, mosClient.UpperPort);
                }

                catch (Exception ex)
                {
                    throw new NcsServerException(string.Format("error connecting mos client {0}", mosClient.HostName, mosClient.UpperPort), ex);
                }
            }
        }

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            RaiseEvents(e, MessageType.REPLY);
        }

        SimpleTcpServer _server = null;
        private void StartServer()
        {
            try
            {
                if (Port == default(int)) throw new NcsServerException("port not specified");                
                if (NcsID == default(string)) throw new NcsServerException("ncs id not specified");                

                _server = new SimpleTcpServer();
                Task.Run(() =>
                {
                    _server.ClientConnected += Server_ClientConnected;
                    _server.ClientDisconnected += Server_ClientDisconnected; ;
                    _server.DataReceived += Server_DataReceived; ;
                    _server.DelimiterDataReceived += Server_DelimiterDataReceived; ;
                    _server.StringEncoder = Encoding.BigEndianUnicode;
                    _server.AutoTrimStrings = true;
                    _server.Start(Port);
                });
            }

            catch (Exception ex)
            {
                throw new NcsServerException("cannot listen to port " + Port, ex);
            }

        }

        private void Client_DataReceived(object sender, Message e)
        {
            RaiseEvents(e, MessageType.REPLY);
        }

        private void Server_DelimiterDataReceived(object sender, Message e)
        {
            RaiseEvents(e, MessageType.UPDATE);
        }

        private void Server_DataReceived(object sender, Message e)
        {
            RaiseEvents(e, MessageType.UPDATE);
        }

        private void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            throw new NotImplementedException();
        }

        private void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            //clientConnected?.Invoke(sender,)
        }

        private void RaiseEvents(Message message, MessageType type)
        {
            try
            {
                var client = GetMosClientFromTcpClient(message.TcpClient);
                if (client == null) return;

                var mosObject = message.MessageString.DeserializeFromString<mos>();

                if (mosObject != null)
                {
                    if (mosObject.Items.Length > 2)
                    {
                        object mosInnerObject = mosObject.Items[2];

                        if (mosInnerObject.GetType() == typeof(roAck))
                            roAckReceived?.Invoke(client, (roAck)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(heartbeat))
                            heartbeatReceived?.Invoke(client, (heartbeat)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(roReqAll))
                            roReqAllReceived?.Invoke(client, (roReqAll)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(mosObj))
                            mosObjReceived?.Invoke(client, (mosObj)mosInnerObject);

                        mosReceived?.Invoke(client, (mos)mosInnerObject);
                        if (type == MessageType.REPLY)
                            replyReceivedfromClient?.Invoke(client, (mos)mosInnerObject);
                        if (type == MessageType.UPDATE)
                            updateReceivedfromClient?.Invoke(client, (mos)mosInnerObject);
                    }
                }
            }

            catch (Exception ex)
            {
                throw new ArgumentException("error in receiving event", ex);
            }

        }

        private MosClient GetMosClientFromTcpClient(System.Net.Sockets.TcpClient client)
        {
            var address = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
            var ip =((IPEndPoint)client.Client.RemoteEndPoint).Port;
            return  MosClients.Find(x => IPAddress.Parse(x.HostName) == address && x.LowerPort == ip);
        }
    }

    public class MosClient
    {
        public MosClient()
        {
            UpperPort = 10541;
            LowerPort = 10540;
        }



        public string HostName { get; set; }
        public int UpperPort { get; set; }
        public int LowerPort { get; set; }
        public string MosID { get; set; }
        public Queue<mos> MessageQueue { get; set; }
    }


}
