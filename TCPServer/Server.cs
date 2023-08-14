using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BeaconLib;
using MessagePack;
using TCPTest.Common;

namespace TCPServer;

public class Server
{
    private readonly Beacon _beacon;
    private readonly List<TcpClient> _clients = new();
    private readonly bool _isRunning = true;
    private readonly TcpListener _listener;
    private readonly ushort _port;

    private SyncedData _syncedData = new()
    {
        Id = 46,
        RandomBinaryData = Encoding.UTF8.GetBytes("TestData"),
        Data = new ObservableCollection<string> { "Test 1", "Test2" }
    };

    public Server(ushort port = 12345)
    {
        _port = port;

        // Start
        _beacon = new Beacon("POSserver", _port);
        _beacon.Start();

        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        Console.WriteLine("Server listening on port " + _port);

        // Start watching for clients on separate thread
        new Thread(() =>
        {
            while (_isRunning)
                if (_listener.Pending())
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    // Add client to connected clients list
                    lock (_clients)
                    {
                        _clients.Add(client);
                    }

                    Thread clientThread = new Thread(HandleClientRead);
                    clientThread.Start(client);
                    Console.WriteLine("Client connected - " + (IPEndPoint)client.Client.RemoteEndPoint);
                }
        }).Start();
    }

    private void HandleClientRead(object? parameter)
    {
        var client = parameter as TcpClient;
        var stream = client.GetStream();
        if (client != null)
        {
            while (_isRunning && client.Connected)
                try
                {
                    // Check for pending data and read pending message
                    var message = stream.ReadMessage(Global.Key, Global.IV);

                    switch (message.Type)
                    {
                        case MessageType.ReceiveDataSync:
                            Console.WriteLine("Received ReceiveDataSync message, sending DataSync");
                            stream.SendMessage(new TCPMessage
                            {
                                Type = MessageType.SendDataSync,
                                Data = _syncedData
                            }, Global.Key, Global.IV);
                            break;
                        case MessageType.SendDataSync:
                            Console.WriteLine("Received modified SyncedData, updating local representation...");
                            // TODO add this implementation
                            Console.WriteLine();
                            Console.WriteLine(MessagePackSerializer.SerializeToJson(message.Data));
                            Console.WriteLine();

                            // Update local SyncedData
                            lock (_syncedData)
                            {
                                _syncedData = message.Data as SyncedData;

                                // Broadcast changes
                                BroadcastChanges(client, message);
                            }

                            break;
                        case MessageType.Telemetry:
                            Console.WriteLine("Telemetry not implemented!");
                            break;
                    }
                }
                catch (Exception e)
                {
                    break;
                }

            Console.WriteLine("Client disconnected - " + (IPEndPoint)client.Client.RemoteEndPoint);
            client.Close();

            lock (_clients)
            {
                _clients.Remove(client); // Remove client from connected clients list
            }
        }
    }

    private void BroadcastChanges(TcpClient client, TCPMessage data)
    {
        // Broadcast changes to all clients except this one
        var filteredClients = _clients.Where(x => x != client).ToList();

        foreach (var filteredClient in filteredClients)
            // Send update data to client
            filteredClient.GetStream().SendMessage(data, Global.Key, Global.IV);
    }
}