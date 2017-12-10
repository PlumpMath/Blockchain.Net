namespace BlockchainNet.Pipe
{
    using System.IO.Pipes;
    using System.Threading.Tasks;

    internal static class PipeHelper
    {
        internal static Task WaitForConnectionAsync(this NamedPipeServerStream serverPipe)
        {
            return Task.Factory.FromAsync(
                (cb, state) => serverPipe.BeginWaitForConnection(cb, state),
                ar => serverPipe.EndWaitForConnection(ar),
                null);
        }
    }
}
