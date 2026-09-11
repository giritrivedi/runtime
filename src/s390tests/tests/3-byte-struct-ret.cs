using System;

struct ThreeBytes
{
    public byte A;
    public byte B;
    public byte C;
}

class Program
{
    static ThreeBytes foo(byte a, byte b, byte c)
    {
        ThreeBytes tb;
        tb.A = a;
        tb.B = b;
        tb.C = c;
        return tb;
    }

    public static int s390xHw()
    {
        ThreeBytes tb = foo(0xAA, 0xBB, 0xCC);
        if (tb.A != 0xAA) return 1;
        if (tb.B != 0xBB) return 2;
        if (tb.C != 0xCC) return 3;
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
