using PcapDotNet.Core;
using PcapDotNet.Packets;
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

namespace Sniffer.UI
{
    public partial class Main : Form
    {
        DataTable InterFaces;
        IList<LivePacketDevice> allDevices;
        DataTable DevicePings;
        Dictionary<int, DataTable> DevicePingsDetails;
        Thread thread;
        public Main()
        {
            InitializeComponent();
            SetInterfaces();
        }

        private void SetInterfaces()
        {
            InterFaces = new DataTable();
            InterFaces.Columns.Add("Id", typeof(int));
            InterFaces.Columns.Add("Description", typeof(string));
            allDevices = LivePacketDevice.AllLocalMachine;
            int id = 1;
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

        private void InitializeDatatypes()
        {
            DevicePings = new DataTable();
            DevicePingsDetails = new Dictionary<int, DataTable>();
            DevicePings.Columns.Add("Id", typeof(int));
            DevicePings.Columns.Add("SourceIP", typeof(string));
            DevicePings.Columns.Add("SourceMAC", typeof(string));
            DevicePings.Columns.Add("DestinationIP", typeof(string));
            DevicePings.Columns.Add("DestinationMAC", typeof(string));
            DevicePings.Columns.Add("AllSendPacket", typeof(int));
            DevicePings.Columns.Add("AllReceivePacket", typeof(int));
            DevicePings.Columns.Add("SuccessPercent", typeof(float));
            gridDevices.DataSource = null;
            gridPings.DataSource = null;
        }


        public void DevicePingHandler(Packet packet)
        {
            bool flg = false;
            for (int i = 0; i < DevicePings.Rows.Count; i++)
            {
                if (packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.Echo)
                {
                    if (DevicePings.Rows[i]["SourceIP"].ToString().Equals(packet.Ethernet.IpV4.Source.ToString())
                   && DevicePings.Rows[i]["DestinationIP"].ToString().Equals(packet.Ethernet.IpV4.Destination.ToString())
                   && DevicePings.Rows[i]["SourceMAC"].ToString().Equals(packet.Ethernet.Source.ToString())
                   && DevicePings.Rows[i]["DestinationMAC"].ToString().Equals(packet.Ethernet.Destination.ToString()))
                    {
                        DevicePings.Rows[i]["AllSendPacket"] = Int32.Parse(DevicePings.Rows[i]["AllSendPacket"].ToString()) + 1;
                        flg = true;
                        DevicePingHandler(Int32.Parse(DevicePings.Rows[i]["Id"].ToString()), packet);

                    }
                }
                else if (packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.EchoReply)
                {
                    if (DevicePings.Rows[i]["SourceIP"].ToString().Equals(packet.Ethernet.IpV4.Destination.ToString())
                   && DevicePings.Rows[i]["DestinationIP"].ToString().Equals(packet.Ethernet.IpV4.Source.ToString())
                   && DevicePings.Rows[i]["SourceMAC"].ToString().Equals(packet.Ethernet.Destination.ToString())
                   && DevicePings.Rows[i]["DestinationMAC"].ToString().Equals(packet.Ethernet.Source.ToString()))
                    {
                        DevicePings.Rows[i]["AllReceivePacket"] = Int32.Parse(DevicePings.Rows[i]["AllReceivePacket"].ToString()) + 1;
                        flg = true;
                        DevicePingHandler(Int32.Parse(DevicePings.Rows[i]["Id"].ToString()), packet);

                    }
                }
                if (Int32.Parse(DevicePings.Rows[i]["AllReceivePacket"].ToString()) != 0 && flg == true)
                {
                    DevicePings.Rows[i]["SuccessPercent"] = Int32.Parse(DevicePings.Rows[i]["AllReceivePacket"].ToString()) / Int32.Parse(DevicePings.Rows[i]["AllSendPacket"].ToString()) * 100;

                }
            }
            if (DevicePings.Rows.Count == 0 || flg == false)
            {
                if (packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.Echo)
                {
                    DevicePings.Rows.Add(DevicePings.Rows.Count + 1, packet.Ethernet.IpV4.Source.ToString(), packet.Ethernet.Source.ToString(), packet.Ethernet.IpV4.Destination.ToString(), packet.Ethernet.Destination.ToString(), 1, 0, 0);

                }
                else if (packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.EchoReply)
                {
                    DevicePings.Rows.Add(DevicePings.Rows.Count + 1, packet.Ethernet.IpV4.Source.ToString(), packet.Ethernet.Source.ToString(), packet.Ethernet.IpV4.Destination.ToString(), packet.Ethernet.Destination.ToString(), 0, 1, 0);


                }
                DevicePingHandler(DevicePings.Rows.Count, packet);

            }

        }

        public void DevicePingHandler(int id, Packet packet)
        {
            if (!DevicePingsDetails.Keys.Contains(id))
            {
                DevicePingsDetails.Add(id, CreateDetailDatatable());
            }
            DevicePingsDetails[id].Rows.Add(
                packet.Ethernet.IpV4.Source.ToString(),
                packet.Ethernet.Source.ToString(),
                packet.Ethernet.IpV4.Destination.ToString(),
                packet.Ethernet.Destination.ToString(),
                packet.Buffer.Length, 
                packet.Timestamp.Millisecond,
                packet.Ethernet.IpV4.Ttl,
                packet.Ethernet.IpV4.Icmp.MessageType);
        }

        public DataTable CreateDetailDatatable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SourceIP", typeof(string));
            dt.Columns.Add("SourceMAC", typeof(string));
            dt.Columns.Add("DestinationIP", typeof(string));
            dt.Columns.Add("DestinationMAC", typeof(string));
            dt.Columns.Add("Bytes", typeof(int));
            dt.Columns.Add("Time", typeof(int));
            dt.Columns.Add("TTL", typeof(int));
            dt.Columns.Add("Type", typeof(string));
            return dt.Copy();
        }


        private void Main_Shown(object sender, EventArgs e)
        {
            comboBoxInterfaces.DataSource = InterFaces;
            comboBoxInterfaces.DisplayMember = "Description";
            comboBoxInterfaces.ValueMember = "Id";
            InitializeDatatypes();

        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            


            if(BtnCapture.Text.Equals("Capture"))
            {
                SetGridDataSource();
                comboBoxInterfaces.Enabled = false;
                BtnCapture.Text = "Stop";
                labelDescription.ResetText();
                thread = new Thread(CapturePacket);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                BtnCapture.Text = "Capture";
                thread.Abort();
                comboBoxInterfaces.Enabled = true;
            }

        }
        public void SetGridDataSource()
        {
            gridDevices.DataSource = DevicePings;
            gridDevices.Columns["Id"].Visible = false;
            gridDevices.Columns["SourceIP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["SourceMAC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["DestinationIP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["DestinationMAC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["AllSendPacket"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["AllReceivePacket"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridDevices.Columns["SuccessPercent"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            gridDevices.Columns["SourceIP"].HeaderText = "Src IP";
            gridDevices.Columns["SourceMAC"].HeaderText = "Src MAC";
            gridDevices.Columns["DestinationIP"].HeaderText = "Dst Ip";
            gridDevices.Columns["DestinationMAC"].HeaderText = "Dst MAC";
            gridDevices.Columns["AllSendPacket"].HeaderText = "Send Packets";
            gridDevices.Columns["AllReceivePacket"].HeaderText = "Receive Packets";
            gridDevices.Columns["SuccessPercent"].HeaderText = "%";
        }
        private void CapturePacket()
        {
            int deviceIndex = 0;
            try
            {
                deviceIndex = Int32.Parse(comboBoxInterfaces.SelectedValue.ToString());
            }
            catch
            {

            }


            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

            // Open the device
            using (PacketCommunicator communicator =
                selectedDevice.Open(65536,                                  // portion of the packet to capture
                                                                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                labelDescription.Text = "Listening on " + InterFaces.Rows[deviceIndex-1]["Description"] + "...";
                using (BerkeleyPacketFilter filter = communicator.CreateFilter("icmp"))
                {
                    // Set the filter
                    communicator.SetFilter(filter);
                }
                // start the capture
                communicator.ReceivePackets(0, DevicePingHandler);
            }
        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //thread.Abort();
            //Application.ExitThread();
        }

        private void gridDevices_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gridDevices.CurrentRow != null)
                {
                    gridPings.DataSource = DevicePingsDetails[int.Parse(gridDevices.CurrentRow.Cells["Id"].Value.ToString())];
                    gridPings.Columns["SourceIP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["SourceMAC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["DestinationIP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["DestinationMAC"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["Bytes"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["TTL"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    gridPings.Columns["Bytes"].Visible = false;
                    gridPings.Columns["SourceIP"].HeaderText = "Src Ip";
                    gridPings.Columns["SourceMAC"].HeaderText = "Src MAC";
                    gridPings.Columns["DestinationIP"].HeaderText = "Dst Ip";
                    gridPings.Columns["DestinationMAC"].HeaderText = "Dst MAC";

                }
            }
            catch { }
        }

        private void gridDevices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            gridDevices_SelectionChanged(null, null);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            InitializeDatatypes();
            SetGridDataSource();
        }
    }

}
