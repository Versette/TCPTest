namespace TCPTest.Common;

public class Global
{
    // TODO This is wrong, I know, but it works for testing :>
    public static byte[] Key =
        Utils.HexStringToByteArray("24ef2cbebe472104bdcbcc77761f3d5702b0e9fc466e3cf0f5555da3d1fde66c");

    public static byte[] IV = Utils.HexStringToByteArray("b4ca2bb819fc1e3cb6343fe9fe624649");
}