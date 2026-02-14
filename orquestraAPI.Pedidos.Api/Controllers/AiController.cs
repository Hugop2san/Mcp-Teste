using Microsoft.AspNetCore.Mvc;
using orquestraAPI.Pedidos.Infrastructure.Mcp;



// FAZE A INJEÇÃO DO SDK DA OPENAI e 


namespace orquestraAPI.Pedidos.Api.Controllers
{
    // POST /ai
    public sealed class AiController : ControllerBase
    {
        private readonly AiOrchestrator _ai;

        public AiController(AiOrchestrator ai) => _ai = ai;

        [HttpPost("ai")]
        public async Task<IActionResult> Chat([FromBody] string prompt)
        {
            var result = await _ai.HandleNaturalLanguageAsync(prompt);
            return Ok(result);
        }
    }

    

}
