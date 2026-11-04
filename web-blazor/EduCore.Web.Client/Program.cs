using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddFluentUIComponents();
builder.Services.AddEduCoreApi(builder.Configuration);
builder.Services.AddScoped<AvisoNavegacao>();

await builder.Build().RunAsync();
