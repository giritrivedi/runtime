using System.Runtime.CompilerServices;

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int multipleArgs(int a, float b, int c, float d, int e,
                            float f, int g, float h, int i) => (int)(a + b + c + d + e + f + g + h + i);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int s390xHw() => multipleArgs(1, 2.0f, 3, 4.0f, 5, 6.0f, 7, 8.0f, 9) == 45 ? 0 : 1;

    static int Main() => s390xHw();
}
