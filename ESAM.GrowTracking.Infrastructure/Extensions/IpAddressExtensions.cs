using System.Net;
using System.Net.Sockets;

namespace ESAM.GrowTracking.Infrastructure.Extensions
{
    public static class IpAddressExtensions
    {
        public static bool IsPrivate(this IPAddress ip)
        {
            ArgumentNullException.ThrowIfNull(ip);
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 10)
                    return true;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return true;
                if (bytes[0] == 192 && bytes[1] == 168)
                    return true;
                if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
                    return true;
                if (bytes[0] == 169 && bytes[1] == 254)
                    return true;
                return false;
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var bytes = ip.GetAddressBytes();
                if ((bytes[0] & 0xFE) == 0xFC)
                    return true;
                if (ip.IsIPv6LinkLocal)
                    return true;
                return false;
            }
            return false;
        }

        public static bool IsLoopback(this IPAddress ip)
        {
            ArgumentNullException.ThrowIfNull(ip);
            return IPAddress.IsLoopback(ip);
        }

        public static bool IsReserved(this IPAddress ip)
        {
            ArgumentNullException.ThrowIfNull(ip);
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 0)
                    return true;
                if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)
                    return true;
                if (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
                    return true;
                if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
                    return true;
                if (bytes[0] >= 224 && bytes[0] <= 239)
                    return true;
                if (bytes[0] >= 240)
                    return true;
                return false;
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0D && bytes[3] == 0xB8)
                    return true;
                if (bytes[0] == 0xFF)
                    return true;
                if (ip.Equals(IPAddress.IPv6Any))
                    return true;
                return false;
            }
            return false;
        }
    }
}