﻿using System.Threading.Tasks;
using Haipa.Messages.Operations.Commands;
using Newtonsoft.Json;
using Rebus.Bus;

namespace Haipa.Modules.Controller.Operations
{
    internal class OperationTaskDispatcher : IOperationTaskDispatcher
    {
        private readonly IBus _bus;

        public OperationTaskDispatcher(IBus bus)
        {
            _bus = bus;
        }


        public Task Send(OperationTaskCommand message)
        {
            var commandJson = JsonConvert.SerializeObject(message);

            return _bus.SendLocal(
                new CreateNewOperationTaskCommand(
                    message.GetType().AssemblyQualifiedName,
                    commandJson, message.OperationId,
                    message.TaskId));
        }
    }
}