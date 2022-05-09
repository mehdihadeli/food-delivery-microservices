using BuildingBlocks.Abstractions.CQRS.Event;
using Store.Services.Customers;

namespace Customers.Api;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class TestController : ControllerBase
{
    private readonly IEventProcessor _eventProcessor;

    public TestController(IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await _eventProcessor.PublishAsync(new TestDomainEvent("data..."));

        return Ok("Hello World");
    }
}
