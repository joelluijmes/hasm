namespace MicParser.OpCode
{
    public struct MicroOpCode
    {
        public ushort NextAddress { get; set; }
        public JAM JAM { get; set; }
        public ALU ALU { get; set; }
        public OutputRegister Output { get; set; }
        public Memory Memory { get; set; }
        public RightRegister RightRegister { get; set; }

        public long Value
        {
            get { return NextAddress | (long) JAM << 9 | (long) ALU << 12 | (long) Output << 20 | (long) Memory << 29 | (long) RightRegister << 32; }
            set
            {
                NextAddress = (ushort) (value & 0x01FF);
                JAM = (JAM) ((value >> 9) & 0x07);
                ALU = (ALU) ((value >> 12) & 0xFF);
                Output = (OutputRegister) ((value >> 20) & 0x01FF);
                Memory = (Memory) ((value >> 29) & 0x07);
                RightRegister = (RightRegister) ((value >> 32) & 0x0F);
            }
        }
    }
}