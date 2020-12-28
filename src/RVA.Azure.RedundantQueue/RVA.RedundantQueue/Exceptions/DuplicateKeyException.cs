using System;

namespace RVA.RedundantQueue.Exceptions
{
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message) : base(message)
        {
        }
    }
}