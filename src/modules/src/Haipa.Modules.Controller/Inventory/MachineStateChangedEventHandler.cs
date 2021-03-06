﻿using System;
using System.Threading.Tasks;
using Haipa.Messages.Resources.Machines.Events;
using Haipa.Resources.Machines;
using Haipa.StateDb;
using Haipa.StateDb.Model;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;

namespace Haipa.Modules.Controller.Inventory
{
    internal class MachineStateChangedEventHandler : IHandleMessages<VMStateChangedEvent>
    {
        private readonly StateStoreContext _stateStoreContext;

        public MachineStateChangedEventHandler(StateStoreContext stateStoreContext)
        {
            _stateStoreContext = stateStoreContext;
        }

        public async Task Handle(VMStateChangedEvent message)
        {
            var vm = await _stateStoreContext.VirtualMachines.FirstOrDefaultAsync(x=> x.VMId == message.VmId);

            if (vm == null)
                return;

            vm.Status = MapVmStatusToMachineStatus(message.Status);
        }

        private static MachineStatus MapVmStatusToMachineStatus(VmStatus status)
        {
            switch (status)
            {
                case VmStatus.Stopped:
                    return MachineStatus.Stopped;
                case VmStatus.Pending:
                    return MachineStatus.Pending;
                case VmStatus.Error:
                    return MachineStatus.Error;
                case VmStatus.Running:
                    return MachineStatus.Running;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}