using orquestraAPI.Pedidos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using orquestraAPI.Pedidos.Domain.Entities;


namespace orquestraAPI.Pedidos.Domain.Interfaces
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<Produto>> GetAll();
        Task<Produto?> GetById(int id); // permite retorno nulo
        Task<Produto> AddAsync(Produto produto); //retorna o objeto quando produto for cadastrado
        /*  
        
        ** Metodos simples **
        Task Update(Produto produto);
        Task Delete(int id);

        ** análises **
        Task<int> TotalProdutos();
        Task<Produto?> ProdutoMaisCaro(); // permite retorno nulo caso não haja produtos

        */
    }
}
