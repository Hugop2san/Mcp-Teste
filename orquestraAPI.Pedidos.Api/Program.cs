using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Domain.Interfaces;
using orquestraAPI.Pedidos.Infrastructure.Repositories;
using orquestraAPI.Pedidos.Infrastructure.Mcp;
using orquestraAPI.Pedidos.Infrastructure.Mcp.Tools;


var builder = WebApplication.CreateBuilder(args);

// ================================
// Registrando controllers
// ================================
builder.Services.AddControllers(); // registra todos os controllers no mesmo assembly

// ================================
// Configuração do Swagger/OpenAPI
// ================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // não precisamos de OpenApiInfo aqui

// ================================
// Injeção de dependência
// ================================
//builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddHttpClient<IProdutoRepository, ProdutoRepository>(client =>
{
    client.BaseAddress = new Uri("https://693a8f799b80ba7262ca6b6c.mockapi.io/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<McpApplyMethods>();

// DI de Tools do MCP
builder.Services.AddScoped<GetProdutosTool>();
builder.Services.AddScoped<GetProdudosIdTool>();


var app = builder.Build();

// ================================
// Pipeline HTTP
// ================================



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // ativa JSON do Swagger
    app.UseSwaggerUI();     // ativa interface do Swagger em /swagger
}


app.UseHttpsRedirection();

// Mapear os controllers automaticamente
app.MapControllers();

app.Run();
