﻿using System.Threading;
using System.Threading.Tasks;
using Haipa.Modules.AspNetCore.ApiProvider;
using Haipa.Modules.AspNetCore.ApiProvider.Endpoints;
using Haipa.Modules.AspNetCore.ApiProvider.Handlers;
using Haipa.Modules.AspNetCore.ApiProvider.Model;
using Haipa.Modules.ComputeApi.Model.V1;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Haipa.Modules.ComputeApi.Endpoints.V1.VirtualDisks
{
    public class Get : GetResourceEndpoint<VirtualDisk, StateDb.Model.VirtualDisk>
    {

        public Get([NotNull] IGetRequestHandler<StateDb.Model.VirtualDisk> requestHandler, [NotNull] ISingleResourceSpecBuilder<StateDb.Model.VirtualDisk> specBuilder) : base(requestHandler, specBuilder)
        {
        }

        [HttpGet("virtualdisks/{id}")]
        [SwaggerOperation(
            Summary = "Get a Virtual Disk",
            Description = "Get a Virtual Disk",
            OperationId = "VirtualDisks_Get",
            Tags = new[] { "Virtual Disks" })
        ]
        [SwaggerResponse(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, "Success", typeof(VirtualDisk))]

        public override Task<ActionResult<VirtualDisk>> HandleAsync([FromRoute] SingleResourceRequest request, CancellationToken cancellationToken = default)
        {
            return base.HandleAsync(request, cancellationToken);
        }

    }
}
