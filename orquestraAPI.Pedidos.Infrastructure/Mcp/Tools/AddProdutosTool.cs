using orquestraAPI.Pedidos.Application.DTOs;
using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Infrastructure.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orquestraAPI.Pedidos.Infrastructure.Mcp.Tools
{


    //  CRIAR ADDASYNC TOOL
    public class AddProdutosTool
    {
        private readonly ProdutoService _produtoService;

        public AddProdutosTool(ProdutoService produtoService)
        {
            _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
        }
        public async Task<object?> CriarProdutosAsync( ProdutoDTO dto) 
        {

            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var created= await _produtoService.CriarProduto(dto );

            if (created == null) throw new Exception("Produto nao foi criado");

            return new
            {
                ok = true,
                data = created,
                error = created == null ? $"Produto {dto.Nome} não criado." : null

            };
        }



    }
}
