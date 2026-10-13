using System;

struct OneByte
{
    public byte V;
}

class Program
{
    static OneByte foo1(byte v)
    {
        OneByte ob;
        ob.V = v;
        return ob;
    }

    public static int s390xHw()
    {
        OneByte ob = foo1(0xAB);
        if (ob.V != 0xAB) return 1;
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
