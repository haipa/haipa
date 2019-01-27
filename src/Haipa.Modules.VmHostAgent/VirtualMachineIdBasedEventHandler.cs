﻿using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Haipa.Messages;
using Haipa.VmManagement;
using Haipa.VmManagement.Data;
using LanguageExt;
using Rebus.Bus;
using Rebus.Handlers;
// ReSharper disable ArgumentsStyleAnonymousFunction

namespace Haipa.Modules.VmHostAgent
{
    internal abstract class MachineOperationHandlerBase<T> : IHandleMessages<AcceptedOperation<T>> where T: OperationCommand, IMachineCommand
    {
        private readonly IPowershellEngine _engine;
        private readonly IBus _bus;

        protected MachineOperationHandlerBase(IBus bus, IPowershellEngine engine)
        {
            _bus = bus;
            _engine = engine;
        }

        protected abstract Task<Either<PowershellFailure, TypedPsObject<VirtualMachineInfo>>> HandleCommand(
            TypedPsObject<VirtualMachineInfo> vmInfo, T command, IPowershellEngine engine);

        public async Task Handle(AcceptedOperation<T> message)
        {
            var command = message.Command;

            var result = await GetVmInfo(command.MachineId, _engine)
                .BindAsync(optVmInfo =>
                {
                    return optVmInfo.MatchAsync(
                        Some: s => HandleCommand(s, command, _engine),
                        None: () => new TypedPsObject<VirtualMachineInfo>(new PSObject(new {Id = message.Command.MachineId})));
                }).ConfigureAwait(false);
            
            await result.MatchAsync(
                LeftAsync: f => HandleError(f,command),
                RightAsync: async vmInfo =>
                {
                    await _bus.Send(new OperationCompletedEvent
                    {
                        OperationId = command.OperationId,

                    }).ConfigureAwait(false);

                    return Unit.Default;
                }).ConfigureAwait(false);
        }

        private async Task<Unit> HandleError(PowershellFailure failure, OperationCommand command)
        {
            await _bus.Send(new OperationFailedEvent(){
                OperationId = command.OperationId,
                ErrorMessage = failure.Message

            }).ConfigureAwait(false);

            return Unit.Default;
        }

        private Task<Either<PowershellFailure, Option<TypedPsObject<VirtualMachineInfo>>>> GetVmInfo(Guid vmId,
            IPowershellEngine engine)
        {
            return engine.GetObjectsAsync<VirtualMachineInfo>(CreateGetVMCommand(vmId)).MapAsync(seq => seq.HeadOrNone());
        }

        protected virtual PsCommandBuilder CreateGetVMCommand(Guid vmId)
        {
            return PsCommandBuilder.Create()
                .AddCommand("get-vm").AddParameter("Id", vmId);
        }
    }
}