﻿using System;
using System.Threading.Tasks;
using Haipa.Messages.Commands.OperationTasks;
using Haipa.Messages.Operations;
using Haipa.VmConfig;
using JetBrains.Annotations;
using LanguageExt.UnsafeValueAccess;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Haipa.Modules.Controller.Operations.Workflows
{
    [UsedImplicitly]
    internal class DestroyMachineSaga : OperationTaskWorkflowSaga<DestroyMachineCommand, DestroyMachineSagaData>,
        IHandleMessages<OperationTaskStatusEvent<RemoveVMCommand>>
    {
        private readonly IOperationTaskDispatcher _taskDispatcher;
        private readonly IVirtualMachineDataService _vmDataService;

        public DestroyMachineSaga(IBus bus, IOperationTaskDispatcher taskDispatcher, IVirtualMachineDataService vmDataService) : base(bus)
        {
            _taskDispatcher = taskDispatcher;
            _vmDataService = vmDataService;
        }

        protected override void CorrelateMessages(ICorrelationConfig<DestroyMachineSagaData> config)
        {
            base.CorrelateMessages(config);
            config.Correlate<OperationTaskStatusEvent<RemoveVMCommand>>(m => m.OperationId, d => d.OperationId);

        }


        public override async Task Initiated(DestroyMachineCommand message)
        {
            Data.MachineId = message.Resource.Id;
            var res = await _vmDataService.GetVM(Data.MachineId);
            var data = res.ValueUnsafe();
            await  _taskDispatcher.Send(new RemoveVMCommand
            {
                MachineId = Data.MachineId,
                VMId = data.VMId,
                OperationId = Data.OperationId,
                TaskId = Guid.NewGuid()

            });
        }

        public Task Handle(OperationTaskStatusEvent<RemoveVMCommand> message)
        {
            return FailOrRun(message, () =>
            {
                return Complete(new DestroyResourcesResponse
                {
                    DestroyedResources = new[] {new Resource(ResourceType.Machine, Data.MachineId) },
                    DetachedResources = new Resource[0]
                });
            });
        }
    }
}