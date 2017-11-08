using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mos.Entities;
using Mos.Utilities;
using SimpleTCP;
using System.Net;

namespace Mos.Middleware
{
    enum MessageType
    {
        REPLY,
        UPDATE
    }

    public class MosServerException : Exception
    {
        public MosServerException() : base() { }
        public MosServerException(string message) : base(message) { }
        public MosServerException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected MosServerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public class MosServer
    {
        public MosServer()
        {

        }
        public MosServer(int port, string ncsID, List<MosClient> mosClients)
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

            catch (Exception ex)
            {
                throw new MosServerException("error starting ncs server!", ex);
            }
        }

        public void SendMosCommand(mos mosCommand)
        {
            MosClients.ForEach((x) => {
                x.MessageQueue.Enqueue(mosCommand);
            });
        }
             

        public event EventHandler<MosClient> clientConnected;
        public event EventHandler<MosServer> lowerPortsServiceStarted;

        public event EventHandler<mos> ReplyReceivedfromClient;
        public event EventHandler<mos> UpdateReceivedfromClient;

        public event EventHandler<roAck> RoAckReceived;
        public event EventHandler<heartbeat> HeartbeatReceived;
        public event EventHandler<mosObj> MosObjReceived;
        public event EventHandler<roReqAll> RoReqAllReceived;
        public event EventHandler<mos> MosReceived;

       
        private void ConnectMosClients()
        {
            SimpleTcpClient client = null;
            string errorMessage = default(string);

            if (MosClients == null) return;
            if (MosClients.Count < 1) throw new MosServerException("no MOS client exist");
            foreach (var mosClient in MosClients)
            {
                #region Dequeue task
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (mosClient.MessageQueue.Count > 0)
                            {
                               
                                if (client == null)
                                {
                                    client = new SimpleTcpClient();
                                    client.StringEncoder = Encoding.BigEndianUnicode;                                    
                                    //client.DataReceived += Client_DataReceived;
                                    //client.DelimiterDataReceived += Client_DelimiterDataReceived;
                                    client.Connect(mosClient.IP, mosClient.UpperPort);
                                }
                                else if (!client.TcpClient.Connected)
                                {
                                    client.Connect(mosClient.IP, mosClient.UpperPort);
                                }
                                else
                                {
                                    var mosObj = mosClient.MessageQueue.Peek().SerializeObject();
                                    var message = client.WriteLineAndGetReply(mosObj, TimeSpan.FromMilliseconds(5000));
                                    RaiseEvents(message, MessageType.REPLY);
                                    mosClient.MessageQueue.Dequeue();
                                    //log.Info(string.Format("MESSAGE SENT TO MOS DEVICE {0}\n {1}", host, mosObj));
                                }
                               
                            }
                            
                        }

                        catch (Exception ex)
                        {
                            if (errorMessage != ex.Message)
                            {
                                errorMessage = ex.Message;
                                throw new MosServerException(string.Format("error connecting mos client {0}", mosClient.IP), ex);

                            }
                        }

                        Thread.Sleep(10);
                    }
                });
                #endregion

                #region heartbeat Task
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (mosClient.MessageQueue.Count < 1) // if queue is empty
                        {
                            mosClient.MessageQueue.Enqueue(
                            new mos()
                            {
                                ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.heartbeat },
                                Items = new object[] { mosClient.MosID, "NCS",new heartbeat() { time = DateTime.Now.ToString() } }
                            });
                        }

                        Thread.Sleep(5000);
                    }
                });
                #endregion heartbeat Task
            }
        }

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            RaiseEvents(e, MessageType.REPLY);
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
            e.Reply(Encoding.BigEndianUnicode.GetBytes(DateTime.Now.ToString()));
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

        SimpleTcpServer _server = null;
        private void StartServer()
        {
            try
            {
                if (Port == default(int)) throw new MosServerException("port not specified");
                if (NcsID == default(string)) throw new MosServerException("ncs id not specified");

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
                throw new MosServerException("cannot listen to port " + Port, ex);
            }

        }

        private void RaiseEvents(Message message, MessageType type)
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
                            RoAckReceived?.Invoke(client, (roAck)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(heartbeat))
                            HeartbeatReceived?.Invoke(client, (heartbeat)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(roReqAll))
                            RoReqAllReceived?.Invoke(client, (roReqAll)mosInnerObject);
                        if (mosInnerObject.GetType() == typeof(mosObj))
                            MosObjReceived?.Invoke(client, (mosObj)mosInnerObject);
                       
                        if (type == MessageType.REPLY)
                            ReplyReceivedfromClient?.Invoke(client, (mos)mosObject);

                        if (type == MessageType.UPDATE)
                            UpdateReceivedfromClient?.Invoke(client, (mos)mosObject);

                        MosReceived?.Invoke(client, mosObject);
                    }
                }
           
        }

        private MosClient GetMosClientFromTcpClient(System.Net.Sockets.TcpClient client)
        {
            var address = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
            var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            return MosClients.Find(x => x.IP == address.ToString());
        }
    }
}


/*
 * Project Manager, Technical Architect, Trainer
 * 15+ years of experience in designing/building solution from manufacturing, ecommerce, online to braodcast.
 * I led award winning project of international, served as technical lead in middle east only Innovation Award project at IBC. Newsroom system that mangae to win award at APICTA- Malaysia, PASHA - Pakistan and MANTHAN-india
 * Passionate to harness upcoming technology to the industry.
 * I am currently looking for a challenging opportunity that demands the unique combination of experience in broadcast industry and software development. I am passionate to bring new technology and trends to the broadcast. 
 
 * Project 
 *
 * 
 SNA (Sky News Arabia) OTT Platform (Role: Technical Architect)
 Summary:
 The projects intends to provide OTT service for Sky News Arabia Live and VOD content. 
 This includes OTT infrastructure and delivery on all major playback platforms (Apple, Android, Apple TV, Chrome, Rouk,Fire TV ect.)
 Transcoding is done with an in house solution powered by GPU processing, Packaging and streaming is served by Wowza streaming server and then content is delivered by two different CDN.
 The content is tagged with medata data by dedicated CMS.


SNA SkyNet- Disaster Recovery System: (Role: Software Architect)
The project was the amulgam of differnet innovative products, tools and technology used to build a cost effective and resilliant DR site for broadcast transmission, the site can be controlled remotely by using an intuitive client.
The project credited to be the only Middleeast project to won IBC Innovation Award in 2015.An in house solution was developed to control different devices: Blackmagic Switcher,Videohub Routers,Zixi, Netvx etc. 



SNA DataHub: (Role: Technical Architect)
The DataHub serves as middleware system that ingest different data feeds from variety of sources: Twitter, Facebook, Weather, Business, Sports etc and provide common platform for all sink application to receive information in uniform manner.
Newsroom system, graphic system, notification system are all connected to it.DataHub is capable of consuming number of API: REST API, NewsML, Twitter Streaming API, Thomson Reuters RealTime feed etc.



SNA IPhone/Android App 2.0 Upgrade (Role: Technical Architect)
Mobile revamp with additional features and performance imporvements.

SNA AR/VR
Supervised developed various Touchscreen, AR/VR imlpementation with Vizrt


SNA SAP ERP (Role: Technical Archiect/Project Manager)
intends to replace hetergenous business application with world proven SAP platform.  SAP S4Hana and SuccessFactors are choosen to replace existing Financial and HR System.


 Dawn News TV- Neuron Newsroom Management System (Head of Software Development / Software Architect)
 The only regional newsroom system that is fully integrated with all renowned promer, video and graphic system using MOS protocol. The system provides generlist to create edit stories, attach graphics /videos and manage rundown.
 The system provides collaborative platform with real times changes among all users.This give newsroom the ability to manage entire workflow from news ingestion, production to delivery on broadcast devices.
 The system is integrated with Vizrt, Wasp3D, Autoscript, EVS and other playout via MOS protocol.Neuron was awarded best media product by PASHA (Pakistan Software House Association) and Manthan Award-India as best regional media product. 
 
 

 Dawn News-Traffic & Billing
 An end to end solution for managing fixed point chart, program / promo schedule and EPG in TV channel.



 
  
 */

