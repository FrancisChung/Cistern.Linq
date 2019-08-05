﻿using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.GetEnumerator
{
    static class Repeat
    {
        public static IEnumerator<U> Get<T, U>(T element, int count, Link<T, U> link)
        {
            return new ConsumerEnumerators.Repeat<T, U>(element, count, link);
        }
    }
}
