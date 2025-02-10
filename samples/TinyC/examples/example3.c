if (i > 100)
{
    if (j == 0)
        a = 1;
    else
        a = 2;
}
else if (i < 100)
{
    if      (j == 0) a = 10;
    else if (j == 1) a = 20;
    else if (j == 2) a = 30;
    else if (j == 3) a = 40;
    else if (j == 4) a = 50;
    else if (j == 5) a = 60;
    else             a = 70;
}
else
{
    b = j == 0 ? 10 :
        j == 1 ? 20 :
        j == 2 ? 30 :
        j == 3 ? 40 :
        j == 4 ? 50 :
        j == 5 ? 60 : 70;
}
