using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using orquestraAPI.Pedidos.Application.DTOs;
using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Domain.Entities;


namespace orquestraAPI.Pedidos.Api.Controllers
{
    [ApiController]
    //[Route("api/produtos")]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _service;

        public ProdutoController(ProdutoService service)
        {
            _service = service;
        }

        [HttpGet("buscar_todos")]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _service.BuscarTodos();
            return Ok(produtos);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Get(int id)
        {
            var produto = await _service.BuscarPorId(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }


        //  CRIAÇÃO DE PRODUTO
        [HttpPost("criar_produto")]
        public async Task<IActionResult> Create([FromBody] ProdutoDTO dto)
        {
            if(dto == null) 
                return BadRequest("Dados do produto são obrigatórios.");

            var created = await _service.CriarProduto(dto);

            if (created == null) 
                return BadRequest("Produto nao foi criado");

              // Retorna 201 Created com Location apontando para Get(id)
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        /*
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProdutoDTO dto)
        {
            await _service.Atualizar(id, dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Remover(id);
            return Ok();
        }


                ** análises **
        Task<int> TotalProdutos();
        Task<Produto?> ProdutoMaisCaro(); // permite retorno nulo caso não haja produtos


        */
    }
}
