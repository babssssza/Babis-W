using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
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
                   // Pipe.Dispose();
                    Pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
                    Pipe.Connect(Timeout);
                    Console.WriteLine("connected");
                }
            }
        }

        public T SendRequest<T>(string messageType, object data)
        {
            lock (Lock) // requests are sent one at a time
            {
                if (Disposed) throw new ObjectDisposedException(nameof(PipeWrite)); // unlikely case

                for (int attempt = 0; attempt < 2; attempt++)
                {
                    try
                    {
                        EnsureConnected();
                        var Req = new RequestMessage
                        {
                            MessageType = messageType,
                            Data = JsonConvert.SerializeObject(data)
                        };

                        WriteMessage(Pipe, Req);
                        var Res = ReadMessage(Pipe);

                        if (!Res.Success)
                        {
                            throw new Exception($"error: {Res.ErrorMessage}");
                        }

                        return JsonConvert.DeserializeObject<T>(Res.Data);
                    }
                    catch (IOException)
                    {
                        Disconnect();
                        if (attempt == 1) throw new Exception("The WRD wrapper connection was lost.");
                    }
                    catch (TimeoutException)
                    {
                        Disconnect();
                        if (attempt == 1) throw new Exception("The WRD wrapper did not respond in time.");
                    }
                }

                throw new Exception("The WRD wrapper connection failed.");
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
            if (MessageLength <= 0 || MessageLength > 10 * 1024 * 1024)
                throw new IOException("Invalid pipe response length.");

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
                var read = stream.Read(buffer, offset, count - offset);
                if (read == 0) throw new IOException("The pipe was closed.");
                offset += read;
            }
        }

        private void Disconnect()
        {
            Pipe?.Dispose();
            Pipe = null;
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
    }
}
