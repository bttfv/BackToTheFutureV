using System;

namespace BackToTheFutureV
{
    /// <summary>
    /// Defines a class that is accessed via a pointer.
    /// </summary>
    public interface INativeMemory
    {
        /// <summary>
        /// Memory Address of the class.
        /// </summary>
        IntPtr MemoryAddress { get; }
    }
}
