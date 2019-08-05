﻿namespace Cistern.Linq.ChainLinq.Consumables
{
    sealed partial class Range<T>
        : Optimizations.ISkipTakeOnConsumable<T>
        , Optimizations.ICountOnConsumable
    {
        public int GetCount(bool onlyIfCheap)
        {
            if (Link is Optimizations.ICountOnConsumableLink countLink)
            {
                var count = countLink.GetCount(_count);
                if (count >= 0)
                    return count;
            }

            if (onlyIfCheap)
            {
                return -1;
            }

            var counter = new Consumer.Count<T>();
            Consume(counter);
            return counter.Result;
        }

        public T Last(bool orDefault)
        {
            var skipped = Skip(_count - 1);

            var last = new Consumer.Last<T>(orDefault);
            skipped.Consume(last);
            return last.Result;
        }

        public Consumable<T> Skip(int toSkip)
        {
            if (toSkip == 0)
                return this;

            if (Link is Optimizations.ISkipTakeOnConsumableLinkUpdate<int,T> skipLink)
            {
                checked
                {
                    var newCount = _count - toSkip;
                    if (newCount <= 0)
                    {
                        return Empty<T>.Instance;
                    }

                    var newStart = _start + toSkip;
                    var newLink = skipLink.Skip(toSkip);

                    return new Range<T>(newStart, newCount, newLink);
                }
            }
            return AddTail(new Links.Skip<T>(toSkip));
        }

        public Consumable<T> Take(int count)
        {
            if (count <= 0)
            {
                return Empty<T>.Instance;
            }

            if (count >= _count)
            {
                return this;
            }

            if (Link is Optimizations.ISkipTakeOnConsumableLinkUpdate<int, T>)
            {
                return new Range<T>(_start, count, Link);
            }

            return AddTail(new Links.Take<T>(count));
        }
    }
}
