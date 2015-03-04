using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace SimpleUdpReciever
{
    class Program
    {
        static void Main(string[] args)
        {
            int localPort = 11000;
            IPEndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
            bool flag = false;

            for( int i = 0; i < args.Length; i++)
            {
                string cmd = args[i];
                string value;
                int tempInt;
                IPAddress tempAddress;
                

                switch (cmd)
                {
                    case "-lp":
                        value = GetValue(args, ref i);
                        if (int.TryParse(value, out tempInt))
                            localPort = tempInt;
                        break;
                    case "-rp":
                        value = GetValue(args, ref i);
                        if (int.TryParse(value, out tempInt))
                            remoteSender.Port = tempInt;
                        break;
                    case "-rh":
                        value = GetValue(args, ref i);
                        if (IPAddress.TryParse(value, out tempAddress))
                            remoteSender.Address = tempAddress;
                        else if (int.TryParse(value, out tempInt) && tempInt == 0)
                            remoteSender.Address = IPAddress.Any;
                        break;
                    case "-?":
                    default:
                        PrintHelpText();
                        flag = true;
                        break;
                }
            }

            // Exit application after help text is displayed
            if (flag)
                return;

            // Display some information
            Console.WriteLine("Welcome! Starting Upd receiving.");
            Console.WriteLine("Local port: " + localPort);
            Console.WriteLine("Remote ip: " + remoteSender.Address.ToString());
            Console.WriteLine("Remote port: " + remoteSender.Port);
            Console.WriteLine("Use '-?' to display help.");
            Console.WriteLine("Press any key to quit.");
            Console.WriteLine("*** Copyright Jesse 20141229 ***");
            Console.WriteLine("*** Version 1.0.1 ***\n");

            // Create UDP client
            UdpClient client = new UdpClient(localPort);
            UdpState state = new UdpState(client, remoteSender);
            // Start async receiving
            client.BeginReceive(new AsyncCallback(DataReceived), state);

            // Wait for any key to terminate application
            Console.ReadKey();
            client.Close();
        }

        private static DataReservoir dataReservoir = new DataReservoir();

        private static void DataReceived(IAsyncResult ar)
        {
            try
            {
                UdpClient c = (UdpClient)((UdpState)ar.AsyncState).c;
                IPEndPoint wantedIpEndPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
                IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = c.EndReceive(ar, ref receivedIpEndPoint);

                dataReservoir.AddProcessingDataByte(receiveBytes, 0, receiveBytes.Length);

                // Check sender
                bool isRightHost = (wantedIpEndPoint.Address.Equals(receivedIpEndPoint.Address)) || wantedIpEndPoint.Address.Equals(IPAddress.Any);
                bool isRightPort = (wantedIpEndPoint.Port == receivedIpEndPoint.Port) || wantedIpEndPoint.Port == 0;
                if (isRightHost && isRightPort)
                {
                    // Convert data to ASCII and print in console
                    //string receivedText = ASCIIEncoding.ASCII.GetString(receiveBytes);
                    //Console.Write(receivedText);
                }

                // Restart listening for udp data packages
                c.BeginReceive(new AsyncCallback(DataReceived), ar.AsyncState);
            }
            catch (Exception e)
            {
                Console.WriteLine("DataReceived" + e);
            }

        }


        private static string GetValue(string[] args, ref int i)
        {
            string value = String.Empty;
            if (args.Length >= i + 2)
            {
                i++;
                value = args[i];
            }
            return value;
        }

        private static void PrintHelpText()
        {
            Console.WriteLine("Simple Udp Receiver is an application that prints incoming data to screen.");
            Console.WriteLine("Data is converted to ASCII before printing.");
            Console.WriteLine("*** Copyright Jesse 20141229 ***");
            Console.WriteLine("*** Version 1.0.1 ***\n");
            Console.WriteLine("Command switches:");
            Console.WriteLine("-? : Displays this text.");
            Console.WriteLine("-lp : Set local receiving port. \"-lp 4001\" Default: 11000");
            Console.WriteLine("-rp : Set remote sender port. \"-rp 4001\" Default: 0 (Any port)");
            Console.WriteLine("-rh : Set remote sender ip. \"-rh 192.168.1.10\" Default: 0 (Any ip)");
            Console.WriteLine("\n Example of usage:\nSimpleUdpReciver.exe -lp 11000 -rh 192.168.10.10 -rp 4001");
        }
    }
}
