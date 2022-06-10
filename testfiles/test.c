char* MSG = "HELLO WORLD";
char* SCREENBUFFERPOINTER = (char*)0x4000;
int SCREENOFFSET = 0;


void MAIN()
{
    PRINT(MSG);
}

void PRINT(char* MSG)
{
    char attribute = 0x07;

    while(*MSG != 0)
    {
        SCREENBUFFERPOINTER[SCREENOFFSET] = *MSG;
        SCREENBUFFERPOINTER[SCREENOFFSET + 1] = attribute;
        SCREENOFFSET += 2;
        MSG++;
    }
}