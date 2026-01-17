using orquestraAPI.Pedidos.Domain.Entities;
using orquestraAPI.Pedidos.Domain.Interfaces;
using orquestraAPI.Pedidos.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace orquestraAPI.Pedidos.Application.Services
{
    public class ProdutoService 
    {
        private readonly IProdutoRepository _repository;

        public ProdutoService(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Produto>> BuscarTodos()
        {
            return await _repository.GetAll();
        }

        public async Task<Produto?> BuscarPorId(int id)
        {
            return await _repository.GetById(id);
        }
        
        public async Task<Produto?> CriarProduto(ProdutoDTO dto)
        {
            // Tratamentos de erro
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nome)) throw new ArgumentException("Nome é obrigatório.", nameof(dto.Nome));
            if (dto.Preco < 0) throw new ArgumentOutOfRangeException(nameof(dto.Preco), "Preço não pode ser negativo.");
            if (dto.Quantidade < 0) throw new ArgumentOutOfRangeException(nameof(dto.Quantidade), "Quantidade não pode ser negativa.");

            var result = new Produto
            {
                Nome = dto.Nome,
                Preco = dto.Preco,
                Quantidade = dto.Quantidade
            };
            
            return await _repository.AddAsync(result);
        }

        /*
        public async Task Atualizar(int id, ProdutoDTO dto)
        {
            var produto = await _repository.GetById(id);
            if (produto == null) return;

            produto.Nome = dto.Nome;
            produto.Preco = dto.Preco;
            produto.Quantidade = dto.Quantidade;

            await _repository.Update(produto);
        }

        public async Task Remover(int id)
        {
            await _repository.Delete(id);
        }
        */
    }
}
