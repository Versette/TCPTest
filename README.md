# TCPTest
By default, the server initializes with a `SyncedData` object filled with sample data. This object is then sent to each client that connects (only once, on connection) and each client keeps a local representation of this object. When a client adds data to the list (`ReadLine()` string), this is sent to the server, and the server broadcasts this change to all connected clients except the one that sent it (to keep all data synchronized this way). 

I published this in case anyone needs a similar solution, but it's unfinished and just test code. It can be improved. 

I will use it for an ordering system.

## Features
- **Automatic server discovery** using UDP broadcasts (rix0rrr/beacon library).
- **AES 256 encryption** (hardcoded encryption key and IV, but this can be easily changed).
- **Supports different types of messages.** It's kind of modular, so more can be implemented by adding more enum members and adding some handling code to the client/server (pretty easy to do, in the switch statements).
- **Uses length-prefixing framing.**
- **Uses MessagePack for serialization** (**LZ4 is used for compression** in the library I use).
- **Server handles each client separately in separate threads.**
