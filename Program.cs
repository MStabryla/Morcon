using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Rewrite;
using VueCliMiddleware;
using Microsoft.AspNetCore.HttpOverrides;
using Morcon.Services;
using Morcon.Models;
using MorconConLib;

// BUILDER SECTION
var builder = WebApplication.CreateBuilder(args);

const int spaPort = 3001;

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var applicationUrl = builder.Configuration.GetValue<string>("applicationUrl");
if (!string.IsNullOrEmpty(applicationUrl))
{
    builder.WebHost.UseUrls([.. applicationUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim())]);
}

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddSignalR();

//BUILD SECTION
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//REDIRECTION TO Vue.js SPA CLIENT
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
    uwApp =>
    {
        if (!app.Environment.IsDevelopment())
        {
            string spaDistPath = Path.Combine(Directory.GetCurrentDirectory(), "Client");
            IFileProvider fileProvider = new PhysicalFileProvider(spaDistPath);
            
            // Rewrite any non-file path to index.html FIRST
            RewriteOptions rewriteOptions = new RewriteOptions().AddRewrite("^(?!.*\\.).*$", "/index.html", skipRemainingRules: true);
            uwApp.UseRewriter(rewriteOptions);
            
            uwApp.UseSpaStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = "",
            });
        }
        else
        {
            uwApp.UseSpa(spabuilder =>
            {
                spabuilder.Options.SourcePath = Path.Combine(".", "Client");
                spabuilder.UseVueCli(npmScript: "dev", port: spaPort, forceKill: true, regex: "ready in", https: false);
                spabuilder.Options.StartupTimeout = TimeSpan.FromSeconds(12);

            });
        }
    }
);

//REDIRECTION TO API CONTROLLERS
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"),
    uwApp =>
    {
        uwApp.Map("/api", mapApp =>
        {
            mapApp.UseRouting();
            mapApp.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });
    }
);

//MAP Websocket connection
app.MapHub<MorconHub>("/api/ws");

app.Run();