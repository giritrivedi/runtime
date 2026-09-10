using System;

struct FiveBytes
{
    public byte A;
    public byte B;
    public byte C;
    public byte D;
    public byte E;
}

class Program
{
    static FiveBytes foo(byte a, byte b, byte c, byte d, byte e)
    {
        FiveBytes fb;
        fb.A = a;
        fb.B = b;
        fb.C = c;
        fb.D = d;
        fb.E = e;
        return fb;
    }

    public static int s390xHw()
    {
        FiveBytes fb = foo(0xAA, 0xBB, 0xCC, 0xDD, 0xEE);
        if (fb.A != 0xAA) return 1;
        if (fb.B != 0xBB) return 2;
        if (fb.C != 0xCC) return 3;
        if (fb.D != 0xDD) return 4;
        if (fb.E != 0xEE) return 5;
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
