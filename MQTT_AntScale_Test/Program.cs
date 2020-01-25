// MQTT_AntScale.Net - Ant+ Scale Device monitor and MQTT Message publisher
// 
// Copyright(C) 2020  Frank Tkalcevic (frank@franksworkshop.com.au)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

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
