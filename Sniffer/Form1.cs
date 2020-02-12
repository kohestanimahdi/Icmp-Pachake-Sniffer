using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sniffer
{
    public partial class Form1 : Form
    {
        DataTable InterFaces;
        IList<LivePacketDevice> allDevices;
        Thread thread;
        public Form1()
        {
            InitializeComponent();
            thread = new Thread(CapturePacket);

            InterFaces = new DataTable();
            InterFaces.Columns.Add("Id", typeof(int));
            InterFaces.Columns.Add("Description", typeof(string));
            allDevices = LivePacketDevice.AllLocalMachine;

            int id = 0;
            foreach (var device in allDevices)
            {
                string des = "";
                if (device.Description != null)
                    des += device.Description;

                foreach (DeviceAddress address in device.Addresses)
                {
                    if (address.Address != null && address.Address.Family == SocketAddressFamily.Internet)
                        des += (" - " + address.Address);
                }
                InterFaces.Rows.Add(id, des);
                id++;
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            CmbInterfaces.DataSource = InterFaces;
            CmbInterfaces.DisplayMember = "Description";
            CmbInterfaces.ValueMember = "Id";
        }

        private void Capture_Click(object sender, EventArgs e)
        {
            //if (Capture.Text.Equals("Capture"))
            //{
            //    thread.Start();
            //    Capture.Text = "Stop";
            //}else
            //{
            //    thread.Abort();
            //    Capture.Text = "Capture";
            //}
            Capture.Enabled = false;
            thread.Start();
        }

        private void CapturePacket()
        {


            int deviceIndex = 4;
            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex];

            // Open the device
            using (PacketCommunicator communicator =
                selectedDevice.Open(65536,                                  // portion of the packet to capture
                                                                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                Console.WriteLine("Listening on " + selectedDevice.Description + "...");
                using (BerkeleyPacketFilter filter = communicator.CreateFilter("icmp"))
                {
                    // Set the filter
                    communicator.SetFilter(filter);
                }
                // start the capture
                communicator.ReceivePackets(0, PacketHandler2);
            }
        }
        private void PacketHandler2(Packet packet)
        {
            // print timestamp and length of the packet
            richTextBox1.Text +=(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length+Environment.NewLine);

            IpV4Datagram ip = packet.Ethernet.IpV4;
            IcmpDatagram icmp = ip.Icmp;
            UdpDatagram udp = ip.Udp;

            // print ip addresses and udp ports
            richTextBox1.Text += (ip.Source + ":" + packet.Ethernet.IpV6.Source + " -> " + ip.Destination + ":" + packet.Ethernet.IpV6.Source + Environment.NewLine);
            richTextBox1.Text += ("************************************************"+ packet.Timestamp.Millisecond.ToString()+ Environment.NewLine);
            richTextBox1.Text += ("************************************************" + icmp.MessageType+Environment.NewLine);

            richTextBox1.Text += ("************************************************"  + Environment.NewLine);

        }
    }
}
