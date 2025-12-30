using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace orquestraAPI.Pedidos.Infrastructure.Mcp
{

    public sealed class McpRequest
    {
        public string Tool { get; set; } = "";
        public JsonElement? Args { get; set; }
    }
    
}
