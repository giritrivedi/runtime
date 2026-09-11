using System;

struct SixBytes
{
    public byte A;
    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte F;
}

class Program
{
    static SixBytes foo(byte a, byte b, byte c, byte d, byte e, byte f)
    {
        SixBytes sb;
        sb.A = a;
        sb.B = b;
        sb.C = c;
        sb.D = d;
        sb.E = e;
        sb.F = f;
        return sb;
    }

    public static int s390xHw()
    {
        SixBytes sb = foo(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);
        if (sb.A != 0xAA) return 1;
        if (sb.B != 0xBB) return 2;
        if (sb.C != 0xCC) return 3;
        if (sb.D != 0xDD) return 4;
        if (sb.E != 0xEE) return 5;
        if (sb.F != 0xFF) return 6;
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
