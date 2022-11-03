using System.Net.Sockets;
using System.Text;

namespace RepoObserver;

public static class Extensions
{
    public static async Task<string> SendMessageAsync(this Socket socket, string msg)
    {
        await socket.SendAsync(Encoding.UTF8.GetBytes(msg), SocketFlags.None);
        var buffer = new byte[1024];
        int received;
        var result = new StringBuilder();
        do
        {
            received = await socket.ReceiveAsync(buffer, SocketFlags.None);
            result.Append(Encoding.UTF8.GetString(buffer, 0, received));
        } while (received > 0);

        return result.ToString();
    }
}