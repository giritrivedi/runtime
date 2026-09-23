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
        int value = 0;

    Start:
        value++;

        if (value < 5)
        {
            goto Start;
        }

        return value == 5 ? 0 : 1;
    }
}

