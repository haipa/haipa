﻿using System;
using Haipa.Messages.Operations.Commands;

namespace Haipa.Messages.Resources.Machines.Commands
{
    [SendMessageTo(MessageRecipient.VMHostAgent)]
    public class StartVMCommand : IVMCommand
    {
        public long MachineId { get; set; }
        public Guid VMId { get; set; }
    }
}