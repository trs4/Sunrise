using System.Net;
using Grpc.Core;
using Sunrise.Model.Communication.Data;
using Sunrise.Model.Discovery;

namespace Sunrise.Model.Communication;

public sealed class SyncServer : SyncService.Server
{
    private readonly SyncDispatcher _dispatcher;

    public SyncServer(SyncDispatcher dispatcher)
        => _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

    public override async Task Subscription(ConnectParameters request, IServerStreamWriter<SubscriptionTicket> response, ServerCallContext context)
    {
        var deviceInfo = new DiscoveryDeviceInfo(request.Name, new IPAddress(request.IPAddress), SyncServiceManager.Port);
        var waitEvent = _dispatcher.Initialize(deviceInfo, response, context.CancellationToken);
        await waitEvent.Task;
    }

    public override Task<Empty> TransferMediaLibrary(MediaLibraryData request, ServerCallContext context)
    {
        _dispatcher.SetMediaLibrary(request);
        return Task.FromResult(new Empty());
    }

}
