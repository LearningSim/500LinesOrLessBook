using System.Net;
using System.Net.Sockets;
using Shared;

namespace Dispatcher;

public class Server
{
    public List<DnsEndPoint> Runners { get; } = new();
    public Dictionary<string, DnsEndPoint> DispatchedCommits { get; } = new();
    public List<string> PendingCommits { get; } = new();

    public bool IsDead { get; }

    public async Task Start(string host, int port)
    {
        using var server = new Socket(SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(Dns.GetHostEntry(host).AddressList.First(), port));
        server.Listen();
        Console.WriteLine($"serving on {server.LocalEndPoint}");
        
        while (true)
        {
            HandleConnection(await server.AcceptAsync()).DoNotAwait();
        }
    }

    private async Task HandleConnection(Socket socket)
    {
        while (true)
        {
            var (cmd, msg) = await socket.ReadRequestAsync();
            if (cmd == ServerCommand.Dispatch)
            {
                if (!Runners.Any())
                {
                    socket.ResponseAsync(ServerState.Error, "No runners are registered")
                        .DoNotAwait();
                    continue;
                }
                
                Console.WriteLine("going to dispatch");
                socket.ResponseAsync(ServerState.Success).DoNotAwait();
                DispatchTests(msg).DoNotAwait();
            }
            else if (cmd == ServerCommand.Register)
            {
                Console.WriteLine("register");
                var (host, port) = msg.SplitByLast(":");
                Runners.Add(new DnsEndPoint(host!, int.Parse(port!)));
                socket.ResponseAsync(ServerState.Success).DoNotAwait();
            }
            else if (cmd == ServerCommand.Results)
            {
                Console.WriteLine("got test results");
                var (commitId, tests) = msg.SplitByFirst(":");
                DispatchedCommits.Remove(commitId!);
                File.WriteAllText($"test_results/{commitId}", tests);
                socket.ResponseAsync(ServerState.Success).DoNotAwait();
            }
            else
            {
                socket.ResponseAsync(ServerState.Error, "Invalid command").DoNotAwait();
            }
        }
    }

    public async Task DispatchTests(string commit)
    {
        while (true)
        {
            foreach (var runner in Runners)
            {
                using var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(runner.Host, runner.Port);
                var response = await client.RequestAsync(ServerCommand.RunTest, commit);
                if (response.State != ServerState.Success) continue;
                
                Console.WriteLine($"adding id {commit}");
                DispatchedCommits[commit] = runner;
                PendingCommits.Remove(commit);
                return;
            }

            await Task.Delay(2000);
        }
    }
}