﻿using System;
using Haipa.Messages.Operations;
using Haipa.VmConfig;
using JetBrains.Annotations;

namespace Haipa.Messages.Commands.OperationTasks
{
    [SendMessageTo(MessageRecipient.VMHostAgent)]
    public class UpdateVirtualMachineCommand : OperationTaskCommand, IHostAgentCommand
    {
        public MachineConfig Config { get; set; }
        public string AgentName { get; set; }

        public Guid MachineId { get; set; }

        public long NewStorageId { get; set; }

        public VirtualMachineMetadata MachineMetadata { get; set; }

    }

}