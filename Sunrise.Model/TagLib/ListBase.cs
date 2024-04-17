using System.Collections;
using System.Text;

namespace Sunrise.Model.TagLib;

public class ListBase<T> : IList<T> where T : IComparable<T>
{
    private readonly List<T> _data = [];

    public ListBase() { }

    public ListBase(ListBase<T> list)
    {
        if (list is not null)
            Add(list);
    }

    public ListBase(params T[] list)
        => Add(list);

    public bool IsEmpty => Count == 0;

    public void Add(ListBase<T> list)
    {
        if (list is not null)
            _data.AddRange(list);
    }

    public void Add(IEnumerable<T> list)
    {
        if (list is not null)
            _data.AddRange(list);
    }

    public void Add(T[] list)
    {
        if (list is not null)
            _data.AddRange(list);
    }

    public virtual void SortedInsert(T item, bool unique)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        int i = 0;

        for (; i < _data.Count; i++)
        {
            if (item.CompareTo(_data[i]) == 0 && unique)
                return;

            if (item.CompareTo(_data[i]) <= 0)
                break;
        }

        Insert(i, item);
    }

    public void SortedInsert(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        SortedInsert(item, false);
    }

    public T[] ToArray() => [.. _data];

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public T this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    public void Add(T item) => _data.Add(item);

    public void Clear() => _data.Clear();

    public bool Contains(T item) => _data.Contains(item);

    public int IndexOf(T item) => _data.IndexOf(item);

    public void Insert(int index, T item) => _data.Insert(index, item);

    public bool Remove(T item) => _data.Remove(item);

    public void RemoveAt(int index) => _data.RemoveAt(index);

    public string ToString(string separator)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < Count; i++)
        {
            if (i != 0)
                builder.Append(separator);

            builder.Append(this[i]);
        }

        return builder.ToString();
    }

    public override string ToString() => ToString(", ");

    public int Count => _data.Count;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void CopyTo(T[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
}
