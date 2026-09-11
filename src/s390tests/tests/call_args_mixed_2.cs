using System.Runtime.CompilerServices;

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static double multipleArgs(int a, int b, int c, int d, int e,
                               double f, double g, double h, double i,
                               int j, double k) => a + b + c + d + e + f + g + h + i + j + k;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int s390xHw() => multipleArgs(1, 2, 3, 4, 5, 6.0, 7.0, 8.0, 9.0, 10, 11.0) == 66.0 ? 0 : 1;

    static int Main() => s390xHw();
}
