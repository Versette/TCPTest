using TCPTest.Common;

namespace TCPClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Client client = new Client();


            while (true)
            {
                var str = Console.ReadLine();
                client._syncedData.Data.Add(str ?? "");
                client._stream.SendMessage(new TCPMessage
                {
                    Type = MessageType.SendDataSync,
                    Data = client._syncedData
                }, Global.Key, Global.IV);
            }
        }
    }
}