using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Sunrise.Model.Common;

public static class Network
{
    private static IPAddress? _machineIPAddress;

    public static IPAddress GetMachineIPAddress() => _machineIPAddress ??= FindMachineIPAddress();

    public static List<(IPAddress IPAddress, NetworkInterface NetworkInterface)> GetAvailableIPAddresses()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        var result = new List<(IPAddress IPAddress, NetworkInterface NetworkInterface)>(networkInterfaces.Length);

        foreach (var ni in networkInterfaces)
        {
            if (!(ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.OperationalStatus == OperationalStatus.Up))
                continue;

            var a = ni.GetIPProperties().UnicastAddresses.Select(c => c.Address)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault() ?? IPAddress.None;

            result.Add((a, ni));
        }

        return result;
    }

    private static IPAddress FindMachineIPAddress()
        => GetNetworkInterface()?.GetIPProperties().UnicastAddresses
        .Select(c => c.Address).Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault() ?? IPAddress.None;

    private static NetworkInterface? GetNetworkInterface()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.OperationalStatus == OperationalStatus.Up);

        var networkInterface = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? networkInterfaces.Where(ni => ni.GetPhysicalAddress().GetAddressBytes().Length > 0)
                .OrderByDescending(KeySelector)
                .FirstOrDefault(ni => !ni.Description.Contains("virtual", StringComparison.OrdinalIgnoreCase)) // Без виртуальных машин
            : networkInterfaces.FirstOrDefault(ni => ni.Description?.StartsWith("wlan", StringComparison.OrdinalIgnoreCase) ?? false); // Через Wi-Fi

        return networkInterface ?? networkInterfaces.FirstOrDefault();
    }

    private static int KeySelector(NetworkInterface networkInterface)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return 0;

        var address = networkInterface.GetIPProperties().UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork);
        return address is null ? 0 : (int)address.PrefixOrigin;
    }

    public static void ClearCache()
        => _machineIPAddress = null;
}
