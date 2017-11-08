using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mos.Entities;
using Mos.Middleware;


namespace Mos.App
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

                ncsServer.MosReceived += NcsServer_MosReceived; 
                ncsServer.Start();

                ncsServer.SendMosCommand(
                            new mos()
                            {
                                ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.mosID, ItemsChoiceType3.ncsID, ItemsChoiceType3.heartbeat },
                                Items = new object[] { "PROMPTER", "NCS", new heartbeat() { time = DateTime.Now.ToString() } }
                            });

            }

            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            while (Console.ReadKey().Key == ConsoleKey.Q) { }
        }

        private static void NcsServer_MosReceived(object sender, mos e)
        {
            log.Info("Message received from " + ((MosClient)sender).IP + " " + e.Items[2].ToString());
        }
    }
}
