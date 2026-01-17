using Microsoft.AspNetCore.Mvc;

using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Application.DTOs;
using orquestraAPI.Pedidos.Domain.Entities;


namespace orquestraAPI.Pedidos.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _service;

        public ProdutoController(ProdutoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _service.BuscarTodos();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var produto = await _service.BuscarPorId(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }


        //  REALIZAR MANUNTENÇÃO NESSE METODO DE CRIAÇÃO DE PRODUTO
        [HttpPost]
        public async Task<IActionResult> Create(ProdutoDTO dto)
        {
            await _service.Criar(dto);
            return Ok();
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
        */
    }
}
