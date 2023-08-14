using MessagePack;

namespace TCPTest.Common;

[MessagePackObject]
public class TCPMessage
{
    [Key(0)] public MessageType Type { get; set; }

    [Key(1)] public object Data { get; set; }
}

public enum MessageType
{
    ReceiveDataSync,
    SendDataSync,
    Telemetry
}