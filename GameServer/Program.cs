//Server

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameServer.Characters;
using GameServer.Rooms;
using GameServer.Weapons;

int ID = 1;
int gameLevel = 1;
Random random = new Random();

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

while (true)
{
    Socket client = await listener.AcceptAsync();

    int clientId = ID++;

    User user = new User(clientId, client);

    clients.Add(user);

    _ = HandleClientMessagesAsync(clientId);
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

async Task HandleClientMessagesAsync(int clientId)
{
    Socket client = clients[clientId].UserSocket;

    User currentUser = clients[clientId];

    using NetworkStream stream = new NetworkStream(client);

    byte[] buffer = new byte[1024];
    int bytesRead;

    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    currentUser.Name = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    BroadcastMessage($"Игрок {currentUser.Name} присоеденился к игре");

    try
    {
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Сообщение от клиента {clientId}: {message}");

            BroadcastMessage(message);
        }
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Ошибка при обработке сообщений от клиента. Клиент {clientId} отключен: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при обработке сообщений от клиента {clientId}: {ex.Message}", "Server error");
    }
    finally
    {
        clients.TryRemove(clientId, out _);

        client.Close();        

        BroadcastMessage($"{clientId} is exited");
    }
}

void BroadcastMessage(string message)
{
    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

    foreach (var client in clients.Values)
    {
        if (client.UserSocket.Connected)
        {
            client.UserSocket.Send(messageBytes);
        }
    }
}