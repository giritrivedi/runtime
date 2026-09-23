using System;

public class S390xInstructionTest
{
    public static int Main()
    {
        int result = s390xHw();
        return result == 0 ? 0 : 1;
    }

    public static int s390xHw()
    {
        int a = 10;
        int b = 5;

        if (a > b)
        {
            goto Greater;
        }

        return 1;

    Greater:
        return 0;
    }
}

