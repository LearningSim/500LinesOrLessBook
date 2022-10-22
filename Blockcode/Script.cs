using System;
using System.Collections.Generic;
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
                if(cts.Token.IsCancellationRequested) return;
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
            for (int i = 0; i < block.Value; i++)
            {
                foreach (Block child in block.Children)
                {
                    await RunBlock(child, token);
                    if(token.IsCancellationRequested) return;
                }
            }
        }
    }
}