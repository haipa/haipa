﻿using Haipa.Messages.Operations.Commands;
using Haipa.Resources;

namespace Haipa.Messages.Resources.Machines.Commands
{
    [SendMessageTo(MessageRecipient.Controllers)]
    public class StartMachineCommand : IResourceCommand
    {
        public Resource Resource { get; set; }
    }
}