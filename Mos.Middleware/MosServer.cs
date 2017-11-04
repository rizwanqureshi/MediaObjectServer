using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MOS.Entities;
using MOS.Utilities;
using SimpleTCP;

namespace MOS.Middleware
{
    public class NcsServerException:Exception
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
            }

            catch(Exception ex)
            {
                throw new NcsServerException("cannot start server!", ex);
            }
        }       

        public event EventHandler<roAck> roAckReceived;
        public event EventHandler<heartbeat> heartbeatReceived;
        public event EventHandler<mosObj> mosObjReceived;
        public event EventHandler<roReqAll> roReqAllReceived;
        public event EventHandler<mos> mosReceived;
        
        private void ConnectMosClients()
        {
            SimpleTcpClient client = null;

            foreach (var mosClient in MosClients)
            {
                client.DataReceived += Client_DataReceived;
                client.DelimiterDataReceived += Client_DelimiterDataReceived;
                client.StringEncoder = Encoding.BigEndianUnicode;
                client.AutoTrimStrings = true;
                client.Connect(mosClient.HostName, mosClient.UpperPort);
                
            }
        }

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            RaiseEvents(e);
        }

        SimpleTcpServer _server = null;
        private void StartServer()
        {
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

        private void Client_DataReceived(object sender, Message e)
        {
            RaiseEvents(e);
        }

        private void Server_DelimiterDataReceived(object sender, Message e)
        {
            RaiseEvents(e);
        }

        private void Server_DataReceived(object sender, Message e)
        {
            RaiseEvents(e);
        }

        private void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            throw new NotImplementedException();
        }

        private void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            throw new NotImplementedException();
        }

        void RaiseEvents(Message message)
        {
            try
            {
                var client = MosClients.Find(x => x.HostName == message.TcpClient.Client.LocalEndPoint.ToString());

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
                        if (mosInnerObject.GetType() == typeof(mos))
                            mosReceived?.Invoke(client, (mos)mosInnerObject);
                    }
                }
            }

            catch (Exception ex)
            {
                throw new ArgumentException("error in receiving event", ex);
            }

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
