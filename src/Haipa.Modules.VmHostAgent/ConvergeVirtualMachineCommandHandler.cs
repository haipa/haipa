﻿using System;
using System.Threading.Tasks;
using HyperVPlus.Messages;
using HyperVPlus.VmConfig;
using HyperVPlus.VmManagement;
using HyperVPlus.VmManagement.Data;
using LanguageExt;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Transport;

// ReSharper disable ArgumentsStyleAnonymousFunction

namespace Haipa.Modules.VmHostAgent
{
    internal class ConvergeTaskRequestedEventHandler : IHandleMessages<ConvergeVirtualMachineRequestedEvent>
    {
        private readonly IPowershellEngine _engine;
        private readonly IBus _bus;
        private Guid _correlationid;

        public ConvergeTaskRequestedEventHandler(
            IPowershellEngine engine,
            IBus bus)
        {
            _engine = engine;
            _bus = bus;
        }

        public async Task Handle(ConvergeVirtualMachineRequestedEvent command)
        {
            _correlationid = command.CorellationId;
            var config = command.Config;

            Either<PowershellFailure, Seq<TypedPsObject<VirtualMachineInfo>>> BindableEnsureUnique(
                Seq<TypedPsObject<VirtualMachineInfo>> list) => EnsureUnique(list, config.Name);

            Task<Either<PowershellFailure, TypedPsObject<VirtualMachineInfo>>> BindableTaskEnsureCreated(
                Seq<TypedPsObject<VirtualMachineInfo>> list) => EnsureCreated(list, config, _engine);


            //Task<Either<PowershellFailure, TypedPsObject<VirtualMachineInfo>>> BindableConvergeVm(
            //    TypedPsObject<VirtualMachineInfo> vmInfo) => ConvergeVm(vmInfo, config, engine);


            var result = await GetVmInfo(config.Name, _engine)
                .BindAsync(BindableEnsureUnique)
                .BindAsync(BindableTaskEnsureCreated)
                .BindAsync(vmInfo => ConvergeVm(vmInfo, config, _engine)).ConfigureAwait(false);

            await result.MatchAsync(
                LeftAsync: HandleError,
                Right: r => r
            ).ConfigureAwait(false);

            await ProgressMessage("Converged").ConfigureAwait(false);


                //await _bus.SendLocal(result.ToEvent(command.CorellationId));

        }

        private async Task<Unit> ProgressMessage(string message)
        {
            using (var scope = new RebusTransactionScope())
            {
                await _bus.Send(new ConvergeVirtualMachineProgressEvent
                {
                    CorellationId = _correlationid,
                    Message = message
                }).ConfigureAwait(false);

                // commit it like this
                await scope.CompleteAsync().ConfigureAwait(false);
            }
            return Unit.Default;

        }

        private async Task<Either<PowershellFailure, Unit>> ConvergeVm(TypedPsObject<VirtualMachineInfo> vmInfo, VirtualMachineConfig config, IPowershellEngine engine)
        {
            var result = await Converge.Firmware(vmInfo, config, engine, ProgressMessage)
                .BindAsync(info => Converge.Cpu(info, config.Cpu, engine, ProgressMessage))
                .BindAsync(info => Converge.Disks(info, config.Disks?.ToSeq(), config, engine, ProgressMessage))

                .BindAsync(info => Converge.CloudInit(
                    info, config.Path, config.Hostname, config.Provisioning?.UserData, engine, ProgressMessage)).ConfigureAwait(false);

            //await Converge.Definition(engine, vmInfo, config, ProgressMessage).ConfigureAwait(false);

            config.Networks?.Iter(async (network) =>
                await Converge.Network(engine, vmInfo, network, config, ProgressMessage)
                    .ConfigureAwait(false));

            //await ProgressMessage("Generate Virtual Machine provisioning disk").ConfigureAwait(false);

            //await Converge.CloudInit(
            //    engine, config.Path,
            //    config.Hostname,
            //    config.Provisioning.UserData,
            //    vmInfo).ConfigureAwait(false);

            //}).ConfigureAwait(false);

            await ProgressMessage("Converged").ConfigureAwait(false);

            return result;
        }

        private Either<PowershellFailure, Seq<TypedPsObject<VirtualMachineInfo>>> EnsureUnique(Seq<TypedPsObject<VirtualMachineInfo>> list, string vmName)
        {

            if (list.Count > 1)
                return Prelude.Left(new PowershellFailure { Message = $"VM name '{vmName}' is not unique." });

            return Prelude.Right(list);
        }

        private static Task<Either<PowershellFailure, TypedPsObject<VirtualMachineInfo>>> EnsureCreated(Seq<TypedPsObject<VirtualMachineInfo>> list, VirtualMachineConfig config, IPowershellEngine engine)
        {
            return list.HeadOrNone().MatchAsync(
                None: () => Converge.CreateVirtualMachine(engine, config.Name,
                    config.Path,
                    config.Memory.Startup),
                Some: s => s
            );

        }

        private static Task<Either<PowershellFailure, Seq<TypedPsObject<VirtualMachineInfo>>>> GetVmInfo(string vmName,
            IPowershellEngine engine)
        {
            return engine.GetObjectsAsync<VirtualMachineInfo>(PsCommandBuilder.Create()
                .AddCommand("get-vm").AddArgument(vmName)
                //this a bit dangerous, because there may be other errors causing the 
                //command to fail. However there seems to be no other way except parsing error response
                .AddParameter("ErrorAction", "SilentlyContinue")
            );
        }

        private static async Task<Unit> HandleError(PowershellFailure failure)
        {
            return Unit.Default;
        }


    }
}