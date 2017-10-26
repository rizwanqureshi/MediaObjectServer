using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SimpleTCP;


namespace MediaObjectServer
{
    class Program
    {
        static void Main(string[] args)
        {


            SimpleTcpClient client = new SimpleTcpClient();
            client.StringEncoder = Encoding.BigEndianUnicode;
            client.DataReceived += client_DataReceived;
            client.DelimiterDataReceived += client_DelimiterDataReceived;
            client.Connect("localhost", 10541);



            var request = new mos()
              {
                  ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.heartbeat },
                  Items = new object[] { "PROMPTER", "SHOFLO", 1, new heartbeat() { time = DateTime.Now.ToString() } }

              }.SerializeObject<mos>();


            Console.WriteLine(request);
            var message = client.WriteLineAndGetReply(request, TimeSpan.FromSeconds(1));



            message = client.WriteLineAndGetReply(new mos()
            {

                ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.roCreate },
                Items = new object[] 
                { 
                    "PROMPTER","SHOFLO",2,
                   new roCreate
                   {
                         roChannel= "Channel",                         
                          roEdStart= DateTime.Now.ToString(),
                          roEdDur= DateTime.Now.AddHours(1).ToString(),
                           roID= "1",
                            roSlug="Rundown Schedule",
                             story = new story[]
                             {
                                 new story
                                 { 
                                     storyID="11",
                                     storyNum="1",
                                     storySlug="Story 1",                                       
                                     item = new item[]{
                                         new item{ 
                                             itemID="12",
                                             itemSlug="VIZ-GFX",
                                             mosID="PILOT",
                                             objID="1122",
                                              
                                         }
                                     }
                                 },

                                  new story
                                 { 
                                     storyID="12",
                                     storyNum="1",
                                     storySlug="Story 2", 
                                     item = new item[]{
                                         new item{ 
                                             itemID="12",
                                             itemSlug="VIZ-GFX",
                                             mosID="PILOT",
                                             objID="1122"  
                                         }
                                     }
                                 }
                        }

                       
                   
                    }
                }

            }.SerializeObject<mos>(), TimeSpan.FromSeconds(1));

            message = client.WriteLineAndGetReply(new mos()
            {

                ItemsElementName = new ItemsChoiceType3[4] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.messageID, ItemsChoiceType3.roStorySend },
                Items = new object[] 
                { 
                    "PROMPTER","SHOFLO",2,
                   new roStorySend
                   {
                        storyID="11",
                        storyNum="1",
                        roID="1",
                        storySlug="Story of the day with breaking news",
                         storyBody= new storyBody()
                         {
                              p = new p[]
                              {
                                  new p 
                                  {
                                      Text= new string[] {"this is angain a new story and should be taken very seiously another probelm yet another problem"},                                       
                                       
                                  }                              
                              }
                             
                         }
                        
                      
                        }

                       
                   
                    }


            }.SerializeObject<mos>(), TimeSpan.FromSeconds(1));





            // Console.WriteLine(message.MessageString);
            client.Disconnect();
            Console.Read();




            return;





            message = client.WriteLineAndGetReply(new mos()
            {

                ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.reqMachInfo },
                Items = new object[] { new reqMachInfo() }

            }.SerializeObject<mos>(), TimeSpan.FromSeconds(1));



            Console.WriteLine(new mos()
            {
                ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.listMachInfo },
                Items = new object[] { 
                    new listMachInfo{ 
                        manufacturer="Shoflo",
                        model="4.2", 
                        SupportedProfiles= new SupportedProfiles(){
                            deviceType= SupportedProfilesDeviceType.NCS, 
                            mosProfile= new mosProfile[]{
                                 new mosProfile{ number=1, Value=true},
                                 new mosProfile{ number=2, Value=true},
                                 new mosProfile{ number=3, Value=true},
                                 new mosProfile{ number=4, Value=true},
                            }}
                    } 
                }

            }.SerializeObject<mos>());




            Console.WriteLine(new mos()
            {
                ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.listMachInfo },
                Items = new object[] 
                { 
                    new listMachInfo
                    { 
                        manufacturer="Shoflo",
                        model="4.2", 
                        SupportedProfiles= new SupportedProfiles(){
                            deviceType= SupportedProfilesDeviceType.NCS, 
                            mosProfile= new mosProfile[]{
                                 new mosProfile{ number=1, Value=true},
                                 new mosProfile{ number=2, Value=true},
                                 new mosProfile{ number=3, Value=true},
                                 new mosProfile{ number=4, Value=true},
                            }
                        }
                    } 
                }

            }.SerializeObject<mos>());

        }

        static void client_DelimiterDataReceived(object sender, Message e)
        {
            //  Console.WriteLine(e.MessageString);
        }

        static void client_DataReceived(object sender, Message e)
        {
            Console.WriteLine("---------");
            Console.WriteLine(e.MessageString);
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


            //using (StringWriter textWriter = new StringWriter())
            //{
            //    xmlSerializer.Serialize(textWriter, toSerialize,ns);
            //    return textWriter.ToString();
            //}

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


            //using (StringWriter textWriter = new StringWriter())
            //{
            //    xmlSerializer.Serialize(textWriter, toSerialize,ns);
            //    return textWriter.ToString();
            //}

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
