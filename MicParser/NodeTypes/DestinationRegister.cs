namespace MicParser.NodeTypes
{
    public enum DestinationRegister : long
    {
        H = 1 << 0 << 20,
        OPC = 1 << 1 << 20,
        TOS = 1 << 2 << 20,
        CPP = 1 << 3 << 20,
        LV = 1 << 4 << 20,
        SP = 1 << 5 << 20,
        PC = 1 << 6 << 20,
        MDR = 1 << 7 << 20,
        MAR = 1 << 8 << 20
    }
}