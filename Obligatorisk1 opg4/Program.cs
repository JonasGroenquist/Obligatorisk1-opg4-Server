using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("TCP server");

TcpListener listener = new TcpListener(IPAddress.Any, 7);

listener.Start();
while (true)
{
    TcpClient socket = listener.AcceptTcpClient();
    IPEndPoint clientEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
    Console.WriteLine("Client connected:" + clientEndPoint.Address);

    Task.Run(() => HandleClient(socket));
}

//listener.Stop();

static void HandleClient(TcpClient socket)
{
    using (NetworkStream ns = socket.GetStream())
    using (StreamReader reader = new StreamReader(ns))
    using (StreamWriter writer = new StreamWriter(ns) { AutoFlush = true })
    {
        while (socket.Connected)
        {
            string message = reader.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(message)) continue;

            Console.WriteLine($"Received: {message}");

            if (message == "method")
            {
                writer.WriteLine("Which method? (add/subtract/random)");
                string method = reader.ReadLine()?.ToLower();

                writer.WriteLine("Enter first number:");
                if (!int.TryParse(reader.ReadLine(), out int num1))
                {
                    writer.WriteLine("Invalid input");
                    continue;
                }

                writer.WriteLine("Enter second number:");
                if (!int.TryParse(reader.ReadLine(), out int num2))
                {
                    writer.WriteLine("Invalid input");
                    continue;
                }

                string result = method switch
                {
                    "add" => $"Result: {num1 + num2}",
                    "subtract" => $"Result: {num1 - num2}",
                    "random" => $"Result: {new Random().Next(num1, num2)}",
                    _ => "Invalid method."
                };

                writer.WriteLine(result);
                Console.WriteLine($"Sent: {result}");
            }
            else if (message == "stop")
            {
                writer.WriteLine("Goodbye world");
                Console.WriteLine("Client disconnected");
                break;
            }
            else
            {
                writer.WriteLine("Unknown command. Type 'method' to start or 'stop' to exit.");
            }
        }
    }
    socket.Close();
}