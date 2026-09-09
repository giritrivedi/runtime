using System;

struct Small {
    public int Value;
}

class Program {
    static Small foo(int v) {
        Small s;
        s.Value = v;
        return s;
    }

    public static int s390xHw() {
        Small s = foo(5);
        if (s.Value != 5)
            return 1;
        return 0;
    }

    static int Main() {
        int result = s390xHw();
        Console.WriteLine(result == 0 ? "PASS" : "FAIL: " + result);
        return result;
    }
}
