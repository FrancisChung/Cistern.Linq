﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Cistern.Linq
{
    public static partial class Enumerable
    {
        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
            }

            if (count == 0)
            {
                return ChainLinq.Consumables.Empty<TResult>.Instance;
            }

            return new ChainLinq.Consumables.Repeat<TResult, TResult>(element, count, ChainLinq.Links.Identity<TResult>.Instance);
        }
    }
}
