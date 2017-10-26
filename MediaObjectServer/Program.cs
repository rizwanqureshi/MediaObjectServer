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


namespace MediaObjectServer
{
    class Program
    {
        static void Main(string[] args)
        {
            heartbeat hb = new heartbeat();
            ItemsChoiceType3[] itemsChoice = new ItemsChoiceType3[1];
            itemsChoice[0] = ItemsChoiceType3.heartbeat;


            object[] objects = new object[1];
            objects[0] = new heartbeat()
            {
                time = DateTime.Now.ToString()

            };


          Console.Write(  new mos()
            {
                changeDate = DateTime.Now.ToString(),
                version = "2.8",
                ItemsElementName = itemsChoice,
                Items = objects

            }.SerializeObject<mos>()
            );

            Console.Read();
        }


    }

    public static class Extensions
    {
        public static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
    }


}
