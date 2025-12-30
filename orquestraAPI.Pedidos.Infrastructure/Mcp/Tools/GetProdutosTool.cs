using System.Threading.Tasks;
using orquestraAPI.Pedidos.Application.Services;

namespace orquestraAPI.Pedidos.Infrastructure.Mcp.Tools
{
    public class GetProdutosTool
    {
        private readonly ProdutoService _produtoService;

        public GetProdutosTool(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public async Task<object> ExecuteAsync()
        {
            var produtos = await _produtoService.BuscarTodos();

            return new
            {
                ok = true,
                data = produtos
            };
        }
    }
}
