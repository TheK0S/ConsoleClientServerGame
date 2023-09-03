//Server

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameServer.Characters;
using GameServer.Weapons;

int ID = 1;
ConcurrentDictionary<int, User> clients = new ConcurrentDictionary<int, User>();

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

    clients.TryAdd(clientId, user);

    _ = HandleClientMessagesAsync(clientId);
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