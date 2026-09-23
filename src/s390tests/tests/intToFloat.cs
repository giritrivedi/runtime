using System.Runtime.CompilerServices;

class Program
{
    // signed int32 -> float (cefbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static float foo(int x) => (float)x;

    // signed int32 -> double (cdfbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static double s390xHw(int x) => (double)x;

    // signed int64 -> float (cegbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static float oneArg(long x) => (float)x;

    // signed int64 -> double (cdgbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static double addOne(long x) => (double)x;

    // unsigned int32 -> float (celfbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static float twoArgs(uint x) => (float)x;

    // unsigned int32 -> double (cdlfbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static double addTwo(uint x) => (double)x;

    // unsigned int64 -> float (celgbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static float addFour(ulong x) => (float)x;

    // unsigned int64 -> double (cdlgbr)
    [MethodImpl(MethodImplOptions.NoInlining)]
    static double multipleArgs(ulong x) => (double)x;

    static int Main()
    {
        int fail = 0;

        // cefbr: int32 -> float
        if (foo(42) < 41.0f || foo(42) > 43.0f) fail |= 1;

        // cdfbr: int32 -> double
        if (s390xHw(42) < 41.0 || s390xHw(42) > 43.0) fail |= 2;

        // cegbr: int64 -> float
        if (oneArg(100L) < 99.0f || oneArg(100L) > 101.0f) fail |= 4;

        // cdgbr: int64 -> double
        if (addOne(100L) < 99.0 || addOne(100L) > 101.0) fail |= 8;

        // celfbr: uint32 -> float
        if (twoArgs(50u) < 49.0f || twoArgs(50u) > 51.0f) fail |= 16;

        // cdlfbr: uint32 -> double
        if (addTwo(50u) < 49.0 || addTwo(50u) > 51.0) fail |= 32;

        // celgbr: uint64 -> float
        if (addFour(200UL) < 199.0f || addFour(200UL) > 201.0f) fail |= 64;

        // cdlgbr: uint64 -> double
        if (multipleArgs(200UL) < 199.0 || multipleArgs(200UL) > 201.0) fail |= 128;

        return fail;
    }
}
