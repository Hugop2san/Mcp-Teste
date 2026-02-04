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

    public sealed class AiOrchestrator
    {
        private readonly McpApplyMethods _dispatcher;
        private readonly OpenAIClient _openAi; // do SDK oficial
        public AiOrchestrator(McpApplyMethods dispatcher, OpenAIClient openAi)
        {
            _dispatcher = dispatcher;
            _openAi = openAi;
        }

        public async Task<object> HandleNaturalLanguageAsync(string userText)
        {
            // 1) pede pra LLM escolher tool+args
            var selection = await SelectToolAsync(userText);

            // 2) usa seu pipeline atual (sem mudar nada)
            var req = new McpRequest { Tool = selection.Tool, Args = selection.Args };
            return await _dispatcher.DispatchAsync(req);
        }

        private async Task<AiToolSelection> SelectToolAsync(string userText)
        {
            // Aqui você configura tools/funções no request da LLM
            // e pede pra ela chamar uma dessas tools.
            // (A forma exata depende do SDK; conceito é esse.)

            var response = await _openAi.Responses.CreateAsync(new()
            {
                Model = "gpt-5.2", // exemplo
                Input = userText,
                Tools = new[]
                {
                Tool.Function(
                    name: "return_all",
                    description: "Retorna todos os produtos",
                    parametersJsonSchema: """
                    { "type": "object", "properties": {}, "additionalProperties": false }
                    """
                ),
                Tool.Function(
                    name: "return_by_id",
                    description: "Retorna um produto pelo id",
                    parametersJsonSchema: """
                    {
                      "type":"object",
                      "properties": { "id": { "type":"integer" } },
                      "required":["id"],
                      "additionalProperties": false
                    }
                    """
                ),
                Tool.Function(
                    name: "criar_produto",
                    description: "Cria um produto",
                    parametersJsonSchema: """
                    {
                      "type":"object",
                      "properties": {
                        "nome": { "type":"string" },
                        "preco": { "type":"number" },
                        "quantidade": { "type":"integer" }
                      },
                      "required":["nome","preco","quantidade"],
                      "additionalProperties": false
                    }
                    """
                ),
            },
                // Dica importante de segurança: você “força” a LLM a escolher tools,
                // em vez de responder texto livre.
                ToolChoice = "auto",
            });

            // A response vai conter o tool_call escolhido e seus argumentos.
            // Você converte isso pro seu AiToolSelection.

            var toolCall = response.GetFirstToolCall(); // pseudo
            return new AiToolSelection
            {
                Tool = toolCall.Name,
                Args = toolCall.ArgumentsAsJsonElement()
            };
        }
    }

}
