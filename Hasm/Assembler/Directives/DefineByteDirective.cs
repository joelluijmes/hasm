using System.Collections.Generic;
using hasm.Parsing.Grammars;
using ParserLib.Evaluation;

namespace hasm.Assembler.Directives
{
    internal sealed class DefineByteDirective : BaseDirective
    {
        public DefineByteDirective(IDictionary<string, string> lookup) : base(lookup) {}
        public override DirectiveTypes DirectiveType { get; }

        public override IList<IAssemblingInstruction> Parse(Line line, ref int address)
        {
            Lookup[line.Label] = address.ToString();
            var tree = HasmGrammar.DefineByte.ParseTree(line.Operands);

            var list = new List<IAssemblingInstruction>();
            foreach (var node in tree.Descendents(n => n.IsValueNode<byte>()))
            {
                var value = node.FirstValue<byte>();
                list.Add(new DefinedByte(value, address));
                address += 2;
            }

            return list;
        }

        private class DefinedByte : IAssemblingInstruction
        {
            public DefinedByte(byte assembled, int address)
            {
                Bytes = new []{assembled};
                Address = address;
            }

            public int Address { get; set; }
            public int Count => 8;
            public byte[] Bytes { get; }
            public bool FullyAssembled => true;
        }
    }
}
