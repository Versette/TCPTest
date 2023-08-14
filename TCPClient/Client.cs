using System.Net;
using System.Net.Sockets;
using BeaconLib;
using MessagePack;
using TCPTest.Common;

namespace TCPClient;

public class Client
{
    private readonly bool _isRunning = true;
    private TcpClient _client;
    private bool _isConnected;
    private bool _isReconnecting;
    public NetworkStream _stream;
    public SyncedData _syncedData = new();

    public Client()
    {
        StartReconnectionThread();
    }

    private void StartReconnectionThread()
    {
        // Start reconnection thread
        new Thread(() =>
        {
            // Initial connection
            Reconnect(true);

            while (_isRunning)
                if (!_isConnected && !_isReconnecting)
                {
                    _isConnected = false;
                    _isReconnecting = true;
                    Console.WriteLine("Server connection lost. Reconnecting...");
                    Reconnect();
                }
        }).Start();
    }

    private void ReadDataWork()
    {
        while (_isConnected)
            try
            {
                if (_stream == null)
                    throw new Exception("Stream was null.");

                // Check for pending data and read pending message
                var message = _stream.ReadMessage(Global.Key, Global.IV);

                // Process message
                switch (message.Type)
                {
                    case MessageType.SendDataSync:
                        // Update synceddata object TODO FINISH
                        Console.WriteLine("Received SendDataSync message. Updating SyncedData...");
                        Console.WriteLine();
                        Console.WriteLine(MessagePackSerializer.SerializeToJson(message.Data));
                        Console.WriteLine();

                        lock (_syncedData)
                        {
                            _syncedData = message.Data as SyncedData;
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading stream, server disconnected.");
                _isConnected = false;
                break;
            }
    }

    private bool ConnectToServer()
    {
        _isConnected = false;
        var resetEvent = new ManualResetEvent(false); // Async could be used, but I will use this instead
        var probe = new Probe("POSserver");

        try
        {
            probe.BeaconsUpdated += AttemptConnection;
            probe.Start();
            Console.WriteLine("Looking for beacon - " + probe.BeaconType);

            void AttemptConnection(IEnumerable<BeaconLocation> beacons)
            {
                if (beacons != null && beacons.Any())
                {
                    var beacon = beacons.First();
                    try
                    {
                        Console.WriteLine("Found beacon");

                        // Attempt connection
                        _client = new TcpClient();
                        _client.Connect(beacon.Address);
                        _stream = _client.GetStream();
                        _isConnected = true;

                        // Start read thread
                        new Thread(ReadDataWork).Start();

                        // Request latest SyncedData
                        Console.WriteLine("Requesting latest SyncedData...");
                        _stream.SendMessage(new TCPMessage
                        {
                            Type = MessageType.ReceiveDataSync,
                            Data = null
                        }, Global.Key, Global.IV);

                        Console.WriteLine("Server connected - " + (IPEndPoint)_client.Client.RemoteEndPoint);
                    }
                    catch (Exception)
                    {
                        _isConnected = false;
                    }
                    finally
                    {
                        resetEvent.Set(); // Signal that the event has completed
                        probe.BeaconsUpdated -= AttemptConnection; // Remove the event handler
                    }
                }
            }

            // Wait for the event to complete
            resetEvent.WaitOne();

            // Clean up and stop the probe
            probe.Stop();
        }
        catch (Exception)
        {
            _isConnected = false;
        }
        finally
        {
            resetEvent.Dispose();
        }

        return _isConnected;
    }

    private void Reconnect(bool initialConnection = false)
    {
        if (!initialConnection)
            Console.WriteLine("Trying to reconnect to server...");

        while (_isRunning)
        {
            if (ConnectToServer())
            {
                _isReconnecting = false;
                break;
            }

            if (!initialConnection)
                Console.WriteLine("Failed to reconnect. Retrying in 1 second...");
            else
                Console.WriteLine("Failed to connect. Retrying in 1 second...");

            Thread.Sleep(1000); // Retry every 1000ms
        }
    }
}