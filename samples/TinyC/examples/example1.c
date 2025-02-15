/*
 * Tiny-C: Example 1.
 */

{
    /////////////////////
    ///// Factorial /////
    /////////////////////
    x = 5;
    fact = 1;

    if (x > 0)
    {
        do
        {
            fact = fact * x;
            x = x - 1;
        }
        while (x != 0);
    }
}
