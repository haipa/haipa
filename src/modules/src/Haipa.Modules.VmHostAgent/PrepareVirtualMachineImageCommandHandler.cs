﻿using System;
using System.IO;
using System.Threading.Tasks;
using Haipa.Messages;
using Haipa.Messages.Operations;
using Haipa.Messages.Operations.Events;
using Haipa.Messages.Resources.Images.Commands;
using Haipa.Resources.Machines.Config;
using JetBrains.Annotations;
using Rebus.Bus;
using Rebus.Handlers;

namespace Haipa.Modules.VmHostAgent
{
    [UsedImplicitly]
    public class
        PrepareVirtualMachineImageCommandHandler : IHandleMessages<
            OperationTask<PrepareVirtualMachineImageCommand>>
    {
        private readonly IBus _bus;

        public PrepareVirtualMachineImageCommandHandler(IBus bus)
        {
            _bus = bus;
        }

        public Task Handle(OperationTask<PrepareVirtualMachineImageCommand> message)
        {
            try
            {
                if (message.Command.ImageConfig == null)
                    return _bus.Publish(
                        OperationTaskStatusEvent.Completed(message.OperationId, message.TaskId));

                var hostSettings = HostSettingsBuilder.GetHostSettings();
                var imageRootPath = Path.Combine(hostSettings.DefaultVirtualHardDiskPath, "Images");

                if (!Directory.Exists(imageRootPath))
                    Directory.CreateDirectory(imageRootPath);

                var imagePath = Path.Combine(imageRootPath,
                    $"{message.Command.ImageConfig.Name}\\{message.Command.ImageConfig.Tag}");

                if (Directory.Exists(imagePath))
                    return _bus.Publish(
                        OperationTaskStatusEvent.Completed(message.OperationId, message.TaskId));

                if (message.Command.ImageConfig.Source == MachineImageSource.Local)
                    throw new Exception("Image not found on local source.");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return _bus.Publish(OperationTaskStatusEvent.Failed(message.OperationId,
                    message.TaskId,
                    new ErrorData {ErrorMessage = ex.Message}));
            }
        }
    }
}