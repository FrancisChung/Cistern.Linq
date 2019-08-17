﻿using Cistern.Linq.ChainLinq.Optimizations;
using System;
using System.Collections;
using System.Collections.Immutable;

namespace Cistern.Linq.Immutable
{
    struct ImmutableArrayEnumerator<T>
        : System.Collections.Generic.IEnumerator<T>
    {
        private ImmutableArray<T>.Enumerator e;

        public ImmutableArrayEnumerator(ImmutableArray<T> source) => e = source.GetEnumerator();
        public void Dispose() { }

        public T Current => e.Current;

        public bool MoveNext() => e.MoveNext();

        public void Reset() => throw new NotImplementedException();
        object IEnumerator.Current => throw new NotImplementedException();
    }

    struct ImmutableArrayEnumerable<T>
        : ITypedEnumerable<T, ImmutableArrayEnumerator<T>>
    {
        private ImmutableArray<T> source;
        public ImmutableArrayEnumerable(ImmutableArray<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableArrayEnumerator<T> GetEnumerator() => new ImmutableArrayEnumerator<T>(source);
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = source.AsSpan();
            return true;
        }
    }

    struct ImmutableHashSetEnumerable<T>
        : ITypedEnumerable<T, ImmutableHashSet<T>.Enumerator>
    {
        private ImmutableHashSet<T> source;
        public ImmutableHashSetEnumerable(ImmutableHashSet<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableHashSet<T>.Enumerator GetEnumerator() => source.GetEnumerator();
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = default;
            return false;
        }
    }

    struct ImmutableListEnumerable<T>
        : ITypedEnumerable<T, ImmutableList<T>.Enumerator>
    {
        private ImmutableList<T> source;
        public ImmutableListEnumerable(ImmutableList<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableList<T>.Enumerator GetEnumerator() => source.GetEnumerator();
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = default;
            return false;
        }
    }

    struct ImmutableQueueEnumerator<T>
        : System.Collections.Generic.IEnumerator<T>
    {
        private ImmutableQueue<T>.Enumerator e;

        public ImmutableQueueEnumerator(ImmutableQueue<T> source) => e = source.GetEnumerator();
        public void Dispose() { }

        public T Current => e.Current;

        public bool MoveNext() => e.MoveNext();

        public void Reset() => throw new NotImplementedException();
        object IEnumerator.Current => throw new NotImplementedException();
    }

    struct ImmutableQueueEnumerable<T>
        : ITypedEnumerable<T, ImmutableQueueEnumerator<T>>
    {
        private ImmutableQueue<T> source;
        public ImmutableQueueEnumerable(ImmutableQueue<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableQueueEnumerator<T> GetEnumerator() => new ImmutableQueueEnumerator<T>(source);
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = default;
            return false;
        }
    }

    struct ImmutableSortedSetEnumerable<T>
        : ITypedEnumerable<T, ImmutableSortedSet<T>.Enumerator>
    {
        private ImmutableSortedSet<T> source;
        public ImmutableSortedSetEnumerable(ImmutableSortedSet<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableSortedSet<T>.Enumerator GetEnumerator() => source.GetEnumerator();
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = default;
            return false;
        }
    }

    struct ImmutableStackEnumerator<T>
        : System.Collections.Generic.IEnumerator<T>
    {
        private ImmutableStack<T>.Enumerator e;

        public ImmutableStackEnumerator(ImmutableStack<T> source) => e = source.GetEnumerator();
        public void Dispose() { }

        public T Current => e.Current;

        public bool MoveNext() => e.MoveNext();

        public void Reset() => throw new NotImplementedException();
        object IEnumerator.Current => throw new NotImplementedException();
    }

    struct ImmutableStackEnumerable<T>
        : ITypedEnumerable<T, ImmutableStackEnumerator<T>>
    {
        private ImmutableStack<T> source;
        public ImmutableStackEnumerable(ImmutableStack<T> source) => this.source = source;

        public System.Collections.Generic.IEnumerable<T> Source => source;

        public ImmutableStackEnumerator<T> GetEnumerator() => new ImmutableStackEnumerator<T>(source);
        public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
        {
            readOnlySpan = default;
            return false;
        }
    }
}