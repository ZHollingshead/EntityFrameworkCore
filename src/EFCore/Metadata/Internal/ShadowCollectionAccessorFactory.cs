// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ShadowCollectionAccessorFactory
    {
        private static readonly MethodInfo _create
            = typeof(ShadowCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IShadowCollectionAccessor Create([NotNull] INavigation navigation)
        {
            var targetClrType = navigation.GetTargetType().ClrType ?? typeof(object);

            var boundMethod = _create.MakeGenericMethod(typeof(HashSet<>).MakeGenericType(targetClrType), targetClrType);

            return (IShadowCollectionAccessor)boundMethod.Invoke(null, null);
        }

        [UsedImplicitly]
        private static IShadowCollectionAccessor CreateGeneric<TCollection, TElement>()
            where TCollection : class, IEnumerable<TElement>, new()
            => new ShadowCollectionAccessor<TCollection, TElement>(() => new TCollection());
    }
}
