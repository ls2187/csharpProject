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
using System.Windows.Shapes;

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;

namespace Communication
{
    /// <summary>
    /// Server.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Server : Window
    {
        TcpListener server;
        TcpClient client;
        NetworkStream stream;
        Thread request;
        IPAddress ip;
        int port;
        int count = 0;

        public Server()
        {
            InitializeComponent();
        }

        private void ServerSetBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                ip = IPAddress.Parse(ServerIPBox.Text);
                port = int.Parse(ServerPortBox.Text);

                count++;

                server = new TcpListener(ip, port);
                client = default(TcpClient);
                server.Start();
            
                request = new Thread(new ThreadStart(Communication));
                request.Start();
                
            }catch(Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void ServertxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            { 
                if (e.Key == Key.Enter)
                {
                    string ServerData = ServertxtBox.Text;
                    byte[] sendBuff = Encoding.UTF8.GetBytes(ServerData);
                
                    stream.Write(sendBuff, 0, sendBuff.Length);
                    ServerListBox.Items.Add("서버: " + ServerData);
                    ServertxtBox.Text = "";
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        
        private void Communication()
        {
            while (true)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    stream = client.GetStream();

                    Receive();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            }
        }

        public async void Receive()
        {
            while (true)
            {
                try
                {
                    byte[] receiveBuff = new byte[256];

                    Thread.Sleep(250);
                    //int nbytes = stream.Read(receiveBuff, 0, receiveBuff.Length);
                    Task<int> nbytes = stream.ReadAsync(receiveBuff, 0, receiveBuff.Length);
                    int inbytes = await nbytes;
                    string output = Encoding.UTF8.GetString(receiveBuff, 0, inbytes);
                    if (output != "")
                        DispatcherFuntion(output);

                    
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private void DispatcherFuntion(string msg)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ServerListBox.Items.Add("클라이언트: " + msg);
            }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            stream.Close();
            client.Close();
            server.Stop();
            request.Abort();
        }
    }
}
