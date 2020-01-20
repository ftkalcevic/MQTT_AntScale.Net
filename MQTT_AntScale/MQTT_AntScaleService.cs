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
