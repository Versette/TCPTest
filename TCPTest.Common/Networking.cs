using System.Net.Sockets;
using System.Security.Cryptography;
using MessagePack;

namespace TCPTest.Common;

public static class Networking
{
    /// <summary>
    ///     Reads a message from the NetworkStream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES IV</param>
    /// <returns></returns>
    public static TCPMessage ReadMessage(this NetworkStream stream, byte[] key, byte[] iv)
    {
        // Read message length
        byte[] sizeBuffer = new byte[4];
        stream.Read(sizeBuffer, 0, sizeBuffer.Length);
        int dataSize = BitConverter.ToInt32(sizeBuffer, 0);

        // Read full message
        byte[] dataBuffer = new byte[dataSize];
        int bytesRead = 0;
        while (bytesRead < dataSize) bytesRead += stream.Read(dataBuffer, bytesRead, dataSize - bytesRead);

        // Process received data
        var unpackedData = UnpackData(dataBuffer, Global.Key, Global.IV);
        return unpackedData as TCPMessage;
    }

    /// <summary>
    ///     Sends a message to the NetworkStream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="data">Message data object</param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES IV</param>
    /// <returns></returns>
    public static bool SendMessage(this NetworkStream stream, object data, byte[] key, byte[] iv)
    {
        try
        {
            // Pack data
            var packedData = PackData(data, key, iv);

            // Get data length bytes
            var lengthBytes = BitConverter.GetBytes(packedData.Length);

            // Write message length bytes
            stream.Write(lengthBytes, 0, 4);

            // Write message itself (packed data)
            stream.Write(packedData, 0, packedData.Length);
            stream.Flush(); // Make sure data is sent immediately

            return true;
        }
        catch (Exception e)

        {
        }

        return false;
    }

    /// <summary>
    ///     Sends a message to the NetworkStream
    /// </summary>
    /// <param name="data">Data object to serialize and pack</param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES IV</param>
    /// <returns></returns>
    public static byte[] PackData(object data, byte[] key, byte[] iv)
    {
        // Serialize with MessagePack (this compresses with LZ4 too)
        var serializedBytes = MessagePackSerializer.Typeless.Serialize(data);

        // Encrypt with AES256 (use 256bit keys)
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(serializedBytes, 0, serializedBytes.Length);
                    csEncrypt.FlushFinalBlock();
                }

                var encryptedBytes = msEncrypt.ToArray();

                return encryptedBytes;
            }
        }
    }

    /// <summary>
    ///     Sends a message to the NetworkStream
    /// </summary>
    /// <param name="data">Data object to deserialize and unpack</param>
    /// <param name="key">AES key</param>
    /// <param name="iv">AES IV</param>
    /// <returns></returns>
    public static object UnpackData(byte[] data, byte[] key, byte[] iv)
    {
        // Decrypt AES256
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream msDecrypted = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msDecrypted);

                        var decryptedBytes = msDecrypted.ToArray();

                        // Deserialize MessagePack (this extracts LZ4 too)
                        var deserializedData =
                            MessagePackSerializer.Typeless
                                .Deserialize(decryptedBytes); //Deserialize<T>(decryptedBytes);
                        return deserializedData;
                    }
                }
            }
        }
    }
}