using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SimpleTCP;


namespace MediaObjectServer
{
    public static class Settings
    {
        public static int SOCKET_TIMEOUT_MILLISECONDS { get; set;}
        public static string NCS_ID {get; set;}
        public static int SERVER_PORT { get; set; }        
        

        public static void LoadSettings()
        {
            SOCKET_TIMEOUT_MILLISECONDS = 1000;
            NCS_ID = "NEURON";
            SERVER_PORT = 10542;
        }

    }


  
}
