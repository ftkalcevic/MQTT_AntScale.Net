using AntScaleLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using System.Configuration;

namespace MQTT_AntScale_Test
{
    class Program
    {
        static MqttClient mqttClient;


        static void Main(string[] args)
        {
            string host = ConfigurationManager.AppSettings["mqttBroker"];
            int port = int.Parse(ConfigurationManager.AppSettings["mqttBrokerPort"]);
            string antNeworkKey = ConfigurationManager.AppSettings["antNetworkKey"];

            mqttClient = new MqttClient(host,port,false,null,null,MqttSslProtocols.None);
            string clientId = Guid.NewGuid().ToString();
            mqttClient.Connect(clientId);

            AntScale a = new AntScale(antNeworkKey);
            a.NewReading += new AntScale.NewReadingDelegate(NewReading);
            a.Start();

            Console.WriteLine("Q to quit");
            bool bDone = false;
            while (!bDone)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "Q":
                    case "q":
                        {
                            // Quit
                            Console.WriteLine("Closing Channel");
                            bDone = true;
                            break;
                        }
                }
                System.Threading.Thread.Sleep(0);
            }
            a.Stop();
            mqttClient.Disconnect();
        }

        static void NewReading(Weight w)
        {
            string json = w.MakeJSON();
            mqttClient.Publish("tele/scales/weight", Encoding.ASCII.GetBytes(json),2,false);
            Console.WriteLine(json);
        }
    }
}
