using MQTTnet;
using MQTTnet.Core.Protocol;
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
using MQTTnet.Core;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Threading;

namespace MQTT_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMqttServer mqttServer;
        public MainWindow()
        {
            InitializeComponent();

            // Configure MQTT server.
            mqttServer = new MqttFactory().CreateMqttServer(options =>
            {
                options.DefaultEndpointOptions.Port = 1884;
                //options.Storage = new RetainedMessageHandler();
                options.ConnectionValidator = c =>
                {
                    if (c.ClientId.Length < 10)
                    {
                        return MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
                    }

                    //if (c.Username != "mySecretUser")
                    //{
                    //    return MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    //}

                    //if (c.Password != "mySecretPassword")
                    //{
                    //    return MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    //}

                    return MqttConnectReturnCode.ConnectionAccepted;
                };
            });
            mqttServer.ApplicationMessageReceived += MqttServer_ApplicationMessageReceived;
            mqttServer.ClientConnected += MqttServer_ClientConnected;
        }

        private void MqttServer_ClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine(e.Client.ClientId);
        }

        private void MqttServer_ApplicationMessageReceived(object s, MqttApplicationMessageReceivedEventArgs e)
        {
            if(e.ApplicationMessage.Topic == "message")
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
                             txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###\n";
                             txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic + "\n";
                             txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload) + "\n";
                             txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel + "\n";
                             txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain + "\n";
                         })
                );
                //Console.WriteLine();
            }
        }

        public async void StartServer()
        {
            await mqttServer.StartAsync();
        }

        public async void StopServer()
        {
            await mqttServer.StopAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StopServer();
        }
    }


    // The implementation of the storage:
    // This code uses the JSON library "Newtonsoft.Json".
    public class RetainedMessageHandler : IMqttServerStorage
    {
        private const string Filename = "D:\\RetainedMessages.json";

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(messages));
            return Task.FromResult(0);
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            IList<MqttApplicationMessage> retainedMessages;
            if (File.Exists(Filename))
            {
                var json = File.ReadAllText(Filename);
                retainedMessages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
            }
            else
            {
                retainedMessages = new List<MqttApplicationMessage>();
            }

            return Task.FromResult(retainedMessages);
        }
        
    }

}
