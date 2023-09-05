//Client

using System.Net.Sockets;
using System.Text;
using System.Net;
using GameServer.Characters;
using Newtonsoft.Json;
using GameServer;

bool isInMenu = false;
string serverIp = "127.0.0.1";
int serverPort = 4444;
EndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

string? name;
Console.WriteLine("Добро пожаловать в Подземелье\n");
Console.Write("Введи свое имя: ");
name = Console.ReadLine();

await socket.ConnectAsync(serverEndPoint);

_ = SendMessageAsync(new Message { ouner = name ?? "no name" });

_ = Task.Run(ReceiveMessagesAsync);


Console.ReadLine();


async Task SendMessageAsync(Message message)
{
    string json = JsonConvert.SerializeObject(message);

    byte[] messageBytes = Encoding.UTF8.GetBytes(json);

    await socket.SendToAsync(messageBytes, SocketFlags.None, serverEndPoint);
}

string MountUserUI(Message message)
{
    string stringUI = "";

    foreach (var client in message.users) stringUI += $"{client}\n";

    stringUI += "\n\n";

    foreach (var mob in message.mobs) stringUI += $"{mob}\n";

    stringUI += "\n\n";

    foreach (var action in message.gameActions) stringUI += $"{action}\n";

    return stringUI;
}

async Task ReceiveMessagesAsync()
{
    byte[] buffer = new byte[2048];
    int bytesRead;

    try
    {
        while ((bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
        {
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Message responseMessage = JsonConvert.DeserializeObject<Message>(response);

            if (isInMenu || responseMessage == null) continue;
                
            await Console.Out.WriteLineAsync(MountUserUI(responseMessage));
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"Ошибка при получении сообщений от сервера: {ex.Message}");
    }
}