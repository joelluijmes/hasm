using System.Linq;

namespace hasm.Parsing.Models
{
    /// <summary>
    ///     Model used to parse instruction with encoding from excel sheet.
    /// </summary>
    public class InstructionEncoding
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InstructionEncoding" /> class.
        /// </summary>
        /// <param name="grammar">The grammar.</param>
        /// <param name="description">The description.</param>
        /// <param name="semantic">The semantic.</param>
        /// <param name="encoding">The encoding format of the grammar.</param>
        public InstructionEncoding(string grammar, string description, string semantic, string encoding)
        {
            Grammar = grammar;
            Description = description;
            Semantic = semantic;
            Encoding = encoding;
        }

        /// <summary>
        ///     Gets the grammar.
        /// </summary>
        /// <value>
        ///     The grammar.
        /// </value>
        public string Grammar { get; }

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        ///     Gets the semantic.
        /// </summary>
        /// <value>
        ///     The semantic.
        /// </value>
        public string Semantic { get; }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        public int Count => Encoding.Length/8;

        /// <summary>
        ///     Gets the encoding.
        /// </summary>
        /// <value>
        ///     The encoding.
        /// </value>
        public string Encoding { get; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{Grammar.Split(' ')[0]} ({Description})";

        /// <summary>
        ///     Parses the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>Row parsed in InstructionEncoding.</returns>
        public static InstructionEncoding Parse(string[] row)
        {
            var encoding = row.Skip(3).Aggregate((a, b) => a + b);
            return new InstructionEncoding(row[0], row[1], row[3], encoding);
        }
    }
}
