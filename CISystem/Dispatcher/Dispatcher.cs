using System.Net;
using System.Net.Sockets;
using Shared;

namespace Dispatcher;

public class Dispatcher
{
    private static readonly Server server = new();

    /// <summary />
    /// <param name="host">dispatcher's host</param>
    /// <param name="port">dispatcher's port</param>
    public static async Task Main(string host = "localhost", int port = 8888)
    {
        CheckRunner().DoNotAwait();
        Redistribute().DoNotAwait();
        await server.Start(host, port);
    }

    private static async Task CheckRunner()
    {
        while (!server.IsDead)
        {
            await Task.Delay(1000);
            foreach (var runner in server.Runners)
            {
                using var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(runner.Host, runner.Port);
                var response = await client.RequestAsync(ServerCommand.Ping);
                if (response.State == ServerState.Success) continue;

                Console.WriteLine($"removing runner {runner}");
                ManageCommitLists(runner);
            }
        }
    }

    private static void ManageCommitLists(DnsEndPoint runner)
    {
        foreach (var (commit, assignedRunner) in server.DispatchedCommits)
        {
            if (!Equals(assignedRunner, runner)) continue;

            server.DispatchedCommits.Remove(commit);
            server.PendingCommits.Add(commit);
            break;
        }

        server.Runners.Remove(runner);
    }

    /// <summary>
    /// kick off tests that failed
    /// </summary>
    private static async Task Redistribute()
    {
        while (!server.IsDead)
        {
            foreach (var commit in server.PendingCommits)
            {
                Console.WriteLine($"running redistribute: [{string.Join(",", server.PendingCommits)}]");
                await server.DispatchTests(commit);
            }

            await Task.Delay(100);
        }
    }
}