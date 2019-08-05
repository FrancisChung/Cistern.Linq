﻿namespace Cistern.Linq.ChainLinq.Links
{
    sealed partial class Skip<T> : Link<T, T>
    {
        private int _toSkip;

        public Skip(int toSkip) : base(LinkType.Skip) =>
            _toSkip = toSkip;

        public override Chain<T> Compose(Chain<T> activity) =>
            new Activity(_toSkip, activity);

        sealed class Activity : Activity<T, T>
        {
            private readonly int _toSkip;

            private int _index;

            public Activity(int toSkip, Chain<T> next) : base(next) =>
                (_toSkip, _index) = (toSkip, 0);

            public override ChainStatus ProcessNext(T input)
            {
                checked
                {
                    _index++;
                }

                if (_index <= _toSkip)
                {
                    return ChainStatus.Filter;
                }
                return Next(input);
            }
        }
    }
}
