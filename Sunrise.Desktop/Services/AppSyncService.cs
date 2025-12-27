using System.Threading.Tasks;
using IcyRain.Grpc.AspNetCore;
using IcyRain.Grpc.AspNetCore.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sunrise.Model.Common;
using Sunrise.Model.Communication;
using Sunrise.Services;

namespace Sunrise.Desktop.Services;

internal sealed class AppSyncService : IAppSyncService
{
    private WebApplication? _app;

    public void Start(SyncDispatcher dispatcher)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(dispatcher);
        builder.Services.AddGrpc();

        var app = _app = builder.Build();
        app.UseGrpcWeb();
        app.MapGrpcService<SyncServer>();
        Tasks.StartOnDefaultScheduler(Run, app);
    }

    private void Run(WebApplication app) => app.Run($"https://*:{SyncServiceManager.Port}");

    public ValueTask ShutdownAsync() => _app?.DisposeAsync() ?? default;
}
