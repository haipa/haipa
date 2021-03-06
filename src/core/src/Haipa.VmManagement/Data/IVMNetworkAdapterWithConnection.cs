﻿namespace Haipa.VmManagement.Data
{
    public interface IVMNetworkAdapterWithConnection : IVMNetworkAdapterCore
    {
        string SwitchName { get; }
        VMNetworkAdapterVlanSetting VlanSetting { get; }
    }
}