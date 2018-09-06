﻿using System;

namespace HyperVPlus.Messages
{
    public class AttachMachineToOperationCommand
    {
        public Guid OperationId { get; set; }
        public Guid MachineId { get; set; }
        public string AgentName { get; set; }

    }
}