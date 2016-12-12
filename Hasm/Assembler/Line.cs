using System.Text;

namespace hasm
{
    internal sealed class Line
    {
        public Line(string label, string instruction, string comment)
        {
            Label = label;
            Instruction = instruction;
            Comment = comment;
            IsInstruction = true;
        }

        public Line(string label, DirectiveTypes directive, string operands, string comment)
        {
            Label = label;
            Directive = directive;
            Operands = operands;
            Comment = comment;
            IsDirective = true;
        }

        public string Label { get; }
        public string Instruction { get; }
        public DirectiveTypes Directive { get; }
        public string Operands { get; }
        public string Comment { get; }
        public bool IsDirective { get; }
        public bool IsInstruction { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Label))
                builder.Append($"{Label}: ");

            builder.Append(IsDirective
                ? $"{Directive} {Operands}"
                : $"{Instruction}: ");

            if (!string.IsNullOrEmpty(Comment))
                builder.Append($"; {Comment}");

            return builder.ToString();
        }
    }
}