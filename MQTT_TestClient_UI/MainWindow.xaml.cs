using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MQTT_TestClient_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMqttClient mqttClient;
        public MainWindow()
        {
            InitializeComponent();
            // Create a new MQTT client.
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            mqttClient.Connected += async (s, e) =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("Apple").Build());

                Console.WriteLine("### SUBSCRIBED ###");
            };
            mqttClient.ApplicationMessageReceived += MqttServer_ApplicationMessageReceived;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Use WebSocket connection.
            var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer("localhost:55888/mqtt")
                //.WithTcpServer("127.0.0.1", 1884)
                .Build();

            await mqttClient.ConnectAsync(options);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic(txtTopic.Text)
                            .WithPayload(txtMessage.Text)
                            .WithExactlyOnceQoS()
                            .WithRetainFlag()
                            .Build();

            await mqttClient.PublishAsync(message);
        }

        private void MqttServer_ApplicationMessageReceived(object s, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Topic == "Apple")
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
                Application.Current.Dispatcher.BeginInvoke(
                         DispatcherPriority.Background,
                         new Action(() =>
                         {
                             txtMessage.Text = "";
                             txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
                             txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
                             txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                             txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
                             txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
                         })
                );
                //Console.WriteLine();
            }
        }
    }
}
