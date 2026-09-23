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
        int value = 10;

        if (value == 10)
        {
            goto First;
        }

    First:
        value += 5;
        goto End;

    End:
        return value == 15 ? 0 : 1;
    }
}

