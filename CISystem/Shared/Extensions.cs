using System.Net.Sockets;
using System.Text;

namespace Shared;

public static class Extensions
{
    public static async Task<(ServerState State, string Body)> RequestAsync(this Socket socket, ServerCommand cmd,
        string? msg = null)
    {
        await socket.SendAsync(Encoding.UTF8.GetBytes($"{(int)cmd}:{msg}"), SocketFlags.None);
        return await socket.ReadResponseAsync();
    }

    public static async Task<(ServerCommand Command, string Body)> ReadRequestAsync(this Socket socket)
    {
        var result = (await ReadTextAsync(socket)).Split(':', 2);
        return ((ServerCommand)int.Parse(result[0]), result[1]);
    }

    public static async Task ResponseAsync(this Socket socket, ServerState state, string? msg = null) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes($"{(int)state}:{msg}"), SocketFlags.None);

    private static async Task<(ServerState State, string Body)> ReadResponseAsync(this Socket socket)
    {
        var result = (await ReadTextAsync(socket)).Split(':', 2);
        return ((ServerState)int.Parse(result[0]), result[1]);
    }

    private static async Task<string> ReadTextAsync(this Socket socket)
    {
        var buffer = new byte[1024];
        var msg = new StringBuilder();
        do
        {
            var received = await socket.ReceiveAsync(buffer, SocketFlags.None);
            msg.Append(Encoding.UTF8.GetString(buffer, 0, received));
        } while (socket.Available > 0);

        return msg.ToString();
    }

    public static (string?, string?) SplitByFirst(this string text, string separator)
    {
        var split = text.Split(separator, 2);
        return split.Length == 2 ? (split[0], split[1]) : (null, null);
    }

    public static (string?, string?) SplitByLast(this string text, string separator)
    {
        var idx = text.LastIndexOf(separator, StringComparison.InvariantCulture);
        return idx != -1 ? (text[..idx], text[(idx + 1)..]) : (null, null);
    }

    public static void DoNotAwait(this Task task)
    {
    }

    public static void DoNotAwait(this ValueTask task)
    {
    }
}