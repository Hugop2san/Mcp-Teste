using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using orquestraAPI.Pedidos.Infrastructure.Mcp;

namespace orquestraAPI.Pedidos.Api.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly McpApplyMethods _dispatcher;

        public McpController(McpApplyMethods dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> Handle([FromBody] McpRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Tool))
                return BadRequest(new { ok = false, error = "tool é obrigatório" });

            var result = await _dispatcher.DispatchAsync(request);

            return Ok(result);
        }













    }
}
