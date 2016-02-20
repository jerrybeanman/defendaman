using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* Carson
       Used as a grouping of DataType & type's ID to denote a specific object, used with the subscribing system.
       e.g. a pair of first = DataType.Player, second = 1 represents "Player 1"
    */
public class Pair<T, Y>
{
    //First 
    public T first;
    public Y second;

    private static readonly IEqualityComparer Item1Comparer = EqualityComparer<T>.Default;
    private static readonly IEqualityComparer Item2Comparer = EqualityComparer<Y>.Default;

    public Pair(T first, Y second)
    {
        this.first = first;
        this.second = second;
    }

    public override string ToString()
    {
        return string.Format("<{0}, {1}>", first, second);
    }

    public static bool operator ==(Pair<T, Y> a, Pair<T, Y> b)
    {
        if (IsNull(a) && !IsNull(b))
            return false;

        if (!IsNull(a) && IsNull(b))
            return false;

        if (IsNull(a) && IsNull(b))
            return true;

        return
            a.first.Equals(b.first) &&
            a.second.Equals(b.second);
    }

    public static bool operator !=(Pair<T, Y> a, Pair<T, Y> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        int multiplier = 21;
        hash = hash * multiplier + first.GetHashCode();
        hash = hash * multiplier + second.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Pair<T, Y>;
        if (object.ReferenceEquals(other, null))
            return false;
        else
            return Item1Comparer.Equals(first, other.first) &&
                   Item2Comparer.Equals(second, other.second);
    }

    private static bool IsNull(object obj)
    {
        return object.ReferenceEquals(obj, null);
    }
}