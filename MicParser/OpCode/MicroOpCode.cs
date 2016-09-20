using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var builder = new StringBuilder();
            var destinations = Enum.GetValues(Output.GetType())
                .Cast<Enum>()
                .Where(Output.HasFlag)
                .ToArray();

            if (destinations.Any())
                builder.Append(destinations.Select(s => s.ToString()).Aggregate((a, b) => $"{a} = {b}") + " =");

            if (ALU != ALU.Clear && ALU != ALU.Preset)
            {
                if (ALU.HasFlag(ALU.One))
                    builder.Append(" 1");
                else if (ALU.HasFlag(ALU.Zero))
                    builder.Append(" 0");
                else if (ALU.HasFlag(ALU.H))
                    builder.Append(" H");
            }

            var printRight = true;
            if (ALU.HasFlag(ALU.Add))
                builder.Append(" + ");
            else if (ALU.HasFlag(ALU.Sub) || ALU.HasFlag(ALU.InverseSub))
                builder.Append(" - ");
            else if (ALU.HasFlag(ALU.And))
                builder.Append(" & ");
            else if (ALU.HasFlag(ALU.Or))
                builder.Append(" | ");
            else if (ALU.HasFlag(ALU.Xor))
                builder.Append(" ^ ");
            else
            {
                builder.Append(ALU);
                printRight = false;
            }

            if (printRight)
                builder.Append(RightRegister);

            builder.Append(";");
            if (Memory != 0)
                builder.Append($" {Memory};");
            
            builder.Append($" goto {NextAddress};");
            return builder.ToString();
        }
    }
}