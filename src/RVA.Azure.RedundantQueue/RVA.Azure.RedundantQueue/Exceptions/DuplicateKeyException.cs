using System;

namespace RVA.Azure.RedundantQueue.Exceptions
{
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message) : base(message)
        {
        }
    }
}