using System;
using System.Runtime.CompilerServices;

class Program
{
    // CHECK_INT_RANGE: long → int
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int foo(long val)
    {
        return checked((int)val);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int s390xHw()
    {
        // Should succeed: 42 fits in int
        int result = foo(42L);
        if (result != 42) return 1;

        // Should succeed: -1 fits in int
        result = foo(-1L);
        if (result != -1) return 2;

        // Should succeed: INT32_MAX fits in int
        result = foo(2147483647L);
        if (result != 2147483647) return 3;

        // Should succeed: INT32_MIN fits in int
        result = foo(-2147483648L);
        if (result != -2147483648) return 4;

        return 0;
    }

    static int Main() => s390xHw();
}
