﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumables
{
    sealed partial class Prepender<T>
        : Consumable<T>
        , IConsumableInternal
        , Optimizations.ICountOnConsumable
    {
        readonly T _element;
        readonly int _count;
        readonly Prepender<T> _previous;

        private int AddCount() =>
            _count < 0 ? _count : Math.Max(-1, _count + 1);

        private Prepender(Prepender<T> previous, T element, int count) =>
            (_previous, _element, _count) = (previous, element, count);

        public Prepender(T element) : this(null, element, 1) { }

        public Prepender<T> Push(T element) =>
            new Prepender<T>(this, element, AddCount());

        public override void Consume(Consumer<T> consumer)
        {
            try
            {
                var next = this;
                do
                {
                    consumer.ProcessNext(next._element);
                    next = next._previous;
                } while (next != null);
                consumer.ChainComplete();
            }
            finally
            {
                consumer.ChainDispose();
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            var next = this;
            do
            {
                yield return next._element;
                next = next._previous;
            } while (next != null);
        }

        int Optimizations.ICountOnConsumable.GetCount(bool onlyIfCheap)
        {
            if (_count < 0)
                throw new OverflowException();

            return _count;
        }
    }
}
