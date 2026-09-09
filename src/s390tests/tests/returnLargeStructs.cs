using System;

struct Large{
    public int IValue;
    public float FValue;
    public char CValue;
    public double DValue;
}

class Program {
    static Large MakeLarge(int i, float f, char c, double f2) {
        Large l;
        l.IValue = i;
        l.FValue = f;
        l.CValue = c;
        l.DValue = f2;
        return l;
    }

    public static int s390xHw() {
        Large l = MakeLarge(5, 6.0f, 'a', 7.0);
        if (l.IValue != 5 && l.FValue != 6.0f && l.CValue != 'a' && l.DValue != 7.0 )
            return 1;
        return 0;
    }
    static int Main() {
        int result = s390xHw();
        Console.WriteLine(result == 0 ? "PASS" : "FAIL: " + result);
        return result;
    }
}
