﻿using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Haipa.Modules.Identity.Models.V1;
using Haipa.Modules.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

using static Microsoft.AspNetCore.Http.StatusCodes;


namespace Haipa.Modules.Identity.Endpoints.V1.Clients
{
    [Route("v{version:apiVersion}")]

    public class Update : BaseAsyncEndpoint
        .WithRequest<UpdateClientRequest>
        .WithResponse<Client>
    {
        private readonly IClientService<Client> _clientService;

        public Update(IClientService<Client> clientService)
        {
            _clientService = clientService;
        }


        [Authorize(Policy = "identity:clients:write:all")]
        [HttpPut("clients/{id}")]
        [SwaggerOperation(
            Summary = "Updates a client",
            Description = "Updates a client",
            OperationId = "Clients_Update",
            Tags = new[] { "Clients" })
        ]
        [SwaggerResponse(Status201Created, "Success", typeof(Client))]

        public override async Task<ActionResult<Client>> HandleAsync([FromRoute] UpdateClientRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var persistentClient = await _clientService.GetClient(request.Id);
            if (persistentClient == null)
                return NotFound($"client with id {request.Id} not found.");

            persistentClient.AllowedScopes = request.Client.AllowedScopes;
            persistentClient.Description = request.Client.Description;
            persistentClient.Name = request.Client.Name;

            await _clientService.UpdateClient(persistentClient);

            return Ok(persistentClient);

        }
    }


    public class UpdateClientRequest
    {
        [FromRoute(Name = "id")] public string Id { get; set; }
        [FromBody] public Client Client { get; set; }
    }

}
