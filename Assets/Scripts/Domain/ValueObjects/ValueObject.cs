namespace Assets.Scripts.Domain.ValueObjects
{
    using System;

    public abstract class ValueObject<T> : IComparable<T> where T : ValueObject<T>
    {
        public abstract int CompareTo(T other);

        public override bool Equals(object obj)
        {
            if (obj is T other)
                return EqualsCore(other);
            return false;
        }

        protected abstract bool EqualsCore(T other);

        public override int GetHashCode()
        {
            return GetHashCodeCore();
        }

        protected abstract int GetHashCodeCore();

        // --- ‚±‚±‚©‚ç”äŠr‰‰ŽZŽq‚ð‹¤’Ê‰» ---
        public static bool operator <(ValueObject<T> left, ValueObject<T> right)
            => left is null ? right is not null : right is not null && left.CompareTo((T)right) < 0;

        public static bool operator >(ValueObject<T> left, ValueObject<T> right)
            => right is null ? left is not null : left is not null && left.CompareTo((T)right) > 0;

        public static bool operator <=(ValueObject<T> left, ValueObject<T> right)
            => left is null || (right is not null && left.CompareTo((T)right) <= 0);

        public static bool operator >=(ValueObject<T> left, ValueObject<T> right)
            => right is null || (left is not null && left.CompareTo((T)right) >= 0);
        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return Equals(right, left);
        }

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !Equals(right, left);
        }

    }
}