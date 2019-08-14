﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumer
{
    abstract class MaxGeneric<T, Accumulator, Maths>
        : Consumer<T, T>
        , Optimizations.IHeadStart<T>
        , Optimizations.ITailEnd<T>
        where T : struct
        where Accumulator : struct
        where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
    {
        protected bool _noData;

        public MaxGeneric() : base(default(Maths).MaxInit) =>
            _noData = true;

        public override void ChainComplete()
        {
            if (_noData)
            {
                ThrowHelper.ThrowNoElementsException();
            }
        }

        void Optimizations.IHeadStart<T>.Execute(ReadOnlySpan<T> source)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            foreach (var input in source)
            {
                if (maths.GreaterThan(input, result) || maths.IsNaN(result))
                    result = input;
            }

            Result = result;
        }

        void Optimizations.IHeadStart<T>.Execute(IList<T> source, int start, int length)
        {
            Maths maths = default;

            var result = Result;

            _noData &= length == 0;
            foreach (var t in source)
            {
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;
        }

        void Optimizations.IHeadStart<T>.Execute<Enumerator>(Optimizations.ITypedEnumerable<T, Enumerator> source)
        {
            Maths maths = default;

            var result = Result;

            foreach (var t in source)
            {
                _noData = false;
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;
        }

        void Optimizations.ITailEnd<T>.Select<S>(ReadOnlySpan<S> source, Func<S, T> selector)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            foreach (var s in source)
            {
                var t = selector(s);
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;
        }

        ChainStatus Optimizations.ITailEnd<T>.SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T> resultSelector)
        {
            Maths maths = default;

            var result = Result;

            _noData &= span.Length == 0;
            foreach (var s in span)
            {
                var t = resultSelector(source, s);
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;

            return ChainStatus.Flow;
        }

        void Optimizations.ITailEnd<T>.Where(ReadOnlySpan<T> source, Func<T, bool> predicate)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            foreach (var input in source)
            {
                if (predicate(input) && (maths.GreaterThan(input, result) || maths.IsNaN(result)))
                    result = input;
            }

            Result = result;
        }

        void Optimizations.ITailEnd<T>.WhereSelect<S>(ReadOnlySpan<S> source, Func<S, bool> predicate, Func<S, T> selector)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            foreach (var s in source)
            {
                if (predicate(s))
                {
                    var t = selector(s);
                    if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                        result = t;
                }
            }

            Result = result;
        }
    }

    sealed class MaxInt : MaxGeneric<int, int, Maths.OpsInt>
    {
        public override ChainStatus ProcessNext(int input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxLong : MaxGeneric<long, long, Maths.OpsLong>
    {
        public override ChainStatus ProcessNext(long input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxDouble : MaxGeneric<double, double, Maths.OpsDouble>
    {
        public override ChainStatus ProcessNext(double input)
        {
            _noData = false;
            if (input > Result || double.IsNaN(Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxFloat : MaxGeneric<float, double, Maths.OpsFloat>
    {
        public override ChainStatus ProcessNext(float input)
        {
            _noData = false;
            if (input > Result || float.IsNaN(Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxDecimal : MaxGeneric<decimal, decimal, Maths.OpsDecimal>
    {
        public override ChainStatus ProcessNext(decimal input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }




    sealed class MaxNullableInt : Consumer<int?, int?>
    {
        public MaxNullableInt() : base(null) { }

        public override ChainStatus ProcessNext(int? input)
        {
            var maybeValue = input.GetValueOrDefault();
            if (!Result.HasValue || (input.HasValue && maybeValue > Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxNullableLong : Consumer<long?, long?>
    {
        public MaxNullableLong() : base(null) { }

        public override ChainStatus ProcessNext(long? input)
        {
            var maybeValue = input.GetValueOrDefault();
            if (!Result.HasValue || (input.HasValue && maybeValue > Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }


    sealed class MaxNullableFloat : Consumer<float?, float?>
    {
        public MaxNullableFloat() : base(null) { }

        public override ChainStatus ProcessNext(float? input)
        {
            if (!Result.HasValue)
            {
                if (!input.HasValue)
                {
                    return ChainStatus.Flow;
                }

                Result = float.NaN;
            }

            if (input.HasValue)
            {
                var value = input.GetValueOrDefault();
                var result = Result.GetValueOrDefault();
                if (value > result || float.IsNaN(result))
                {
                    Result = value;
                }
            }

            return ChainStatus.Flow;
        }
    }

    sealed class MaxNullableDouble : Consumer<double?, double?>
    {
        public MaxNullableDouble() : base(null) { }

        public override ChainStatus ProcessNext(double? input)
        {
            if (!Result.HasValue)
            {
                if (!input.HasValue)
                {
                    return ChainStatus.Flow;
                }

                Result = double.NaN;
            }

            if (input.HasValue)
            {
                var value = input.GetValueOrDefault();
                var result = Result.GetValueOrDefault();
                if (value > result || double.IsNaN(result))
                {
                    Result = value;
                }
            }

            return ChainStatus.Flow;
        }
    }


    sealed class MaxNullableDecimal : Consumer<decimal?, decimal?>
    {
        public MaxNullableDecimal() : base(null) { }

        public override ChainStatus ProcessNext(decimal? input)
        {
            if (!Result.HasValue)
            {
                Result = input;
            }
            else if (input.HasValue)
            {
                var value = input.GetValueOrDefault();
                if (value > Result.GetValueOrDefault())
                {
                    Result = value;
                }
            }

            return ChainStatus.Flow;
        }
    }

    sealed class MaxValueType<T> : Consumer<T, T>
    {
        bool _first;

        public MaxValueType() : base(default) =>
            _first = true;

        public override ChainStatus ProcessNext(T input)
        {
            if (_first)
            {
                _first = false;
                Result = input;
            }
            else if (Comparer<T>.Default.Compare(input, Result) > 0)
            {
                Result = input;
            }

            return ChainStatus.Flow;
        }

        public override void ChainComplete()
        {
            if (_first)
            {
                ThrowHelper.ThrowNoElementsException();
            }
        }
    }

    sealed class MaxRefType<T> : Consumer<T, T>
    {
        public MaxRefType() : base(default) { }

        public override ChainStatus ProcessNext(T input)
        {
            if (Result == null || (input != null && Comparer<T>.Default.Compare(input, Result) > 0))
            {
                Result = input;
            }

            return ChainStatus.Flow;
        }
    }
}
