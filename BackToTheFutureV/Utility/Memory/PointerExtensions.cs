using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace BackToTheFutureV
{
    /// <summary>
    /// Extension methods for the <see cref="IntPtr"/>.
    /// </summary>
    internal static class PointerExtensions
    {
        /// <summary>
        /// Checks if pointer is in range of 0x10000 and <see cref="long.MaxValue"/>.
        /// </summary>
        /// <param name="ptr">Pointer to check.</param>
        /// <returns>True if pointer may be valid, otherwise False.</returns>
        [Pure]
        [DebuggerStepThrough]
        public static bool MayBeValid(this IntPtr ptr)
        {
            return ptr.IsInRangeOf(0x10000, long.MaxValue);
        }

        /// <summary>
        /// Checks if pointer is in given range.
        /// </summary>
        /// <param name="ptr">Pointer to check.</param>
        /// <param name="min">Minimum value of the range.</param>
        /// <param name="max">Maximum value of the range.</param>
        /// <returns></returns>
        public static bool IsInRangeOf(this IntPtr ptr, long min, long max)
        {
            long ptrLong = ptr.ToInt64();
            return ptrLong >= min && ptrLong <= max;
        }
    }
}
