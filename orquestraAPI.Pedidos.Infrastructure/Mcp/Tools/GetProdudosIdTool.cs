using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace orquestraAPI.Pedidos.Infrastructure.Mcp.Tools
{
    public class GetProdudosIdTool
    {
        private readonly ProdutoService _produtoServiceid;

        public GetProdudosIdTool( ProdutoService produtoServiceid )
        {
            _produtoServiceid = produtoServiceid;
        }

        // Metodo pra retornar produto por id
        public async Task<object?> getallbyidmcp(int id)
        {
            var produtosid = _produtoServiceid.BuscarPorId(id) ;

            return new 
            {
                ok =    true,
                data =  produtosid,
                error = produtosid == null ? $"Produto {id} não encontrado" : null

            };
        }


    }
}
