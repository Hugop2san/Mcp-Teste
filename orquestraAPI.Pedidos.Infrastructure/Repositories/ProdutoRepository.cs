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
            // instancia do banco de dados
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


        //  
        public async Task<Produto> AddAsync(Produto produto)
        {
            var apiModel = new ProdutoApiModel
            {
                nome = produto.Nome,
                preco = produto.Preco.ToString(),  
                quantidade = produto.Quantidade.ToString()
            };

            using var response = await _http.PostAsJsonAsync("produto", apiModel);

            // Garante que obtenhamos um status 2xx; caso contrário lançará HttpRequestException
            try
            {
                // Verifica se a resposta indica sucesso de 200 a 299
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                // Lê o corpo da resposta para incluir na mensagem de erro por conta do API retornar erros customizados
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro na chamada POST /produto: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}", ex);
            }

            //le o o conteudo da resposta do tipo json e trato-o como ProdutoApiModel   
            var created = await response.Content.ReadFromJsonAsync<ProdutoApiModel>();
            if (created is null)
                throw new InvalidOperationException("Resposta da API veio vazia ao criar produto.");

            //TryParse para evitar exceções de conversão.
            if (!int.TryParse(created.id, out var id))
                throw new FormatException($"Id retornado inválido: '{created.id}'");
            if (!decimal.TryParse(created.preco, out var preco))
                throw new FormatException($"Preço retornado inválido: '{created.preco}'");
            if (!int.TryParse(created.quantidade, out var quantidade))
                throw new FormatException($"Quantidade retornada inválida: '{created.quantidade}'");

            return new Produto
            {
                Id = id,
                Nome = created.nome,
                Preco = preco,
                Quantidade = quantidade
            };
        }



        // AJUSTAR OS OUTROS METODOS PARA BUSCAR NA API TAMBEM !!!!!!!!

        /* 
       
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
