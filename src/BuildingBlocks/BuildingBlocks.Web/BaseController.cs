using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web;

[ApiController]
public abstract class BaseController : Controller
{
    protected const string BaseApiPath = Constants.BaseApiPath;
    private IMapper? _mapper;

    private IMediator _mediator;
    private ICommandBus _commandBus;
    private IQueryBus _queryBus;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

    protected IQueryBus? QueryProcessor => _queryBus ??= HttpContext.RequestServices.GetService<IQueryBus>()!;

    protected ICommandBus CommandBus => _commandBus ??= HttpContext.RequestServices.GetService<ICommandBus>()!;

    protected IMapper Mapper => (_mapper ??= HttpContext.RequestServices.GetService<IMapper>())!;
}
