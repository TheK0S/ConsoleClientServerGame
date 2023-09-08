//Server

using System.Collections.Concurrent;
using System.Globalization;
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
Queue<GameAction> gameActions = new Queue<GameAction>();

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


//Data exchange
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




while (true)
{
    if (clients?.Count == 0) continue;

    WeaponsFill();
    MobsInit(mobsCount);
    CaveInit(gameLevel, mobsCount);

    break;
}



//Game actions
while (isGameContinue)
{
    UsersTurn();
    MobsTurn();

    BroadcastMessage(CreateMessage());
    Thread.Sleep(7000);

    currentGameActions.Clear();
    RemoveTheDead();

    if (currentCave == null || currentCave.mobs.Count == 0)
    {
        Message CongratulationMsg = CreateMessage();
        if(gameLevel == 7)
        {
            CongratulationMsg.gameActions.Add("> Поздравляю! Вы ЧЕМПИОН!");
            BroadcastMessage(CongratulationMsg);
            isGameContinue = false;
            continue;
        }

        CongratulationMsg.gameActions.Add("> Поздравляю! Вы прошли уровень!\n\nГотовтесь к новой битве!" );

        int randomIndex = random.Next(currentCave.users.Count - 1);
        User user = currentCave.users[randomIndex];
        user.Weapon = null;

        int randomWeaponIndex = random.Next(weapons.Count - 1);
        user.Weapon = weapons[randomWeaponIndex];

        CongratulationMsg.gameActions.Add($"Игрок {user.Name} получил в награду {user.Weapon}");
        BroadcastMessage(CongratulationMsg);

        gameLevel++;
        MobsInit(mobsCount);
        CaveInit(gameLevel, mobsCount);
    }

    if (currentCave.users.Count <= 0)
    {
        Message CongratulationMsg = CreateMessage();
        CongratulationMsg.gameActions = new List<string> { "Вы проиграли!" };
        BroadcastMessage(CongratulationMsg);

        Thread.Sleep(3000);
        break;
    }
}




void WeaponsFill(){
    weapons.Add(new Weapon("Meч", 0, 40, 0));
    weapons.Add(new Weapon("Щит", 0, 0, 20));
    weapons.Add(new Weapon("Булава", 0, 30, 0));
    weapons.Add(new Weapon("Аптечка", 50, 0, 0));
}

void RemoveTheDead()
{
    if(currentCave == null) return;

    List<int> usersToRemove = new List<int>();
    List<int> mobsToRemove = new List<int>();

    //User remove
    foreach (var user in currentCave.users)
        if (!user.IsAlive)
            usersToRemove.Add(currentCave.users.IndexOf(user));

    foreach (var userIndex in usersToRemove)
        if (userIndex != -1) currentCave.users.RemoveAt(userIndex);

    //Mob remove
    foreach (var mob in currentCave.mobs)
        if (!mob.IsAlive)
            mobsToRemove.Add(currentCave.mobs.IndexOf(mob));

    foreach (var mobIndex in mobsToRemove)
        if (mobIndex != -1) currentCave.mobs.RemoveAt(mobIndex);
}

void UsersTurn()
{
    while (gameActions.Count > 0)
    {
        var currentAction = gameActions.Dequeue();
        User currentUser = clients?[currentAction.ounerId-1];

        if (currentUser == null) return;

        if (currentAction.isAttack)
            currentGameActions.Add(currentUser.AttackTheEnemy(currentCave.mobs?[currentAction.targetId]));
        else
            currentGameActions.Add(currentUser.DropWeapon(currentCave.users?[currentAction.targetId - 1]));
    }
}

void MobsTurn()
{
    if (currentCave == null || currentCave.mobs.Count == 0) return;

    if (currentCave.users.Count == 0) return;

    foreach (var mob in currentCave.mobs)
    {
        int userIndex = random.Next(currentCave.users.Count - 1);
        if (userIndex < 0 || userIndex >= currentCave.users.Count) continue;

        currentGameActions.Add(mob.AttackTheUser(currentCave.users[userIndex]));
    }
}

Message CreateMessage()
{
    Message msg = new Message();
    msg.users = clients ?? new List<User>();
    msg.mobs = currentCave?.mobs ?? new List<Mob>();
    msg.gameActions = currentGameActions;

    return msg;
}

void CaveInit(int gameLvl, int mCount)
{
    if(gameLvl > mCount -1) gameLvl = mCount - 1;

    currentCave = new Cave();
    currentCave.users = clients?.FindAll(c => c.IsAlive == true) ?? new List<User>();
    currentCave.mobs = new List<Mob>();

    for (int i = 0; i < gameLvl + 1; i++)
        currentCave.mobs.Add(mobs[i]);
}

void MobsInit(int mobsCount)
{
    mobs.Clear();

    for (int i = 0; i < mobsCount; i++)
    {
        mobs.Add(new Mob(i + 1, $"Mob-{i + 1}", (i + 1) * 5, (i + 1) * 2, (i + 1) * 2));
    }
}

async Task HandleClientMessagesAsync(User user)
{
    Socket client = user.UserSocket;

    using NetworkStream stream = new NetworkStream(client);

    byte[] buffer = new byte[2048];
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

            user.TargetId = message.targetId;
            if(message.action != null && message.action == "attack")
                gameActions.Enqueue(new GameAction(user.Id, user.TargetId, true));

            if (message.action != null && message.action == "send")
                gameActions.Enqueue(new GameAction(user.Id, user.TargetId, false));
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