//Client

using static System.Net.Mime.MediaTypeNames;
using System.Net.Sockets;
using System.Text;
using System.Net;

string serverIp = "127.0.0.1";
int serverPort = 4444;
EndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


string? name;
Console.WriteLine("Welcome to Underworld\n");
Console.Write("Enter your name: ");
name = Console.ReadLine();

await socket.ConnectAsync(serverEndPoint);

_ = Task.Run(ReceiveMessagesAsync);

await SendMessageAsync("Hello server");
Console.ReadLine();

async Task SendMessageAsync(string message)
{
    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

    await socket.SendToAsync(messageBytes, SocketFlags.None, serverEndPoint);
}

async Task ReceiveMessagesAsync()
{
    byte[] buffer = new byte[512];
    int bytesRead;

    try
    {
        while ((bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
        {
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            await Console.Out.WriteLineAsync(response);
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"Ошибка при получении сообщений от сервера: {ex.Message}");
    }
}