using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BabisW.Execution
{
    public class PipeWrite
    {

        private readonly string PipeName;
        private readonly int Timeout;
        private NamedPipeClientStream Pipe;
        private readonly object Lock = new object();
        private bool Disposed = false;

        public PipeWrite(string pipeName, int timeoutMs = 5000)
        {
            PipeName = pipeName;
            Timeout = timeoutMs;
        }

        private void EnsureConnected()
        {
            lock (Lock)
            {
                if (Pipe == null || !Pipe.IsConnected)
                {
                    Disconnect();
                    Pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
                    Pipe.Connect(Timeout);
                    Pipe.ReadTimeout = Timeout;
                    Pipe.WriteTimeout = Timeout;
                    Console.WriteLine("connected");
                }
            }
        }

        public T SendRequest<T>(string messageType, object data, int requestTimeoutMs = 5000)
        {
            lock (Lock) // requests are sent one at a time
            {
                if (Disposed) throw new ObjectDisposedException(nameof(PipeWrite)); // unlikely case

                Exception lastError = null;
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        EnsureConnected();
                        Pipe.ReadTimeout = requestTimeoutMs;
                        Pipe.WriteTimeout = requestTimeoutMs;
                        var Req = new RequestMessage
                        {
                            MessageType = messageType,
                            Data = JsonConvert.SerializeObject(data)
                        };

                        WriteMessage(Pipe, Req);
                        var Res = ReadMessage(Pipe);

                        if (Res == null)
                        {
                            throw new InvalidDataException("The wrapper returned an empty response.");
                        }

                        if (!Res.Success)
                        {
                            throw new Exception($"The wrapper rejected the request: {Res.ErrorMessage}");
                        }

                        return JsonConvert.DeserializeObject<T>(Res.Data);
                    }
                    catch (Exception ex) when (
                        ex is IOException ||
                        ex is TimeoutException ||
                        ex is InvalidDataException ||
                        ex is EndOfStreamException ||
                        ex is JsonException)
                    {
                        lastError = ex;
                        Disconnect();
                        if (attempt < 2)
                        {
                            Thread.Sleep(250);
                        }
                    }
                }

                throw new InvalidOperationException(
                    "The Babis-W wrapper connection was lost. Restart injection and try again.",
                    lastError);
            }
        }


        private void WriteMessage(NamedPipeClientStream Me, RequestMessage Req)
        {
            var MessageJson = JsonConvert.SerializeObject(Req);
            var MessageBytes = Encoding.UTF8.GetBytes(MessageJson);
            var LengthBytes = BitConverter.GetBytes(MessageBytes.Length);

            Me.Write(LengthBytes, 0, LengthBytes.Length);
            Me.Write(MessageBytes, 0, MessageBytes.Length);
            Me.Flush();
        }

        private ResponseMessage ReadMessage(NamedPipeClientStream Me)
        {
            // Read message length
            var LenBuffer = new byte[4]; // sufficient
            ReadExactly(Me, LenBuffer, 4);
            var MessageLength = BitConverter.ToInt32(LenBuffer, 0);
            if (MessageLength <= 0 || MessageLength > 16 * 1024 * 1024)
            {
                throw new InvalidDataException("The wrapper returned an invalid message length.");
            }

            // Read message
            var MessageBuffer = new byte[MessageLength];
            ReadExactly(Me, MessageBuffer, MessageLength);

            var messageJson = Encoding.UTF8.GetString(MessageBuffer);
            return JsonConvert.DeserializeObject<ResponseMessage>(messageJson);
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
            var offset = 0;
            while (offset < count)
            {
                var bytesRead = stream.Read(buffer, offset, count - offset);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("The wrapper closed the pipe.");
                }

                offset += bytesRead;
            }
        }

        public void Dispose()
        {
            lock (Lock)
            {
                if (!Disposed)
                {
                    Disconnect();
                    Disposed = true;
                }
            }
        }

        public void ResetConnection()
        {
            lock (Lock)
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            Pipe?.Dispose();
            Pipe = null;
        }
    }
}
