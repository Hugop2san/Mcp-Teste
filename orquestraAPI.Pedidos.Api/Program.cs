using orquestraAPI.Pedidos.Api.Controllers;
using orquestraAPI.Pedidos.Application.Services;
using orquestraAPI.Pedidos.Domain.Interfaces;
using orquestraAPI.Pedidos.Infrastructure.Mcp;
using orquestraAPI.Pedidos.Infrastructure.Mcp.Tools;
using orquestraAPI.Pedidos.Infrastructure.Repositories;

// PACOTE DA OPEN AI
using OpenAI;

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

// DI do MCP
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<McpApplyMethods>();
// DI tools
builder.Services.AddScoped<GetProdutosTool>();
builder.Services.AddScoped<GetProdudosIdTool>();
builder.Services.AddScoped<AddProdutosTool>();

// DI OpenAI com tratamento
var apiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("OpenAI:ApiKey não configurada.");

// DI do SDK oficial da OpenAI
builder.Services.AddSingleton(_ => new OpenAIClient(apiKey));

// DI do “Modo IA”
builder.Services.AddScoped<AiOrchestrator>();

// Di do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


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

//frontend
app.UseCors("frontend");
// Mapear os controllers automaticamente
app.MapControllers();

app.Run();
