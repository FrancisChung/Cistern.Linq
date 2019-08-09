﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumer
{
    abstract class SumGeneric<T, Accumulator, Maths>
        : Consumer<T, T>
        , Optimizations.IWhereArray<T>
        , Optimizations.ISelectMany<T>
        , Optimizations.IPipeline<ReadOnlyMemory<T>>
        , Optimizations.IPipeline<List<T>>
        , Optimizations.IPipeline<IEnumerable<T>>
        where T : struct
        where Accumulator : struct
        where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
    {
        protected Accumulator accumulator = default(Maths).Zero;

        public SumGeneric() : base(default) { }

        public override void ChainComplete() => Result = default(Maths).Cast(accumulator);

        public void Pipeline(ReadOnlyMemory<T> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source.Span)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }
        public void Pipeline(List<T> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public void Pipeline(IEnumerable<T> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public void Where(T[] memory, Func<T, bool> predicate)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in memory)
            {
                if (predicate(x))
                    sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public ChainStatus SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T> resultSelector)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var item in span)
            {
                sum = maths.Add(sum, resultSelector(source, item));
            }
            accumulator = sum;

            return ChainStatus.Flow;
        }
    }

    abstract class SumGenericNullable<T, Accumulator, Maths>
        : Consumer<T?, T>
        , Optimizations.IWhereArray<T?>
        , Optimizations.ISelectMany<T?>
        , Optimizations.IPipeline<ReadOnlyMemory<T?>>
        , Optimizations.IPipeline<List<T?>>
        , Optimizations.IPipeline<IEnumerable<T?>>
        where T : struct
        where Accumulator : struct
        where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
    {
        protected Accumulator accumulator = default(Maths).Zero;

        public SumGenericNullable() : base(default) { }

        public override void ChainComplete() => Result = default(Maths).Cast(accumulator);

        public void Pipeline(ReadOnlyMemory<T?> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source.Span)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }
        public void Pipeline(List<T?> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public void Pipeline(IEnumerable<T?> source)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in source)
            {
                sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public void Where(T?[] memory, Func<T?, bool> predicate)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var x in memory)
            {
                if (predicate(x))
                    sum = maths.Add(sum, x);
            }

            accumulator = sum;
        }

        public ChainStatus SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T?> resultSelector)
        {
            Maths maths = default;

            Accumulator sum = accumulator;
            foreach (var item in span)
            {
                sum = maths.Add(sum, resultSelector(source, item));
            }
            accumulator = sum;

            return ChainStatus.Flow;
        }
    }

    sealed class SumDouble : SumGeneric<double, double, Maths.OpsDouble>
    {
        public override ChainStatus ProcessNext(double input)
        {
            accumulator += input;
            return ChainStatus.Flow;
        }
    }

    sealed class SumNullableDouble : SumGenericNullable<double, double, Maths.OpsDouble>
    {
        public override ChainStatus ProcessNext(double? input)
        {
            accumulator += input.GetValueOrDefault();
            return ChainStatus.Flow;
        }
    }


    sealed class SumFloat : SumGeneric<float, double, Maths.OpsFloat>
    {
        public override ChainStatus ProcessNext(float input)
        {
            accumulator += input;
            return ChainStatus.Flow;
        }
    }

    sealed class SumNullableFloat : SumGenericNullable<float, double, Maths.OpsFloat>
    {
        public override ChainStatus ProcessNext(float? input)
        {
            accumulator += input.GetValueOrDefault();
            return ChainStatus.Flow;
        }
    }

    sealed class SumInt : SumGeneric<int, int, Maths.OpsInt>
    {
        public override ChainStatus ProcessNext(int input)
        {
            accumulator += input;
            return ChainStatus.Flow;
        }
    }

    sealed class SumNullableInt : SumGenericNullable<int, int, Maths.OpsInt>
    {
        public override ChainStatus ProcessNext(int? input)
        {
            accumulator += input.GetValueOrDefault();
            return ChainStatus.Flow;
        }
    }

    sealed class SumLong : SumGeneric<long, long, Maths.OpsLong>
    {
        public override ChainStatus ProcessNext(long input)
        {
            accumulator += input;
            return ChainStatus.Flow;
        }
    }

    sealed class SumNullableLong : SumGenericNullable<long, long, Maths.OpsLong>
    {
        public override ChainStatus ProcessNext(long? input)
        {
            accumulator += input.GetValueOrDefault();
            return ChainStatus.Flow;
        }
    }

    sealed class SumDecimal : SumGeneric<decimal, decimal, Maths.OpsDecimal>
    {
        public override ChainStatus ProcessNext(decimal input)
        {
            accumulator += input;
            return ChainStatus.Flow;
        }
    }

    sealed class SumNullableDecimal : SumGenericNullable<decimal, decimal, Maths.OpsDecimal>
    {
        public override ChainStatus ProcessNext(decimal? input)
        {
            accumulator += input.GetValueOrDefault();
            return ChainStatus.Flow;
        }
    }
}
