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

using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Windows.Threading;

namespace Communication
{
    /// <summary>
    /// Client.xaml에 대한 상호 작용 논리
    /// </summary>
    
    public partial class Client : Window
    {
        TcpClient[] tcp = new TcpClient[5];
        NetworkStream[] ClientStream = new NetworkStream[5];
        Thread t1;
        int count = 0;
        //StreamWriter streamWriter;

        public Client()
        {
            InitializeComponent();
        }
        
        private void ClientSetBtn_Click(object sender, RoutedEventArgs e)
        {
            // (1) ip와 포트 설정, TCP connection이 자동으로 연결된다.
            try
            { 
                tcp[count] = new TcpClient(ClientIPBox.Text, int.Parse(ClientPortBox.Text));
                
            }
            catch (SocketException err)
            {
                MessageBox.Show("접속 실패: " + err.ToString());
                return;
            }

            MessageBox.Show("Connected");

            // (2) NetworkStream을 얻어옴 
            ClientStream[count] = tcp[count].GetStream();
            //스트림라이트를 쓸 때 사용
            //streamWriter = new StreamWriter(ClientStream);

            count++;

            t1 = new Thread(new ThreadStart(ReceiveMessages));
            t1.Start();

        }

        private void ReceiveMessages()
        {
            try
            { 
                while (true)
                { 
                    // 스트림으로부터 들어온 바이트 데이터 읽기
                    byte[] outbuf = new byte[256];
                
                    Thread.Sleep(500);

                    int nbytes = ClientStream[count-1].Read(outbuf, 0, outbuf.Length);
            
                    string output = Encoding.UTF8.GetString(outbuf, 0, nbytes);
                
                    if(output != "")
                        DispatcherFuntion(output);
                }
            }
            catch(Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void DispatcherFuntion(string msg)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ClientListBox.Items.Add("서버: " + msg);
            }));
        }
        
        private void ClienttxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string msg = ClienttxtBox.Text;

                    //스트림 형식으로 보내기
                    //streamWriter.WriteLineAsync(msg);
                    //streamWriter.Flush();

                    // (3) 스트림에 바이트 데이터를 실어서 서버로 전송
                    byte[] buff = Encoding.UTF8.GetBytes(msg);
                    ClientStream[count-1].Write(buff, 0, buff.Length);

                    ClientListBox.Items.Add("클라이언트: " + msg);
                
                    ClienttxtBox.Text="";
                }
            }catch(Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ClientStream[count - 1].Close();
            tcp[count - 1].Close();
            t1.Abort();
        }
    }
}