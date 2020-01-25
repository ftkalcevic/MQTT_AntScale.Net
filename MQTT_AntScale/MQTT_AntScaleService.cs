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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using System.Configuration;

namespace MQTT_AntScale
{
    public partial class MQTT_AntScaleService : ServiceBase
    {
        MqttClient mqttClient;
        AntScale antScale;

        public MQTT_AntScaleService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Trace.TraceInformation("Service Starting");

            string host = ConfigurationManager.AppSettings["mqttBroker"];
            int port = int.Parse(ConfigurationManager.AppSettings["mqttBrokerPort"]);
            string antNeworkKey = ConfigurationManager.AppSettings["antNetworkKey"];

            mqttClient = new MqttClient(host, port, false, null, null, MqttSslProtocols.None);
            string clientId = Guid.NewGuid().ToString();
            mqttClient.Connect(clientId);

            antScale = new AntScale(antNeworkKey);
            antScale.NewReading += new AntScale.NewReadingDelegate(NewReading);
            antScale.Start();
        }

        protected override void OnStop()
        {
            Trace.TraceInformation("Service Stopping");
            antScale.Stop();
            mqttClient.Disconnect();
        }
        void NewReading(Weight w)
        {
            string json = w.MakeJSON();
            mqttClient.Publish("tele/scales/weight", Encoding.ASCII.GetBytes(json), 2, false);
            Trace.TraceInformation("Got Msg: " + json);
        }
    }
}
