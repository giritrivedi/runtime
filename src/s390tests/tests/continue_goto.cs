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
        int i = 0;
        int sum = 0;

    Loop:
        i++;

        if (i > 10)
        {
            goto End;
        }

        if (i % 2 == 0)
        {
            goto Loop;
        }

        sum += i;
        goto Loop;

    End:
        return sum == 25 ? 0 : 1;
    }
}

