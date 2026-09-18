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

        for (;;)
        {
            i++;

            if (i == 1000)
            {
                break;
            }
        }

        return i == 1000 ? 0 : 1;
    }
}
