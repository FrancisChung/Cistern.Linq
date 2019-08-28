﻿using Cistern.Linq.Utils;
using System;

namespace Cistern.Linq.ChainLinq.Consumer
{
    sealed class ToArrayKnownSize<T> : Consumer<T, T[]>
    {
        private int _index;

        public ToArrayKnownSize(int count) : base(new T[count]) =>
            _index = 0;

        public override ChainStatus ProcessNext(T input)
        {
            Result[_index++] = input;
            return ChainStatus.Flow;
        }
    }

    sealed class ToArrayViaBuilder<T>
        : Consumer<T, T[]>
        , Optimizations.IHeadStart<T>

    {
        ArrayBuilder<T> builder;

        public ToArrayViaBuilder() : base(null) =>
            builder = new ArrayBuilder<T>();

        public override ChainStatus ProcessNext(T input)
        {
            builder.Add(input);
            return ChainStatus.Flow;
        }

        public override void ChainComplete()
        {
            Result = builder.ToArray();
        }

        void Optimizations.IHeadStart<T>.Execute(ReadOnlySpan<T> source)
        {
            foreach (var item in source)
                builder.Add(item);
        }

        void Optimizations.IHeadStart<T>.Execute<Enumerator>(Optimizations.ITypedEnumerable<T, Enumerator> source)
        {
            foreach (var item in source)
                builder.Add(item);
        }
    }
}
