//Client

using System.Net.Sockets;
using System.Text;
using System.Net;
using GameServer.Characters;
using Newtonsoft.Json;
using GameServer;
using Microsoft.VisualBasic;

bool isInMenu = false;
bool isGameStarted = false;
bool isDataUpdate = false;

Message currentMessage = new Message();
object locker = new object();

string serverIp = "127.0.0.1";
int serverPort = 4444;
EndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

string? name;

Console.WriteLine("Добро пожаловать в Подземелье\n");
Console.Write("Введи свое имя: ");
name = Console.ReadLine();

await socket.ConnectAsync(serverEndPoint);

await SendMessageAsync(new Message { ouner = name ?? "no name" });

_ = Task.Run(ReceiveMessagesAsync);


while (true)
{
    if (!isGameStarted) continue;

    SelectAction();

    while (true)
    {
        if (isDataUpdate) break;
    }
}


void SelectAction()
{
    while (true)
    {
        Console.WriteLine("Нажми 1 чтобы атаковать или 2 чтобы передать оружие");
        ConsoleKeyInfo key = Console.ReadKey();

        Message msgToServer = new Message();

        if (key.Key == ConsoleKey.D1)
        {
            while (true)
            {
                Console.WriteLine($"Выберите какого моба хотите атаковать и введите его номер по порядку от 1 до {currentMessage.mobs.Count}");

                if (int.TryParse(Console.ReadLine(), out int mobIndex) && mobIndex > 0 && mobIndex <= currentMessage.mobs.Count)
                {
                    msgToServer.action = "attack";
                    msgToServer.targetId = --mobIndex;

                    break;
                }
            }
        }
        else if (key.Key == ConsoleKey.D2)
        {
            Console.WriteLine($"Выберите какому игроку вы хотите передать оружие. Введите его номер от 1 до {currentMessage.users.Count}" +
                $" Внимание! Если вы укажете несуществующего играка, то оружие будет уничтожено");
           
            msgToServer.action = "send";
            msgToServer.targetId = int.TryParse(Console.ReadLine(), out int userIndex)? --userIndex : -1;
        }

        isDataUpdate = false;

        _ = SendMessageAsync(msgToServer);
    }
}


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

    Console.Clear();

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
            isGameStarted = true;
            isDataUpdate = true;

            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Message responseMessage = JsonConvert.DeserializeObject<Message>(response);

            if (responseMessage == null) continue;

            lock (locker)
            {
                currentMessage = responseMessage;
            }

            if (isInMenu) continue;
                
            await Console.Out.WriteLineAsync(MountUserUI(responseMessage));
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"Ошибка при получении сообщений от сервера: {ex.Message}");
    }
}