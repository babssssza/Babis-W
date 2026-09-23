namespace BabisW.Execution
{
    public class PipeSync
    {
        private readonly PipeWrite Pipe;

        public PipeSync(string PipeName, int Timeout = 5000)
        {
            Pipe = new PipeWrite(PipeName, Timeout);
        }

        public T SendRequest<T>(string messageType, object data, int requestTimeoutMs = 5000)
        {
            return Pipe.SendRequest<T>(messageType, data, requestTimeoutMs);
        }

        public void Dispose()
        {
            Pipe.Dispose();
        }

        public void ResetConnection()
        {
            Pipe.ResetConnection();
        }
    }
}
