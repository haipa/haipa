﻿namespace HyperVPlus.Agent.Management.Data
{
    public sealed class VMRemoteFx3DVideoAdapterInfo : VirtualMachineDeviceInfo
    {

        public string MaximumScreenResolution { get; set; }

        public byte MaximumMonitors { get; set; }

        public ulong VRAMSizeBytes { get; set; }

    }
}