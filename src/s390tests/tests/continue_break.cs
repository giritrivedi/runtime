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
        int sum = 0;

        for (int i = 1; i <= 10; i++)
        {
            if (i == 3)
            {
                continue;
            }

            if (i == 8)
            {
                break;
            }

            sum += i;
        }

        return sum == 25 ? 0 : 1;
    }
}

