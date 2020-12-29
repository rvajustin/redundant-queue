using System;

namespace RVA.RedundantQueue.Implementations
{
    public class QueuePriority : IComparable, IComparable<byte>, IComparable<QueuePriority>
    {
        public static QueuePriority First = new QueuePriority(byte.MinValue);
        public static QueuePriority Second = new QueuePriority(byte.MinValue + 1);
        public static QueuePriority Last = new QueuePriority(byte.MaxValue);
        private readonly byte value;

        private QueuePriority(byte value)
        {
            this.value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is QueuePriority qp) return value.CompareTo(qp.value);
            if (obj is byte b) return value.CompareTo(b);
            return value.CompareTo(obj);
        }

        public int CompareTo(byte obj)
        {
            return value.CompareTo(obj);
        }

        public int CompareTo(QueuePriority other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return value.CompareTo(other.value);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        protected bool Equals(QueuePriority other)
        {
            return value == other.value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static implicit operator QueuePriority(byte value)
        {
            return new QueuePriority(value);
        }

        public static implicit operator byte(QueuePriority value)
        {
            return value.value;
        }
    }
}