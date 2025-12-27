using System.Timers;
using IcyRain;
using Sunrise.Model.Common;
using Sunrise.Model.Communication;
using Timer = System.Timers.Timer;

namespace Sunrise.Model.Discovery;

public sealed class DiscoveryClient : DiscoveryChannel
{
    private readonly Dictionary<string, DiscoveryDeviceInfo> _deviceInfos = [];
    private Action<DiscoveryDeviceInfo> _callback;
    private Timer _broadcastTimer;
    private ArraySegment<byte> _message;

    private DiscoveryClient() { }

    public static IDisposable Search(Action<DiscoveryDeviceInfo> callback)
    {
        var client = new DiscoveryClient();
        client.RunSearch(SyncServiceManager.Port, callback);
        return client;
    }

    private void RunSearch(int port, Action<DiscoveryDeviceInfo> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        var services = GetCachedDevices();

        if (services?.Count > 0)
        {
            foreach (var service in services)
                _callback(service);
        }

        BeginListeningForBroadcasts();

        if (_message.Count > 0)
            BuffersPool<byte>.ReturnSegment(_message);

        var message = new SearchMessage()
        {
            Name = DiscoveryConsts.Name,
            DeviceName = Environment.MachineName,
            IPAddress = Network.GetMachineIPAddress().GetAddressBytes(),
            Port = port,
        };

        _message = Serialization.Serialize(message);

        // Таймер для оповещения поиска
        _broadcastTimer = new Timer(DiscoveryConsts.BroadcastTime);
        _broadcastTimer.Elapsed += BroadcastTimer_Elapsed;
        _broadcastTimer.Start();
    }

    private void BroadcastTimer_Elapsed(object? sender, ElapsedEventArgs e)
        => Send(_message); // Broadcast discovery

    private void DisposeBroadcastTimer()
    {
        if (_broadcastTimer is null)
            return;

        _broadcastTimer.Elapsed -= BroadcastTimer_Elapsed;
        _broadcastTimer.Stop();
        _broadcastTimer.Dispose();
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private IReadOnlyCollection<DiscoveryDeviceInfo>? GetCachedDevices()
    {
        lock (_deviceInfos)
        {
            if (_deviceInfos.Count == 0)
                return null;

            var keys = _deviceInfos.Keys;
            var deviceInfos = _deviceInfos.ToDictionary();

            foreach (string key in keys)
            {
                var deviceInfo = deviceInfos[key];

                if (DateTime.Now.Subtract(deviceInfo.CreationDate).TotalMinutes > 10)
                {
                    _deviceInfos.Remove(key);
                    deviceInfos.Remove(key);
                }
            }

            return deviceInfos.Values;
        }
    }
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    protected override void OnListenSendResponse(SearchMessage message)
    {
        if (message is not NotifyMessage notifyMessage)
            return;

        DiscoveryDeviceInfo deviceInfo;

        lock (_deviceInfos)
        {
            string deviceName = notifyMessage.DeviceName;

            if (_deviceInfos.TryGetValue(deviceName, out var existingDeviceInfo)
                && DateTime.Now.Subtract(existingDeviceInfo.CreationDate).TotalSeconds < 10)
            {
                return;
            }

            deviceInfo = new DiscoveryDeviceInfo(notifyMessage);
            _deviceInfos[deviceName] = deviceInfo;
        }

        try
        {
            var socket = GetSocket(out _);

            if (!IsDisposed)
                socket.SendTo(_message, deviceInfo.IPAddress);
        }
        catch { }

        _callback(deviceInfo);
    }

    protected override void OnDispose()
    {
        DisposeBroadcastTimer();
        BuffersPool<byte>.ReturnSegment(_message);
        base.OnDispose();
    }

}
