using System;

namespace MicParser.OpCode
{
    public class MicroOpCode
    {
        public ushort NextAddress { get; set; }
        public JAM JAM { get; set; }
        public ALU ALU { get; set; }
        public OutputRegister Output { get; set; }
        public Memory Memory { get; set; }
        public RightRegister RightRegister { get; set; }

        public long Value
        {
            get { return NextAddress | (long) JAM | (long) ALU | (long) Output | (long) Memory | (long) RightRegister; }
            set
            {
                NextAddress = (ushort) (value & 0x01FF);
                JAM = FromValue<JAM>(value);
                ALU = FromValue<ALU>(value);
                Output = FromValue<OutputRegister>(value);
                Memory = FromValue<Memory>(value);
                RightRegister = FromValue<RightRegister>(value);
            }
        }

        private static TEnum FromValue<TEnum>(long value)
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException();

            var mask = GetEnumMask<TEnum>();
            return (TEnum) (object) (value & mask);
        }

        private static long GetEnumMask<TEnum>()
        {
            long mask = 0;
            foreach (var value in Enum.GetValues(typeof(TEnum)))
                mask |= (long) value;

            return mask;
        }
    }
}