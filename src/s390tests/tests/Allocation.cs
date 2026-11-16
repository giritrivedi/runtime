using System;
using System.Runtime.CompilerServices;

public class S390xInstructionTest
{
    public static int Main()
    {
        int result = s390xHw();
        Console.WriteLine(result == 0 ? "PASS" : "FAIL: " + result);
        return result == 0 ? 0 : 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int Consume(Span<byte> s)
    {
        return s.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int FlushTest(int n)
    {
        Span<byte> buf = stackalloc byte[n];
        buf[0] = 0xAA;
        buf[n - 1] = 0xBB;
        return buf.Length; // Returns n
    }

    public static int s390xHw()
    {
        int r;

        // 1. Test FlushTest buffer length returns
        r = FlushTest(5);
        if (r != 5) return 1;

        r = FlushTest(40);
        if (r != 40) return 2;

        r = FlushTest(400);
        if (r != 400) return 3;

        return 0; // PASS
    }
}
