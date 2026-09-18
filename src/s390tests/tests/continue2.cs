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

        while (i < 10)
        {
            i++;

            if (i % 2 == 0)
            {
                continue;
            }

            sum += i;
        }

        return sum == 25 ? 0 : 1;
    }
}

