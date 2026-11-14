using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA2014 // Suppress warning for stackalloc inside loop

public class S390xInstructionTest
{
    public static int Main()
    {
        int result = s390xHw();
        Console.WriteLine(result == 0 ? "PASS" : "FAIL: " + result);
        return result == 0 ? 0 : 1;
    }

    public static int s390xHw()
    {
        // 1. Test Constant Size Allocation (<= 64B)
        if (!TestConstantAllocation())
            return 101;

        // 2. Test Dynamic Size Allocation (5B, 127B, 1024B)
        if (!TestDynamicAllocation(5) || !TestDynamicAllocation(127) || !TestDynamicAllocation(1024))
            return 102;

        // 3. Test Zero-Size Allocation (0B)
        if (!TestZeroSizeAllocation())
            return 103;

        // 4. Test Multiple Allocations in a Single Method
        if (!TestMultipleAllocations())
            return 104;

        // 5. Test localloc in a Loop with Child Calls
        if (!TestLocallocInLoop())
            return 105;

        // All tests passed
        return 0;
    }

    // =========================================================================
    // Test 1: Constant Size Allocation (Unrolled & Zero-Initialized)
    // =========================================================================
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TestConstantAllocation()
    {
        byte* buf = stackalloc byte[16];

        // Verify zero-init
        for (int i = 0; i < 16; i++)
        {
            if (buf[i] != 0) return false;
            buf[i] = (byte)(i + 1);
        }

        // Verify written data
        for (int i = 0; i < 16; i++)
        {
            if (buf[i] != (byte)(i + 1)) return false;
        }

        return true;
    }

    // =========================================================================
    // Test 2: Dynamic (Variable) Size Allocation with Loop Zeroing
    // =========================================================================
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TestDynamicAllocation(int size)
    {
        byte* buf = stackalloc byte[size];

        // Verify zero-init
        for (int i = 0; i < size; i++)
        {
            if (buf[i] != 0) return false;
        }

        // Write pattern
        for (int i = 0; i < size; i++)
        {
            buf[i] = (byte)(i & 0xFF);
        }

        // Verify pattern
        for (int i = 0; i < size; i++)
        {
            if (buf[i] != (byte)(i & 0xFF)) return false;
        }

        return true;
    }

    // =========================================================================
    // Test 3: Zero-Size Allocation (Bailout path returning NULL)
    // =========================================================================
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TestZeroSizeAllocation()
    {
        int zero = 0;
        byte* pDynamicZero = stackalloc byte[zero];
        byte* pConstZero   = stackalloc byte[0];

        return (pDynamicZero == null) && (pConstZero == null);
    }

    // =========================================================================
    // Test 4: Multiple localloc Calls in a Single Method
    // =========================================================================
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TestMultipleAllocations()
    {
        int localBefore = 0x12345678;

        byte* buf1 = stackalloc byte[32];
        byte* buf2 = stackalloc byte[64];
        byte* buf3 = stackalloc byte[128];

        int localAfter = 0x76543210;

        for (int i = 0; i < 32; i++)  buf1[i] = 0xAA;
        for (int i = 0; i < 64; i++)  buf2[i] = 0xBB;
        for (int i = 0; i < 128; i++) buf3[i] = 0xCC;

        for (int i = 0; i < 32; i++)  if (buf1[i] != 0xAA) return false;
        for (int i = 0; i < 64; i++)  if (buf2[i] != 0xBB) return false;
        for (int i = 0; i < 128; i++) if (buf3[i] != 0xCC) return false;

        if (localBefore != 0x12345678 || localAfter != 0x76543210) return false;

        return true;
    }

    // =========================================================================
    // Test 5: localloc in a Loop with Child Calls (RSA / Outgoing Arg Safety)
    // =========================================================================
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int HelperFunction(int a, int b, int c, int d, int e, int f)
    {
        return a + b + c + d + e + f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TestLocallocInLoop()
    {
        for (int iter = 1; iter <= 10; iter++)
        {
            int size = iter * 16;
            byte* buf = stackalloc byte[size];

            buf[0] = (byte)iter;
            buf[size - 1] = (byte)(iter * 2);

            int sum = HelperFunction(buf[0], buf[size - 1], 1, 2, 3, 4);
            if (sum != (iter + (iter * 2) + 10)) return false;

            if (buf[0] != (byte)iter || buf[size - 1] != (byte)(iter * 2)) return false;
        }
        return true;
    }
}
