using System;

struct EightBytes
{
    public byte A;
    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte F;
    public byte G;
    public byte H;
}
struct sample
{
    public int a; 
    public int b;
}

class Program
{
    static EightBytes foo1(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
    {
        EightBytes sb;
        sb.A = a;
        sb.B = b;
        sb.C = c;
        sb.D = d;
        sb.E = e;
        sb.F = f;
        sb.G = g;
        sb.H = h;
        return sb;
    }

    static sample foo2(int a, int b )
    {
         sample s; 
         s.a = a;
         s.b = b; 
         return s;
    }


    public static int s390xHw()
    {
        EightBytes sb = foo1(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22);
        if (sb.A != 0xAA) return 1;
        if (sb.B != 0xBB) return 2;
        if (sb.C != 0xCC) return 3;
        if (sb.D != 0xDD) return 4;
        if (sb.E != 0xEE) return 5;
        if (sb.F != 0xFF) return 6;
        if (sb.G != 0x11) return 7;
        if (sb.H != 0x22) return 8;

	sample s = foo2(2,5);
	if (s.a != 2) return 9; 
	if (s.b != 5) return 10; 
	
        return 0;
    }

    static int Main()
    {
        int r = s390xHw();
        Console.WriteLine(r == 0 ? "PASS" : "FAIL: " + r);
        return r;
    }
}
