using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Net;
using log4net;
using log4net.Config;
using SimpleTCP;
using MOS;





namespace MediaObjectServer
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Queue<mos> mosRequestQueue = new Queue<mos>();

        static int messageID = 0;
        static void Main(string[] args)
        {

            // receive data from external source. this should be an event e.g. web hooks etc.

            //transform incoming event data to mos message and enqueue to message queue

            //peek packet and send it to mos device

            //receive response from mos device and dequeue message

            //start server to listen to incoming message from mos devices

           

            #region Start Server

            SimpleTcpServer server = new SimpleTcpServer();
            Task.Run(() =>
            {
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.DataReceived += Server_DataReceived;
                server.DelimiterDataReceived += Server_DelimiterDataReceived;
                server.StringEncoder = Encoding.BigEndianUnicode;
                server.Start(1900);
            });
            #endregion

            InitializedMosClient("10.69.70.101", 10640, "PROMPTER");
            InitializedMosClient("localhost", 1900, "PROMPTER");

            while (Console.ReadKey().Key == ConsoleKey.Q)
            {

            }



            //#region test region
            //var request = new mos()
            //    {
            //        ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.heartbeat },
            //        Items = new object[] { "PROMPTER", "NEURON", 1, new heartbeat() { time = DateTime.Now.ToString() } }

            //    }.SerializeObject<mos>();




            //Console.Read();
            //return;

            //var message = client.WriteLineAndGetReply(new mos()
            // {

            //     ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.roCreate },
            //     Items = new object[]
            //    {
            //        "PROMPTER","SHOFLO",2,
            //       new roCreate
            //       {
            //             roChannel= "Channel",
            //              roEdStart= DateTime.Now.ToString(),
            //              roEdDur= DateTime.Now.AddHours(1).ToString(),
            //               roID= "1",
            //                roSlug="Rundown Schedule",
            //                 story = new story[]
            //                 {
            //                     new story
            //                     {
            //                         storyID="11",
            //                         storyNum="1",
            //                         storySlug="Story 1",
            //                         item = new item[]{
            //                             new item{
            //                                 itemID="12",
            //                                 itemSlug="VIZ-GFX",
            //                                 mosID="PILOT",
            //                                 objID="1122",

            //                             }
            //                         }
            //                     },

            //                      new story
            //                     {
            //                         storyID="12",
            //                         storyNum="1",
            //                         storySlug="Story 2",
            //                         item = new item[]{
            //                             new item{
            //                                 itemID="12",
            //                                 itemSlug="VIZ-GFX",
            //                                 mosID="PILOT",
            //                                 objID="1122"
            //                             }
            //                         }
            //                     }
            //            }



            //        }
            //    }

            // }.SerializeObject<mos>(), TimeSpan.FromSeconds(1));

            //request = new mos()
            //{
            //    ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.roStorySend },
            //    Items = new object[]
            //    {
            //        "PROMPTER","SHOFLO",2,
            //       new roStorySend
            //       {
            //            storyID="11",
            //            storyNum="1",
            //            roID="1",
            //            storySlug="Story of the day with breaking news",
            //            storyBody= new storyBody()
            //             {
            //                  p = new p[]
            //                  {
            //                      new p
            //                      {
            //                          Text= new string[] {"this is angain a new story and should be taken very seiously another problem yet another problem"},
            //                          ItemsElementName=new ItemsChoiceType[]{ItemsChoiceType.storyPresenter,ItemsChoiceType.pi,ItemsChoiceType.storyPresenterRR},
            //                          Items = new object[] {"Chris", new pi{ Text= new string[] {"Smile Please"}},"10"}

            //                      }

            //                  }
            //             }

            //            }

            //        }
            //}.SerializeObject<mos>();

            //client.WriteLineAndGetReply(request, TimeSpan.FromSeconds(1));
            //Console.WriteLine(request);
            //// Console.WriteLine(message.MessageString);
            //client.Disconnect();
            //Console.Read();
            //message = client.WriteLineAndGetReply(new mos()
            //{

            //    ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.reqMachInfo },
            //    Items = new object[] { new reqMachInfo() }

            //}.SerializeObject<mos>(), TimeSpan.FromSeconds(1));
            //Console.WriteLine(new mos()
            //{
            //    ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.listMachInfo },
            //    Items = new object[] {
            //        new listMachInfo{
            //            manufacturer="NEURON",
            //            model="4.2",
            //            SupportedProfiles= new SupportedProfiles(){
            //                deviceType= SupportedProfilesDeviceType.NCS,
            //                mosProfile= new mosProfile[]{
            //                     new mosProfile{ number=1, Value=true},
            //                     new mosProfile{ number=2, Value=true},
            //                     new mosProfile{ number=3, Value=true},
            //                     new mosProfile{ number=4, Value=true},
            //                }}
            //        }
            //    }

            //}.SerializeObject<mos>());
            //Console.WriteLine(new mos()
            //{
            //    ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.listMachInfo },
            //    Items = new object[]
            //    {
            //        new listMachInfo
            //        {
            //            manufacturer="NEURON",
            //            model="4.2",
            //            SupportedProfiles= new SupportedProfiles(){
            //                deviceType= SupportedProfilesDeviceType.NCS,
            //                mosProfile= new mosProfile[]{
            //                     new mosProfile{ number=1, Value=true},
            //                     new mosProfile{ number=2, Value=true},
            //                     new mosProfile{ number=3, Value=true},
            //                     new mosProfile{ number=4, Value=true},
            //                }
            //            }
            //        }
            //    }

            //}.SerializeObject<mos>()); 
            //#endregion

        }

        #region server events
        private static void Server_DelimiterDataReceived(object sender, Message e)
        {
            throw new NotImplementedException();
        }

        private static void Server_DataReceived(object sender, Message e)
        {
            log.Info("-----RECEIVED FROM CLIENT----\n" + e.MessageString);

            e.ReplyLine(e.MessageString.ToLower());
        }

        private static void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            throw new NotImplementedException();
        }

        private static void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            log.Info("----CLIENT CONNECTIED FROM IP----\n" + e.Client.RemoteEndPoint);
        } 
        #endregion

        #region client events
        static void client_DelimiterDataReceived(object sender, Message e)
        {
            //  Console.WriteLine(e.MessageString);
        }

        static void client_DataReceived(object sender, Message e)
        {
            log.Info("------RECEIVED FROM SERVER----\n" + e.MessageString);
        } 
        #endregion

       

        static void InitializedMosClient(string host, int port, string mosId)
        {
            #region Start Client
            SimpleTcpClient client = null;
            Task.Run(() =>
            {
                client = new SimpleTcpClient();
                client.StringEncoder = Encoding.BigEndianUnicode;
                client.DataReceived += client_DataReceived;
                client.DelimiterDataReceived += client_DelimiterDataReceived;
                client.Connect(host, port);

            });
            #endregion

            #region Dequeue task
            Task.Run(() =>
            {
                while (true)
                {
                    if (mosRequestQueue.Count > 0)
                    {
                        var mosObj = mosRequestQueue.Peek();
                        if (client.TcpClient.Connected)
                        {
                            client.WriteLineAndGetReply(mosObj.SerializeObject(), TimeSpan.FromSeconds(1));
                            log.Info("--QUEUE COUNT--" + mosRequestQueue.Count);
                            mosRequestQueue.Dequeue();
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
            #endregion

            #region HeartBeat Task
          
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
                        log.Info("Queued");
                    }
                });

            
            #endregion

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
    }




}
