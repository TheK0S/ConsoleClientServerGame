//Server

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameServer;
using GameServer.Characters;
using GameServer.Rooms;
using GameServer.Weapons;
using Newtonsoft.Json;

int ID = 1;
int gameLevel = 1;
bool isGameContinue = true;
int mobsCount = 10;
Random random = new Random();
List<string> currentGameActions = new List<string>();

List<User> clients = new List<User>();
List<Mob> mobs = new List<Mob>();
List<Cave> caves = new List<Cave>();
List<Weapon> weapons = new List<Weapon>();
Cave? currentCave;

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 4444;

Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(new IPEndPoint(ipAddress, port));
listener.Listen(10);
Console.WriteLine($"Сервер запущен. Ожидание подключений на порту {port}...");

_ = Task.Run(async () =>
{
    while (true)
    {
        Socket client = await listener.AcceptAsync();

        int clientId = ID++;

        User user = new User(clientId, client);

        clients.Add(user);

        _ = HandleClientMessagesAsync(user);
    }
});


while(isGameContinue)
{
    if (clients?.Count == 0) continue;

    MobsInit(mobsCount);
    CaveInit(gameLevel);




    Message msg = new Message();
    msg.users = clients ?? new List<User>();
    msg.mobs = currentCave?.mobs ?? new List<Mob>();
    msg.gameActions = currentGameActions.ToList();

    BroadcastMessage(msg);
    Thread.Sleep(10000);

    gameLevel++;
}




void CaveInit(int gameLvl)
{
    currentCave = new Cave();
    currentCave.users = clients ?? new List<User>();
    currentCave.mobs = new List<Mob>();

    for (int i = 0; i < gameLvl + 1; i++)
        currentCave.mobs.Add(mobs[random.Next(gameLvl)]);
}

void MobsInit(int mobsCount)
{
    for (int i = 0; i < mobsCount; i++)
    {
        mobs.Add(new Mob(i + 1, $"Mob-{i + 1}", (i + 1) * 5, (i + 1) * 2, (i + 1) * 2));
    }
}

async Task HandleClientMessagesAsync(User user)
{
    Socket client = user.UserSocket;

    using NetworkStream stream = new NetworkStream(client);

    byte[] buffer = new byte[1024];
    int bytesRead;

    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

    string jsonMsg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    Message msg = JsonConvert.DeserializeObject<Message>(jsonMsg);

    if(msg != null)
    {
        user.Name = msg.ouner;
        currentGameActions.Add($"Игрок {user.Name} присоеденился к игре");
    }
        
    try
    {
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            string clientResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Message message = JsonConvert.DeserializeObject<Message>(clientResponse);

            if (message == null) continue;
        }
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Ошибка при обработке сообщений от клиента. Клиент {user.Name} отключен: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при обработке сообщений от клиента {user.Name}: {ex.Message}", "Server error");
    }
    finally
    {
        clients.Remove(user);

        client.Close();

        currentGameActions.Add($"{user.Name} is exited");
    }
}

void BroadcastMessage(Message mes)
{
    if (clients?.Count == 0) return;

    string jsonMessage = JsonConvert.SerializeObject(mes);

    byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

    foreach (var client in clients)
    {
        if (client.UserSocket.Connected)
        {
            client.UserSocket.Send(messageBytes);
        }
    }
}