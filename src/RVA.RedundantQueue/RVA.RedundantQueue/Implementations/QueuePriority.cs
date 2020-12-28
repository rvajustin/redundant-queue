using System;

namespace RVA.RedundantQueue.Implementations
{
    public class QueuePriority : IComparable, IComparable<byte>, IComparable<QueuePriority>
    {
        private readonly byte _value;

        public static QueuePriority First = new QueuePriority(byte.MinValue);
        public static QueuePriority Second = new QueuePriority(byte.MinValue + 1);
        public static QueuePriority Last = new QueuePriority(byte.MaxValue);

        private QueuePriority(byte value)
        {
            _value = value;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        protected bool Equals(QueuePriority other)
        {
            return _value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is QueuePriority qp) return _value.CompareTo(qp._value);
            if (obj is byte b) return _value.CompareTo(b);
            return _value.CompareTo(obj);
        }

        public int CompareTo(byte obj)
        {
            return _value.CompareTo(obj);
        }

        public int CompareTo(QueuePriority other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _value.CompareTo(other._value);
        }

        public static implicit operator QueuePriority(byte value)
        {
            return new QueuePriority(value);
        }

        public static implicit operator byte(QueuePriority value)
        {
            return value._value;
        }
    }
}