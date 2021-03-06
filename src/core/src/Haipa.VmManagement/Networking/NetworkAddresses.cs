﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using Haipa.Resources.Machines;
using Haipa.VmManagement.Data;

namespace Haipa.VmManagement.Networking
{
    public static class NetworkAddresses
    {
        public static string[] GetAddressesByFamily(IEnumerable<string> addresses, AddressFamily family)
        {
            return addresses.Where(a =>
            {
                var ipAddress = IPAddress.Parse(a);
                return ipAddress.AddressFamily == family;
            }).ToArray();
        }


        public static IEnumerable<string> AddressesAndSubnetsToSubnets(IReadOnlyList<string> ipAddresses,
            IReadOnlyList<string> netmasks)
        {
            for (var i = 0; i < ipAddresses.Count; i++)
            {
                var address = ipAddresses[i];
                var netmask = netmasks[i];
                if (netmask.StartsWith("/"))
                    yield return IPNetwork.Parse(address + netmask).ToString();
                else
                    yield return IPNetwork.Parse(address, netmask).ToString();
            }
        }
    }

    public static class VirtualNetworkQuery
    {
        public static MachineNetworkData[] GetNetworksByAdapters(Guid vmId,
            IEnumerable<IVMNetworkAdapterWithConnection> networkAdapters)
        {
            var scope = new ManagementScope(@"\\.\root\virtualization\v2");
            var resultList = new List<MachineNetworkData>();

            var networkNames = new List<string>();

            foreach (var networkAdapter in networkAdapters)
            {
                var guestNetworkId = networkAdapter.Id.Replace("Microsoft:", "Microsoft:GuestNetwork\\")
                    .Replace("\\", "\\\\");
                var obj = new ManagementObject();
                var path = new ManagementPath(scope.Path +
                                              $":Msvm_GuestNetworkAdapterConfiguration.InstanceID=\"{guestNetworkId}\"");

                obj.Path = path;
                obj.Get();

                var info = new MachineNetworkData
                {
                    Name = Networks.GenerateName(ref networkNames, networkAdapter),
                    IPAddresses = ObjectToStringArray(obj.GetPropertyValue("IPAddresses")),
                    DefaultGateways = ObjectToStringArray(obj.GetPropertyValue("DefaultGateways")),
                    DnsServers = ObjectToStringArray(obj.GetPropertyValue("DNSServers")),
                    DhcpEnabled = (bool) obj.GetPropertyValue("DHCPEnabled")
                };
                info.Subnets = AddressesAndSubnetsToSubnets(info.IPAddresses,
                    ObjectToStringArray(obj.GetPropertyValue("Subnets"))).ToArray();

                resultList.Add(info);
            }

            return resultList.ToArray();
        }

        private static IEnumerable<string> AddressesAndSubnetsToSubnets(IReadOnlyList<string> ipAddresses,
            IReadOnlyList<string> netmasks)
        {
            for (var i = 0; i < ipAddresses.Count; i++)
            {
                var address = ipAddresses[i];
                var netmask = netmasks[i];
                if (netmask.StartsWith("/"))
                {
                    yield return IPNetwork.Parse(address + netmask).ToString();
                }
                else
                {
                    if (netmask.IndexOf('.') == -1)
                        yield return IPNetwork.Parse($"{address}/{netmask}").ToString();
                    else
                    {
                        yield return IPNetwork.Parse(address, netmask).ToString();
                    }
                }

            }
        }

        private static string[] ObjectToStringArray(object value)
        {
            if (value != null && value is IEnumerable enumerable) return enumerable.Cast<string>().ToArray();

            return new string[0];
        }
    }
}