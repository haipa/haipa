﻿using System.Collections.Generic;

namespace Haipa.StateDb.Model
{
    public class VMHostMachine : Machine
    {
        public VMHostMachine()
        {
            MachineType = MachineType.VMHost;
        }

        public virtual List<VirtualMachine> VMs { get; set; }

        public string HardwareId { get; set; }
    }
}