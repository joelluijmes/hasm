using System.IO;

namespace MicAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var assembly = File.ReadAllLines("assembly.txt");
            var assembler = new Assembler(assembly);
            assembler.Parse();
        }
    }
}
