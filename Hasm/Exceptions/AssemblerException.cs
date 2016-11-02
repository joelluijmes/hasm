using System;

namespace hasm.Exceptions
{
    /// <summary>
    ///     Exceptions produced by hasm
    /// </summary>
    /// <seealso cref="System.Exception" />
    internal sealed class AssemblerException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblerException" /> class.
        /// </summary>
        public AssemblerException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblerException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AssemblerException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblerException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference (Nothing in
        ///     Visual Basic) if no inner exception is specified.
        /// </param>
        public AssemblerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}