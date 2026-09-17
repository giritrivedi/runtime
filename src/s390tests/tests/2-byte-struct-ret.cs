using System;

struct TwoBytes
{
    public byte X;
    public byte Y;
}

class Program
{
    static TwoBytes foo(byte x, byte y)
    {
        TwoBytes tb;
        tb.X = x;
        tb.Y = y;
        return tb;
    }

    public static int s390xHw()
    {
        TwoBytes tb = foo(0xAA, 0x55);
        if (tb.X != 0xAA) return 1;
        if (tb.Y != 0x55) return 2;
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
