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
bool isCompleteUserAction = false;
bool isFirstMessage = true;

Message currentMessage = new Message();
object locker = new object();
List<string> currentActions = new List<string>();

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
    isCompleteUserAction = false;

    currentActions.Add("> Нажми 1 чтобы атаковать или 2 чтобы передать оружие");
    MountUserUI(currentMessage);
    PrintActions();

    ConsoleKeyInfo key = Console.ReadKey();

    Message msgToServer = new Message();
    
    if (key.Key == ConsoleKey.D1)
    {
        while (true)
        {
            currentActions.Add($"> Введите номер по порядку моба для атаки от 1 до {currentMessage.mobs.Count}");
            MountUserUI(currentMessage);
            PrintActions();

            if (int.TryParse(Console.ReadLine(), out int mobIndex) && mobIndex > 0 && mobIndex <= currentMessage.mobs.Count)
            {
                msgToServer.ouner = name;
                msgToServer.action = "attack";
                msgToServer.targetId = --mobIndex;
                isCompleteUserAction = true;

                break;
            }
        }
    }
    else if (key.Key == ConsoleKey.D2)
    {
        currentActions.Add($"> Выберите какому игроку вы хотите передать оружие. Введите его номер от 1 до {currentMessage.users.Count}" +
            $" Внимание! Если вы укажете несуществующего играка, то оружие будет уничтожено");

        MountUserUI(currentMessage);
        PrintActions();

        msgToServer.action = "send";
        msgToServer.targetId = int.TryParse(Console.ReadLine(), out int userIndex) ? --userIndex : -1;
        isCompleteUserAction = true;
    }

    isDataUpdate = false;
    MountUserUI(currentMessage);

    _ = SendMessageAsync(msgToServer);
}





void PrintActions()
{
    Console.Clear();

    foreach (var action in currentActions)
        Console.WriteLine(action);
}

async Task SendMessageAsync(Message message)
{
    string json = JsonConvert.SerializeObject(message);

    byte[] messageBytes = Encoding.UTF8.GetBytes(json);

    await socket.SendToAsync(messageBytes, SocketFlags.None, serverEndPoint);
}

void MountUserUI(Message message)
{
    List<string> userActions = currentActions.FindAll(a => a.StartsWith('>'));

    currentActions.Clear();

    foreach (var client in message.users) currentActions.Add($"{client}");
    if (currentActions.Count > 0) currentActions[currentActions.Count - 1] += "\n";

    foreach (var mob in message.mobs) currentActions.Add($"{mob}");
    if (currentActions.Count > 0) currentActions[currentActions.Count - 1] += "\n";


    foreach (var action in message.gameActions) currentActions.Add($"{action}");
    if (currentActions.Count > 0) currentActions[currentActions.Count - 1] += "\n";

    if (!isCompleteUserAction) currentActions.AddRange(userActions);

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

            MountUserUI(responseMessage);
            if(!isFirstMessage) PrintActions();
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"Ошибка при получении сообщений от сервера: {ex.Message}");
    }
}