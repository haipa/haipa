﻿using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Ardalis.Specification;
using Haipa.Modules.AspNetCore.ApiProvider.Handlers;
using Haipa.Modules.AspNetCore.ApiProvider.Model;
using Microsoft.AspNetCore.Mvc;

namespace Haipa.Modules.AspNetCore.ApiProvider.Endpoints
{
    [Route("v{version:apiVersion}")]
    public abstract class ListEndpoint<TRequest,TResult,TModel> : BaseAsyncEndpoint
        .WithRequest<TRequest>
        .WithResponse<ListResponse<TResult>> where TModel : class
        where TRequest : ListRequest
    {
        private readonly IListRequestHandler<TModel> _listRequestHandler;

        protected ListEndpoint(IListRequestHandler<TModel> listRequestHandler)
        {
            _listRequestHandler = listRequestHandler;
        }

        protected abstract ISpecification<TModel> CreateSpecification(TRequest request);


        public override Task<ActionResult<ListResponse<TResult>>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            return _listRequestHandler.HandleListRequest<TRequest,TResult>(request,CreateSpecification, cancellationToken);
        }
    }

}
