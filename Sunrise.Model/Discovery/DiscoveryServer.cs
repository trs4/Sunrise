using System.Timers;
using Sunrise.Model.Common;
using Timer = System.Timers.Timer;

namespace Sunrise.Model.Discovery;

public sealed class DiscoveryServer : DiscoveryChannel
{
    private readonly Dictionary<string, DiscoveryDeviceInfo> _deviceInfos = [];
    private readonly DiscoveryServerInfo _serverInfo;
    private readonly Action<DiscoveryDeviceInfo> _callback;
    private Timer _broadcastTimer;

    public DiscoveryServer(string deviceName, Action<DiscoveryDeviceInfo> callback)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            throw new ArgumentNullException(nameof(deviceName));

        _callback = callback ?? throw new ArgumentNullException(nameof(callback));

        var message = new NotifyMessage()
        {
            Name = DiscoveryConsts.Name,
            DeviceName = deviceName,
            IPAddress = Network.GetMachineIPAddress().GetAddressBytes(),
        };

        _serverInfo = new DiscoveryServerInfo(message);
    }

    public void Start() => Tasks.StartOnDefaultScheduler(OnStart);

    private void OnStart()
    {
        if (IsDisposed)
            return;

        BeginListeningForBroadcasts();
        SendNotification();

        _broadcastTimer = new Timer(DiscoveryConsts.BroadcastTime);
        _broadcastTimer.Elapsed += BroadcastTimer_Elapsed;
        _broadcastTimer.Start();
    }

    private void BroadcastTimer_Elapsed(object? sender, ElapsedEventArgs e)
        => SendNotification();
    
    private void SendNotification()
    {
        if (IsDisposed)
            return;

        Send(_serverInfo.Data);
    }

    private void SendNotificationByFilter(SearchMessage message)
    {
        if (message.DeviceName is not null && !Environment.MachineName.Equals(message.DeviceName, StringComparison.OrdinalIgnoreCase))
            return;

        Tasks.StartOnDefaultScheduler(SendNotification);
    }

    protected override void OnListenSendResponse(SearchMessage message)
    {
        if (message is NotifyMessage)
            return;

        SendNotificationByFilter(message);

        if (message.IPAddress is null || message.Port <= 0)
            return;

        DiscoveryDeviceInfo deviceInfo;

        lock (_deviceInfos)
        {
            string deviceName = message.DeviceName;

            if (_deviceInfos.TryGetValue(deviceName, out var existingDeviceInfo)
                && DateTime.Now.Subtract(existingDeviceInfo.CreationDate).TotalSeconds < 10)
            {
                return;
            }

            deviceInfo = new DiscoveryDeviceInfo(message);
            _deviceInfos[deviceName] = deviceInfo;
        }

        _callback(deviceInfo);
    }

}
