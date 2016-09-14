namespace MicParser.NodeTypes
{
    public enum RightRegister : long
    {
        MDR  = 0L << 32,
        PC   = 1L << 32,
        MBR  = 2L << 32,
        MBRU = 3L << 32,
        SP   = 4L << 32,
        LV   = 5L << 32,
        CPP  = 6L << 32,
        TOS  = 7L << 32,
        OPC  = 8L << 32
    }
}