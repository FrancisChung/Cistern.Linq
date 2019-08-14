﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumables
{
    sealed partial class WhereSelectArray<T, U> : ConsumableEnumerator<U>
    {
        internal T[] Underlying { get; }
        internal Func<T, bool> Predicate { get; }
        internal Func<T, U> Selector { get; }

        int _idx;

        public WhereSelectArray(T[] array, Func<T, bool> predicate, Func<T, U> selector) =>
            (Underlying, Predicate, Selector) = (array, predicate, selector);

        public override void Consume(Consumer<U> consumer) =>
            ChainLinq.Consume.ReadOnlyMemory.Invoke(Underlying, new Links.WhereSelect<T, U>(Predicate, Selector), consumer);

        internal override ConsumableEnumerator<U> Clone() =>
            new WhereSelectArray<T, U>(Underlying, Predicate, Selector);

        public override bool MoveNext()
        {
            if (_state != 1)
                return false;

            while (_idx < Underlying.Length)
            {
                var current = Underlying[_idx++];
                if (Predicate(current))
                {
                    _current = Selector(current);
                    return true;
                }
            }

            _current = default(U);
            return false;
        }

        public override object TailLink => this;

        public override Consumable<V> ReplaceTailLink<Unknown, V>(Link<Unknown, V> newLink) =>
            throw new NotImplementedException();

        public override Consumable<U> AddTail(Link<U, U> transform) =>
            new Array<T, U>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.WhereSelect<T, U>(Predicate, Selector), transform));

        public override Consumable<V> AddTail<V>(Link<U, V> transform) =>
            new Array<T, V>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.WhereSelect<T, U>(Predicate, Selector), transform));
    }

    sealed partial class WhereSelectEnumerable<TEnumerable, TEnumerator, T, U> : ConsumableEnumerator<U>
        where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
        where TEnumerator : IEnumerator<T>
    {
        internal TEnumerable Underlying { get; }
        internal Func<T, bool> Predicate { get; }
        internal Func<T, U> Selector { get; }

        TEnumerator _enumerator;

        public WhereSelectEnumerable(TEnumerable enumerable, Func<T, bool> predicate, Func<T, U> selector) =>
            (Underlying, Predicate, Selector) = (enumerable, predicate, selector);

        public override void Consume(Consumer<U> consumer) =>
            ChainLinq.Consume.Enumerable.Invoke<TEnumerable, TEnumerator, T, U>(Underlying, new Links.WhereSelect<T, U>(Predicate, Selector), consumer);

        internal override ConsumableEnumerator<U> Clone() =>
            new WhereSelectEnumerable<TEnumerable, TEnumerator, T, U>(Underlying, Predicate, Selector);

        public override void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = default;
            }
            base.Dispose();
        }

        public override bool MoveNext()
        {
            switch (_state)
            {
                case 1:
                    _enumerator = Underlying.GetEnumerator();
                    _state = 2;
                    goto case 2;

                case 2:
                    while (_enumerator.MoveNext())
                    {
                        var current = _enumerator.Current;
                        if (Predicate(current))
                        {
                            _current = Selector(current);
                            return true;
                        }
                    }
                    _state = int.MaxValue;
                    goto default;

                default:
                    _current = default(U);
                    if (_enumerator != null)
                    {
                        _enumerator.Dispose();
                        _enumerator = default;
                    }
                    return false;
            }
        }

        public override object TailLink => this;

        public override Consumable<V> ReplaceTailLink<Unknown, V>(Link<Unknown, V> newLink) =>
            throw new NotImplementedException();

        public override Consumable<U> AddTail(Link<U, U> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, U>(Underlying, Links.Composition.Create(new Links.WhereSelect<T, U>(Predicate, Selector), transform));

        public override Consumable<V> AddTail<V>(Link<U, V> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, V>(Underlying, Links.Composition.Create(new Links.WhereSelect<T, U>(Predicate, Selector), transform));
    }
}
