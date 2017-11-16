using System;
using System.ServiceModel.Channels;

namespace FORCEBuild.Windows.Stream
{
    public class BufferManagerBufferPool
    {
        private readonly BufferManager m_bufferManager;

        private static BufferManagerBufferPool pool;

        /// <summary>
        /// Create a new BufferManagerBufferPool with the specified maximum buffer pool size
        /// and a maximum size for each individual buffer in the pool.
        /// </summary>
        /// <param name="maxBufferPoolSize">the maximum size to allow for the buffer pool</param>
        /// <param name="maxBufferSize">the maximum size to allow for each individual buffer in the pool</param>
        /// <exception cref="InsufficientMemoryException">There was insufficient memory to create the requested buffer pool.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Either maxBufferPoolSize or maxBufferSize was less than zero.</exception>
        private BufferManagerBufferPool(long maxBufferPoolSize, int maxBufferSize)
        {
            m_bufferManager = BufferManager.CreateBufferManager(maxBufferPoolSize, maxBufferSize);
        }

        public static BufferManagerBufferPool Current => pool??(pool=new BufferManagerBufferPool(102400,4096));

        /// <summary>
        /// Return a byte-array buffer of at least the specified size from the pool.
        /// </summary>
        /// <param name="size">the size in bytes of the requested buffer</param>
        /// <returns>a byte-array that is the requested size</returns>
        /// <exception cref="ArgumentOutOfRangeException">size cannot be less than zero</exception>
        public byte[] Take(int size)
        {
            return m_bufferManager.TakeBuffer(size);
        }

        /// <summary>
        /// Return the given buffer to this manager pool.
        /// </summary>
        /// <param name="buffer">a reference to the buffer being returned</param>
        /// <exception cref="ArgumentException">the Length of buffer does not match the pool's buffer length property</exception>
        /// <exception cref="ArgumentNullException">the buffer reference cannot be null</exception>
        public void Return(byte[] buffer)
        {
            m_bufferManager.ReturnBuffer(buffer);
        }

        /// <summary>
        /// Release the buffers currently cached in this manager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the buffers currently cached in this manager.
        /// </summary>
        /// <param name="disposing">true if managed resources are to be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            m_bufferManager.Clear();
        }
    }
}