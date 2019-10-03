﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq
{
    static class Utils
    {
        internal interface ITryFindSpecificType
        {
            string Namespace { get; }

            IConsumable<U> TryCreateSpecific<T, U, Construct>(Construct construct, IEnumerable<T> e, string name)
                where Construct : IConstruct<T, U>;

            bool TryInvoke<T, Invoker>(Invoker invoker, IEnumerable<T> e, string name)
                where Invoker : IInvoker<T>;
        }

        private static object sync = new object();
        private static (string Namespace, ITryFindSpecificType TryFind)[] finders = Array.Empty<(string, ITryFindSpecificType)>();

        internal static void Register(ITryFindSpecificType finder)
        {
            lock(sync)
            {
                foreach(var item in finders)
                {
                    if (ReferenceEquals(item.TryFind, finder))
                        return;
                }

                var newArray = new (string, ITryFindSpecificType)[finders.Length + 1];
                finders.CopyTo(newArray, 0);
                newArray[newArray.Length - 1] = (finder.Namespace, finder);
                finders = newArray;
            }
        }

        internal interface IConstruct<T, U>
        {
            IConsumable<U> Create<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T>;
        }

        internal interface IInvoker<T>
        {
            void Invoke<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T>;
        }

        internal static IConsumable<U> CreateConsumableSearch<T, U, Construct>(Construct construct, IEnumerable<T> e)
            where Construct : IConstruct<T, U>
        {
            if (finders.Length > 0)
            {
                var ty = e.GetType();

                var enumerableNamespace = ty.Namespace ?? String.Empty; // Namespace can be null - https://docs.microsoft.com/en-us/dotnet/api/system.type.namespace
                var enumerableName = ty.Name;

                foreach (var search in finders)
                {
                    if (enumerableNamespace.Equals(search.Namespace))
                    {
                        var found = search.TryFind.TryCreateSpecific<T, U, Construct>(construct, e, enumerableName);
                        if (found != null)
                            return found;
                    }
                }
            }

            return construct.Create<Optimizations.IEnumerableEnumerable<T>, IEnumerator<T>>(new Optimizations.IEnumerableEnumerable<T>(e));
        }

        internal static void InvokeSearch<T, Invoker>(Invoker invoker, IEnumerable<T> e)
            where Invoker : IInvoker<T>
        {
            if (finders.Length > 0)
            {
                var ty = e.GetType();

                var enumerableNamespace = ty.Namespace;
                var enumerableName = ty.Name;

                foreach (var search in finders)
                {
                    if (enumerableNamespace.Equals(search.Namespace))
                    {
                        var invoked = search.TryFind.TryInvoke(invoker, e, enumerableName);
                        if (invoked)
                            return;
                    }
                }
            }

            invoker.Invoke<Optimizations.IEnumerableEnumerable<T>, IEnumerator<T>>(new Optimizations.IEnumerableEnumerable<T>(e));
        }

        struct Construct<T, U>
            : IConstruct<T, U>
        {
            private readonly ILink<T, U> link;

            public Construct(ILink<T, U> link) => this.link = link;

            public IConsumable<U> Create<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T> => new Consumables.Enumerable<TEnumerable, TEnumerator, T, U>(e, link);
        }


        internal static IConsumable<U> CreateConsumable<T, U>(IEnumerable<T> e, ILink<T, U> transform)
        {
            if (e is T[] array)
            {
                return
                    array.Length == 0
                      ? Consumables.Empty<U>.Instance
                      : new Consumables.Array<T, U>(array, 0, array.Length, transform);
            }
            else if (e is List<T> list)
            {
                return new Consumables.Enumerable<Optimizations.ListEnumerable<T>, List<T>.Enumerator, T, U>(new Optimizations.ListEnumerable<T>(list), transform);
            }
            else if (e is Consumables.IConsumableProvider<T> provider)
            {
                return provider.GetConsumable(transform);
            }
            /*
             * I don't think we should use IList in the general case?
             * 
                        else if (e is IList<T> ilist)
                        {
                            return new Consumables.IList<T, U>(ilist, 0, ilist.Count, transform);
                        }
            */
            else
            {
                return CreateConsumableSearch<T, U, Construct<T, U>>(new Construct<T, U>(transform), e);
            }
        }

        struct ConstructWhere<T>
            : IConstruct<T, T>
        {
            private readonly Func<T, bool> predicate;

            public ConstructWhere(Func<T, bool> predicate) => this.predicate = predicate;

            public IConsumable<T> Create<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T> => new Consumables.WhereEnumerable<TEnumerable, TEnumerator, T>(e, predicate);
        }

        internal static IConsumable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source is IConsumable<TSource> consumable)
            {
                if (consumable.TailLink is Optimizations.IMergeWhere<TSource> optimization)
                {
                    return optimization.MergeWhere(consumable, predicate);
                }

                return consumable.AddTail(new Links.Where<TSource>(predicate));
            }
            else if (source is TSource[] array)
            {
                if (array.Length == 0)
                    return Consumables.Empty<TSource>.Instance;
                else
                    return new Consumables.WhereArray<TSource>(array, predicate);
            }
            else if (source is List<TSource> list)
            {
                return new Consumables.WhereEnumerable<Optimizations.ListEnumerable<TSource>, List<TSource>.Enumerator, TSource>(new Optimizations.ListEnumerable<TSource>(list), predicate);
            }
            else
            {
                return CreateConsumableSearch<TSource, TSource,ConstructWhere<TSource>>(new ConstructWhere<TSource>(predicate), source);
            }
        }

        struct ConstructSelect<T, U>
            : IConstruct<T, U>
        {
            private readonly Func<T, U> selector;

            public ConstructSelect(Func<T, U> selector) => this.selector = selector;

            public IConsumable<U> Create<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T> => new Consumables.SelectEnumerable<TEnumerable, TEnumerator, T, U>(e, selector);
        }

        internal static IConsumable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source is IConsumable<TSource> consumable)
            {
                if (consumable.TailLink is Optimizations.IMergeSelect<TSource> optimization)
                {
                    return optimization.MergeSelect(consumable, selector);
                }

                return consumable.AddTail(new Links.Select<TSource, TResult>(selector));
            }
            else if (source is TSource[] array)
            {
                if (array.Length == 0)
                    return Consumables.Empty<TResult>.Instance;
                else
                    return new Consumables.SelectArray<TSource, TResult>(array, selector);
            }
            else if (source is List<TSource> list)
            {
                return new Consumables.SelectEnumerable<Optimizations.ListEnumerable<TSource>, List<TSource>.Enumerator, TSource, TResult>(new Optimizations.ListEnumerable<TSource>(list), selector);
            }
            else
            {
                return CreateConsumableSearch<TSource, TResult, ConstructSelect<TSource, TResult>>(new ConstructSelect<TSource, TResult>(selector), source);
            }
        }

        internal static ILink<T, T> GetSkipLink<T>(int skip) =>
            skip == 0 ? Links.Identity<T>.Instance : new Links.Skip<T>(skip);
        internal static IEnumerable<T> Skip<T>(IEnumerable<T> source, int skip)
        {
            switch (source)
            {
                case IConsumable<T> consumable:
                    if (consumable.TailLink is Optimizations.IMergeSkipTake<T> optimization)
                    {
                        return optimization.MergeSkip(consumable, skip);
                    }

                    return consumable.AddTail(GetSkipLink<T>(skip));

                case T[] array:
                    var start = skip;
                    var count = array.Length - skip;

                    if (count <= 0)
                        return Consumables.Empty<T>.Instance;
                    else
                        return new Consumables.Array<T, T>(array, start, count, Links.Identity<T>.Instance);

                case List<T> list:
                    return new Consumables.Enumerable<Optimizations.ListEnumerable<T>, List<T>.Enumerator, T, T>(new Optimizations.ListEnumerable<T>(list), GetSkipLink<T>(skip));

                default:
                    return CreateConsumableSearch<T, T, Construct<T, T>>(new Construct<T, T>(GetSkipLink<T>(skip)), source);
            }
        }

        internal static IEnumerable<T> Take<T>(IEnumerable<T> source, int take)
        {
            if (take <= 0)
                return Consumables.Empty<T>.Instance;

            switch (source)
            {
                case IConsumable<T> consumable:
                    if (consumable.TailLink is Optimizations.IMergeSkipTake<T> optimization)
                    {
                        return optimization.MergeTake(consumable, take);
                    }
                    return consumable.AddTail(new Links.Take<T>(take));

                case T[] array:
                    if (array.Length <= 0)
                        return Consumables.Empty<T>.Instance;
                    else
                        return new Consumables.Array<T, T>(array, 0, Math.Min(take, array.Length), Links.Identity<T>.Instance);

                case List<T> list:
                    return new Consumables.Enumerable<Optimizations.ListEnumerable<T>, List<T>.Enumerator, T, T>(new Optimizations.ListEnumerable<T>(list), new Links.Take<T>(take));

                default:
                    return CreateConsumableSearch<T, T, Construct<T, T>>(new Construct<T, T>(new Links.Take<T>(take)), source);
            }
        }

        internal static IConsumable<T> AsConsumable<T>(IEnumerable<T> e)
        {
            if (e is IConsumable<T> c)
            {
                return c;
            }
            else
            {
                return CreateConsumable(e, Links.Identity<T>.Instance);
            }
        }

        // TTTransform is faster tahn TUTransform as AddTail version call can avoid
        // expensive JIT generic interface call
        internal static IConsumable<T> PushTTTransform<T>(IEnumerable<T> e, ILink<T, T> transform)
        {
            if (e is IConsumable<T> consumable)
            {
                return consumable.AddTail(transform);
            }
            else
            {
                return CreateConsumable(e, transform);
            }
        }

        // TUTrasform is more flexible but slower than TTTransform
        internal static IConsumable<U> PushTUTransform<T, U>(IEnumerable<T> e, ILink<T, U> transform)
        {
            if (e is IConsumable<T> consumable)
            {
                return consumable.AddTail(transform);
            }
            else
            {
                return CreateConsumable(e, transform);
            }
        }

        internal static Result Consume<T, Result>(IConsumable<T> consumable, Consumer<T, Result> consumer)
        {
            consumable.Consume(consumer);
            return consumer.Result;
        }

        struct Invoker<T>
            : IInvoker<T>
        {
            private readonly Consumer<T> consumer;

            public Invoker(Consumer<T> consumer) => this.consumer = consumer;

            public void Invoke<TEnumerable, TEnumerator>(TEnumerable e)
                where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerator>
                where TEnumerator : IEnumerator<T>
            {
                if (e.TryGetSourceAsSpan(out var span))
                    Cistern.Linq.Consume.ReadOnlySpan.Invoke(span, consumer);
                else
                    Cistern.Linq.Consume.Enumerable.Invoke<TEnumerable, TEnumerator, T>(e, consumer);
            }
        }

        internal static Result Consume<T, Result>(IEnumerable<T> e, Consumer<T, Result> consumer)
        {
            if (e is IConsumable<T> consumable)
            {
                consumable.Consume(consumer);
            }
            else if (e is T[] array)
            {
                if (array.Length == 0)
                {
                    try { consumer.ChainComplete(ChainStatus.Filter); }
                    finally { consumer.ChainDispose(); }
                }
                else
                {
                    Cistern.Linq.Consume.ReadOnlySpan.Invoke(array, consumer);
                }
            }
            else if (e is List<T> list)
            {
                if (list.Count == 0)
                {
                    try { consumer.ChainComplete(ChainStatus.Filter); }
                    finally { consumer.ChainDispose(); }
                }
                else
                {
                    Cistern.Linq.Consume.List.Invoke(list, consumer);
                }
            }
            else if (e is Consumables.IConsumableProvider<T> provider)
            {
                var c = provider.GetConsumable(Links.Identity<T>.Instance);
                c.Consume(consumer);
            }
            else
            {
                InvokeSearch(new Invoker<T>(consumer), e);
            }

            return consumer.Result;
        }
    }
}
