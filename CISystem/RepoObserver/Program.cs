using System.Diagnostics;
using System.Net.Sockets;

namespace RepoObserver;

/// <summary>
/// This is the repo observer.
///It checks for new commits to the master repo, and will notify the dispatcher of
///the latest commit ID, so the dispatcher can dispatch the tests against this commit ID
/// </summary>
public class Program
{
    /// <summary/>
    /// <param name="repo">path to the repository this will observe</param>
    /// <param name="dispatchServer">dispatcher host:port</param>
    public static async Task Main(string repo, string dispatchServer = "localhost:8888")
    {
        var hostAndPort = dispatchServer.Split(":");
        var host = hostAndPort[0];
        var port = int.Parse(hostAndPort[1]);
        using var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(host, port);
        
        while (true)
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
            if(string.IsNullOrEmpty(commit)) return;

            var response = await client.SendMessageAsync(commit);
            if (response != "ok")
            {
                throw new Exception($"Could not dispatch the test: {response}");
            }
            
            Console.WriteLine("dispatched!");
            Thread.Sleep(5000);
        }
        
        client.Shutdown(SocketShutdown.Both);
    }
}