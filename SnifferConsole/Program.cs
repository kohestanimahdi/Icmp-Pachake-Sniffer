using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

namespace SnifferConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Retrieve the device list from the local machine
            //IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            //if (allDevices.Count == 0)
            //{
            //    Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
            //    return;
            //}

            //// Print the list
            //for (int i = 0; i != allDevices.Count; ++i)
            //{
            //    LivePacketDevice device = allDevices[i];
            //    Console.Write((i + 1) + ". " + device.Name);
            //    if (device.Description != null)
            //        Console.WriteLine(" (" + device.Description + ")");
            //    else
            //        Console.WriteLine(" (No description available)");
            //}

            ////2
            // Retrieve the interfaces list
            //IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            //// Scan the list printing every entry
            //for (int i = 0; i != allDevices.Count(); ++i)
            //    DevicePrint(allDevices[i]);

            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }
            //// Scan the list printing every entry
            //for (int i = 0; i != allDevices.Count(); ++i)
            //    DevicePrint(allDevices[i]);
            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                Console.Write((i + 1) + ". " + device.Name);
                if (device.Description != null)
                    Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }

            int deviceIndex = 0;
            do
            {
                Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                string deviceIndexString = Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

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
            //Retrieve the device list from the local machine

            //IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

                                     //if (allDevices.Count == 0)
                                     //{
                                     //    Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                                     //    return;
                                     //}

                                     //// Print the list
                                     //for (int i = 0; i != allDevices.Count; ++i)
                                     //{
                                     //    LivePacketDevice device = allDevices[i];
                                     //    Console.Write((i + 1) + ". " + device.Name);
                                     //    if (device.Description != null)
                                     //        Console.WriteLine(" (" + device.Description + ")");
                                     //    else
                                     //        Console.WriteLine(" (No description available)");
                                     //}

                                     //int deviceIndex = 0;
                                     //do
                                     //{
                                     //    Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                                     //    string deviceIndexString = Console.ReadLine();
                                     //    if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                                     //        deviceIndex < 1 || deviceIndex > allDevices.Count)
                                     //    {
                                     //        deviceIndex = 0;
                                     //    }
                                     //} while (deviceIndex == 0);

                                     //// Take the selected adapter
                                     //PacketDevice selectedDevice = allDevices[deviceIndex - 1];

                                     //// Open the device
                                     //using (PacketCommunicator communicator =
                                     //    selectedDevice.Open(65536,                                  // portion of the packet to capture
                                     //                                                                // 65536 guarantees that the whole packet will be captured on all the link layers
                                     //                        PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                     //                        1000))                                  // read timeout
                                     //{
                                     //    Console.WriteLine("Listening on " + selectedDevice.Description + "...");

                                     //    // Retrieve the packets
                                     //    Packet packet;
                                     //    do
                                     //    {
                                     //        PacketCommunicatorReceiveResult result = communicator.ReceivePacket(out packet);
                                     //        switch (result)
                                     //        {
                                     //            case PacketCommunicatorReceiveResult.Timeout:
                                     //                // Timeout elapsed
                                     //                continue;
                                     //            case PacketCommunicatorReceiveResult.Ok:
                                     //                Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" +
                                     //                                  packet.Length);
                                     //                break;
                                     //            default:
                                     //                throw new InvalidOperationException("The result " + result + " shoudl never be reached here");
                                     //        }
                                     //    } while (true);
                                     //}
        }
        private static void PacketHandler(Packet packet)
        {
            Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
        }

        private static void PacketHandler2(Packet packet)
        {
            Console.WriteLine("***************************************************************");
            // print timestamp and length of the packet
            Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);

            IpV4Datagram ip = packet.Ethernet.IpV4;
            UdpDatagram udp = ip.Udp;

            // print ip addresses and udp ports
            Console.WriteLine(ip.Source + ":" + udp.SourcePort + " -> " + ip.Destination + ":" + udp.DestinationPort);
        }

        private static void DevicePrint(IPacketDevice device)
        {
            // Name
            Console.WriteLine(device.Name);

            // Description
            if (device.Description != null)
                Console.WriteLine("\tDescription: " + device.Description);

            // Loopback Address
            Console.WriteLine("\tLoopback: " +
                              (((device.Attributes & DeviceAttributes.Loopback) == DeviceAttributes.Loopback)
                                   ? "yes"
                                   : "no"));

            // IP addresses
            foreach (DeviceAddress address in device.Addresses)
            {
                Console.WriteLine("\tAddress Family: " + address.Address.Family);

                if (address.Address != null)
                    Console.WriteLine(("\tAddress: " + address.Address));
                if (address.Netmask != null)
                    Console.WriteLine(("\tNetmask: " + address.Netmask));
                if (address.Broadcast != null)
                    Console.WriteLine(("\tBroadcast Address: " + address.Broadcast));
                if (address.Destination != null)
                    Console.WriteLine(("\tDestination Address: " + address.Destination));
            }
            Console.WriteLine();
        }
    }
}
