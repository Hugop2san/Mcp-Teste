using System.Net.Http.Json;
using orquestraAPI.Pedidos.Domain.Entities;
using orquestraAPI.Pedidos.Domain.Interfaces;
using orquestraAPI.Pedidos.Infrastructure.ExternalModels;
using System.Linq;



namespace orquestraAPI.Pedidos.Infrastructure.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {

        // INSTANCIA, SERIALIZAÇÃO DOS DADOS DA API, ETC
        private readonly HttpClient _http;
        public ProdutoRepository(HttpClient http)
        {
            _http = http;
        }


        //METODOS INTERFACES COM DADOS DA API 
        public async Task<IEnumerable<Produto>> GetAll()
        {
            var apiResponse = await _http.GetFromJsonAsync<List<ProdutoApiModel>>(
                "produto"
                );

            if (apiResponse is null) return Enumerable.Empty<Produto>();


            return apiResponse.Select(
                p => new Produto
                {
                    Id = int.Parse(p.id),
                    Nome = p.nome,
                    Preco = decimal.Parse(p.preco),
                    Quantidade = int.Parse(p.quantidade)
                }
                );
        }

        //METODOS INTERFACES COM DADOS DA API 
        public async Task<Produto?> GetById(int id)
        {
            var apiResponse = await _http.GetFromJsonAsync<ProdutoApiModel>(
                $"/produto/{id}"
                );

            if (apiResponse == null) return null;


            return new Produto
                {
                    Id = int.Parse(apiResponse.id),
                    Nome = apiResponse.nome,
                    Preco = decimal.Parse(apiResponse.preco),
                    Quantidade = int.Parse(apiResponse.quantidade)
                };
        }


        //  CONSERTAR !
        public Task<Produto> AddAsync(Produto produto)
        {
            var apiModel = new ProdutoApiModel
            {
                nome = produto.Nome,
                preco = produto.Preco.ToString(),
                quantidade = produto.Quantidade.ToString()
            };


            var response = await _http.PostAsJsonAsync("produto", apiModel);

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<ProdutoApiModel>();

            return new Produto
            {
                Id = int.Parse(created.id),
                Nome = created.nome,
                Preco = decimal.Parse(created.preco),
                Quantidade = int.Parse(created.quantidade)
            };
        }



        // AJUSTAR OS OUTROS METODOS PARA BUSCAR NA API TAMBEM !!!!!!!!

        /* 
        public Task<Produto?> GetById(int id)
        {

            return Task.FromResult(_produtos.FirstOrDefault(p => p.Id == id));
        }


        public Task Update(Produto produto)
        {
            var existing = _produtos.First(p => p.Id == produto.Id);
            existing.Nome = produto.Nome;
            existing.Preco = produto.Preco;
            existing.Quantidade = produto.Quantidade;

            return Task.CompletedTask;
        }

        public Task Delete(int id)
        {
            var p = _produtos.FirstOrDefault(x => x.Id == id);
            if (p != null)
                _produtos.Remove(p);

            return Task.CompletedTask;
        }


        // Realizar a logica das analizes depois
        public Task<int> TotalProdutos()
        {
            throw new NotImplementedException();
        }

        public Task<Produto> ProdutoMaisCaro()
        {
            throw new NotImplementedException();
        }
        */
    }
}
