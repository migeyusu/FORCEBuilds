using System;

namespace FORCEBuild.Windows.Stream
{
    /// <summary>
    /// The IBufferPool interface specifies two methods: Take, and Return.
    /// These provide for taking byte-array data from a common pool, and returning it.
    /// </summary>
    public interface IBufferPool : IDisposable
    {
        /// <summary>
        /// Take byte-array storage from the buffer-pool.
        /// </summary>
        /// <param name="size">the number of bytes to take</param>
        /// <returns>a byte-array that comes from the buffer-pool</returns>

        byte[] Take(int size);

        /// <summary>
        /// Return the given byte-array buffer to the common buffer-pool.
        /// </summary>
        /// <param name="buffer">the byte-array to return to the buffer-pool</param>
        void Return(byte[] buffer);
    }
}