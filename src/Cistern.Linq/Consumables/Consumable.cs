﻿using System.Diagnostics;

namespace Cistern.Linq.Consumables
{
    internal abstract class Consumable<T, U> : Consumable<U>
    {
        public ILink<T, U> Link { get; }

        protected Consumable(ILink<T, U> link) =>
            Link = link;

        public abstract IConsumable<U> Create(ILink<T, U> first);
        public override IConsumable<U> AddTail(ILink<U, U> next) => Create(Links.Composition.Create(Link, next));

        public abstract IConsumable<V> Create<V>(ILink<T, V> first);
        public override IConsumable<V> AddTail<V>(ILink<U, V> next) => Create(Links.Composition.Create(Link, next));

        protected bool IsIdentity => ReferenceEquals(Link, Links.Identity<T>.Instance);

        public override object TailLink => Link is Links.Composition<T, U> c ? c.TailLink : Link;

        public override IConsumable<V> ReplaceTailLink<Unknown,V>(ILink<Unknown,V> newLink)
        {
            if (Link is Links.Composition<T, U> composition)
            {
                return Create(composition.ReplaceTail(newLink));
            }

            Debug.Assert(typeof(Unknown) == typeof(T));
            return Create((ILink<T,V>)(object)newLink);
        }
    }
}
