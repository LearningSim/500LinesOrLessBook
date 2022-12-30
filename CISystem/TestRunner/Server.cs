using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace TestRunner;

public class Server
{
    private readonly string repoFolder;
    public DnsEndPoint DispatcherServer { get; private set; }
    public double LastCommunication { get; private set; }

    public bool Busy { get; private set; }
    public bool IsDead { get; }

    public Server(string repoFolder)
    {
        this.repoFolder = repoFolder;
    }

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
            if (cmd == ServerCommand.Ping)
            {
                Console.WriteLine("pinged");
                LastCommunication = (double)DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                socket.ResponseAsync(ServerState.Success).DoNotAwait();
            }
            else if (cmd == ServerCommand.RunTest)
            {
                Console.WriteLine($"got runtest command: am I busy? {Busy}");
                if (Busy)
                {
                    socket.ResponseAsync(ServerState.Busy).DoNotAwait();
                }
                else
                {
                    socket.ResponseAsync(ServerState.Success).DoNotAwait();
                    Console.WriteLine("running");
                    Busy = true;
                    RunTests(msg);
                    Busy = false;
                }
            }
            else
            {
                socket.ResponseAsync(ServerState.Error, "Invalid command").DoNotAwait();
            }
        }
    }

    private void RunTests(string commit)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = "bash";
        psi.Arguments = $"update_repo.sh {repo}";
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = false;

        using var process = Process.Start(psi);
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
        var commit = await (process?.StandardOutput.ReadToEndAsync() ?? Task.FromResult(""));
        Console.WriteLine(commit);
    }
}