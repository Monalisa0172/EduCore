using EduCore.Web.Client.Api;
using EduCore.Web.Client.Pages;
using EduCore.Web.Components;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

// Necessario para o prerender das paginas do Client que injetam os servicos da API.
builder.Services.AddEduCoreApi(builder.Configuration);
builder.Services.AddScoped<EduCore.Web.Client.Shared.AvisoNavegacao>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Endereco inexistente: devolve 404 com a pagina "nao encontrada" (e nao uma tela em branco).
app.UseStatusCodePagesWithReExecute("/nao-encontrado", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EduCore.Web.Client._Imports).Assembly);

app.Run();
