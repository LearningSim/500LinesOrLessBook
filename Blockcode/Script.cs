using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcode
{
    public class Script
    {
        public event Action<Block> BeforeStep = delegate { };
        public event Action<Block> AfterStep = delegate { };
        private readonly List<Block> blocks;
        public static Dictionary<string, Func<Block, CancellationToken, Task>> Commands { get; set; }
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public Script(List<Block> blocks = null)
        {
            this.blocks = blocks;
        }

        public async Task Run()
        {
            foreach (var block in blocks)
            {
                await RunBlock(block, cts.Token);
                if (cts.Token.IsCancellationRequested) return;
            }
        }

        public void Stop() => cts.Cancel();

        private async Task RunBlock(Block block, CancellationToken token)
        {
            if (block.IsStub) return;

            BeforeStep(block);
            await Commands[block.Label](block, token);
            AfterStep(block);
        }

        public async Task Repeat(Block block, CancellationToken token)
        {
            var children = block.GetChildren();
            for (int i = 0; i < block.Value; i++)
            {
                if (await RunBlocks(children, token)) return;
            }
        }

        public async Task Forever(Block block, CancellationToken token)
        {
            var children = block.GetChildren();
            if(children.All(c => c.IsStub)) return;
            
            while (true)
            {
                if (await RunBlocks(children, token)) return;
                await Task.Delay(1, token);
            }
        }

        private async Task<bool> RunBlocks(IReadOnlyList<Block> blocks, CancellationToken token)
        {
            foreach (var child in blocks)
            {
                await RunBlock(child, token);
                if (token.IsCancellationRequested) return true;
            }

            return false;
        }

        public async Task Wait(Block block, CancellationToken token) => await Task.Delay(block.Value.Value, token);
    }
}