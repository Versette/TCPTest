# TCPTest
By default, the server initializes with a `SyncedData` object that is filled with sample data. This pbject is then sent to each client that connects (only once, on connection) and each client keeps a local representation of this object. When a client adds data to the list (`ReadLine()` string), this gets sent to the server, and the server broadcasts this change to all connected clients excluding the one that sent this (to keep all data synced in that way). 

I published this in case anyone needs some similar solution, but it's unfinished and just sample test code. It can be improved. 

I will use it for an ordering system.

## Features
- Automatic server discovery by using UDP broadcasts (rix0rrr/beacon library).
- AES 256 encryption (hardcoded encryption key and IV, but this can be changed easily).
- Supports different kinds of messages. It's sort of modular, so more can be implemented by adding more enum members and adding handling code to the client/server (pretty easy to do, in the switch statements).
- Uses length-prefixing framing.
- Uses MessagePack for serialization (LZ4 used for compression in the library that I use).
- Server handles each client separately in separate threads.
