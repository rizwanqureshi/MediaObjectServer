using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Threading;
using log4net;
using log4net.Config;
using SimpleTCP;
using Mos.Entities;
using Mos.Middleware;

namespace MediaObjectServer
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static List<Queue<mos>> mosRequestQueueList = new List<Queue<mos>>();
       
        static int messageID = 0;

        static void Main(string[] args)
        {

            try
            {
                MosServer ncsServer = new MosServer()
                {
                    Port = 10541,
                    NcsID = "NCS",
                    MosClients = new List<MosClient>() {
                    new MosClient{  IP="127.0.0.1", UpperPort=10541, MosID="PROMPTER"}
                }
                };

                ncsServer.MosReceived += NcsServer_mosReceived;
                ncsServer.Start();

                return;
                #region Start Server
                SimpleTcpServer server = new SimpleTcpServer();
                Task.Run(() =>
                {
                    server.ClientConnected += Server_ClientConnected;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    server.DataReceived += Server_DataReceived;
                    server.DelimiterDataReceived += server_DelimiterDataReceived;
                    server.StringEncoder = Encoding.UTF8;
                    server.Start(10541);
                });
            }

            catch(Exception ex)
            {
                log.Error(ex.Message);
            }

            //server = new SimpleTcpServer();
            //Task.Run(() =>
            //{
            //    server.ClientConnected += Server_ClientConnected;
            //    server.ClientDisconnected += Server_ClientDisconnected;
            //    server.DataReceived += Server_DataReceived;
            //    server.DelimiterDataReceived += Server_DelimiterDataReceived;
            //    server.StringEncoder = Encoding.BigEndianUnicode;
            //    server.Start(10540);
            //});
            #endregion

            InitializedMosClient("127.0.0.1", 10541, "PROMPTER", new Queue<mos>());
            // InitializedMosClient("10.69.70.102", UPPER_PORT, "PILOT", new Queue<mos>());
            
            //InitializedMosClient("10.69.70.102", LOWER_PORT, "PILOT", new Queue<mos>());


            //   MosAPI mosPackets = new MosAPI();
            //mosRequestQueueList.ForEach(x=>x.Enqueue(mosPackets.roCreate()));
            //  Thread.Sleep(1000);
            //  mosRequestQueueList.ForEach(x => x.Enqueue(new MosAPI().roDelete("Rundown Schedule-79e1e3a0-8a5d-4e39-b396-9255c3e08fc0")));
            while (Console.ReadKey().Key == ConsoleKey.Q) { }
        }

        private static void NcsServer_mosReceived(object sender, mos e)
        {
            log.Info(e.SerializeObject());
        }

        private static void NcsServer_mosObjReceived(object sender, mosObj e)
        {
           
        }

        static void server_DelimiterDataReceived(object sender, Message e)
        {
            log.Info(e.MessageString);
        }

        static void Program_roAckReceived(object sender, roAck e)
        {
            log.Warn("roAck Received");
        }

        static void Program_heartbeatReceived(object sender, heartbeat e)
        {
            log.Warn("--------------heart received------------" + e.time);
        }

        #region server events

        private static void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            log.Warn(string.Format("CLIENT [ {0} ] DISCONNECTED", e.Client.RemoteEndPoint.ToString()));
        }

        private static void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            log.Warn(string.Format("CLIENT [ {0} ] CONNECTED", e.Client.RemoteEndPoint));
        }

        private static void Server_DataReceived(object sender, Message e)
        {
            try
            {
                log.Info(e.MessageString);

                // log.Warn(string.Format("MESSAGE RECEIVED AT SERVER FROM [ {1} ] \n {2}", ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Port, e.TcpClient.Client.RemoteEndPoint.ToString(), e.MessageString));
                //  RaiseEvents(e.MessageString);
            }

            catch (Exception ex)
            {
                log.Debug(ex.Message);
            }
        }
        #endregion

        #region client events
        static void client_DataReceived(object sender, Message e)
        {
            try
            {
                //log.Warn(string.Format("MESSAGE RECEIVED FROM MOS DEVICE [ {1} ] \n {2}", ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Port, e.TcpClient.Client.RemoteEndPoint.ToString(), e.MessageString));
                //RaiseEvents(e.MessageString);
            }

            catch (Exception ex)
            {
                log.Debug(ex.Message);
            }
        }
        #endregion

        public static object GetMosObject(string mosString)
        {
            try
            {
                var mosObject = mosString.DeserializeFromString<mos>();
                if (mosObject != null)
                {
                    if (mosObject.Items.Length > 2)
                    {
                        return mosObject.Items[2].GetType().ToString();
                    }
                }
                return null;
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        static void InitializedMosClient(string host, int port, string mosId, Queue<mos> mosRequestQueue)
        {
            mosRequestQueueList.Add(mosRequestQueue);
            SimpleTcpClient client = null;
            string errorMessage = "";

            #region Dequeue task
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (mosRequestQueue.Count > 0)
                        {

                            if (client == null)
                            {
                                client = new SimpleTcpClient();
                                client.StringEncoder = Encoding.BigEndianUnicode;
                                client.DataReceived += client_DataReceived;
                                client.Connect(host, port);
                                log.Info(string.Format("NCS CONNECTED TO HOST {0}:{1}", host, port));
                            }

                            if (!client.TcpClient.Connected)
                            {
                                client.Connect(host, port);
                            }

                            var mosObj = mosRequestQueue.Peek().SerializeObject();
                            if (client.TcpClient.Connected)
                            {
                                var Message = client.WriteLineAndGetReply(mosObj, TimeSpan.FromMilliseconds(10));
                                mosRequestQueue.Dequeue();
                                log.Info(string.Format("MESSAGE SENT TO MOS DEVICE {0}\n {1}", host, mosObj));
                            }
                        }
                    }



                    catch (System.Net.Sockets.SocketException ex)
                    {

                        if (errorMessage != ex.Message)
                        {
                            errorMessage = ex.Message;
                            log.Error("NETWORK SOCKET EXCEPTION");
                        }

                    }

                    catch (Exception ex)
                    {
                        if (errorMessage != ex.Message)
                        {
                            errorMessage = ex.Message;
                            log.Error(ex.Message);
                        }
                    }

                    Thread.Sleep(10);
                }
            });
            #endregion

            #region HeartBeat Task

            SendHeartBeat(mosId, mosRequestQueue);

            #endregion

        }

        static void SendHeartBeat(string mosId, Queue<mos> mosRequestQueue)
        {

            Task.Run(() =>
            {
                while (true)
                {
                    if (mosRequestQueue.Count < 1) // if queue is empty
                    {
                        mosRequestQueue.Enqueue(
                        new mos()
                        {
                            ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.heartbeat },
                            Items = new object[] { mosId, "NCS", ++messageID, new heartbeat() { time = DateTime.Now.ToString() } }
                        });
                    }


                    Thread.Sleep(5000);
                }
            });
        }

       

    }

    public static class Extensions
    {
        public static string SerializeObject<T>(this T toSerialize)
        {

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false); // no BOM in a .NET string
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {
                    xmlSerializer.Serialize(writer, toSerialize, ns);
                    return stringWriter.ToString();
                }
            }

        }

        public static byte[] SerializeObjectInByteArray<T>(this T toSerialize)
        {

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false); // no BOM in a .NET string
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {
                    xmlSerializer.Serialize(writer, toSerialize, ns);
                    return Encoding.ASCII.GetBytes(stringWriter.ToString());
                }
            }


        }

        public static T DeserializeFromString<T>(this string toDesrialize) where T : class
        {
            try
            {
                using (TextReader reader = new StringReader(toDesrialize))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    return ser.Deserialize(reader) as T;
                }
            }

            catch (Exception ex)
            {
                return default(T); // is this really the right approach?  Just ignore the error and silently return null?
            }
        }
    }




}
