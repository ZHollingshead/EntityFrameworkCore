// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ShadowCollectionAccessor<TCollection, TElement> : IShadowCollectionAccessor
        where TCollection : class, IEnumerable<TElement>
    {
        private readonly Func<TCollection> _createCollection;
        private readonly bool _targetShadow;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type CollectionType => typeof(TCollection);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ShadowCollectionAccessor([CanBeNull] Func<TCollection> createCollection)
        {
            _createCollection = createCollection;
            // This should change once #749 or #9914 is implemented
            _targetShadow = typeof(TElement) == typeof(object);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Add(object collection, object value)
        {
            if (_targetShadow)
            {
                return false;
            }

            var genericCollection = (ICollection<TElement>)collection;
            var element = (TElement)value;

            if (!genericCollection.Contains(element))
            {
                genericCollection.Add(element);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRange(object collection, IEnumerable<object> values)
        {
            if (_targetShadow)
            {
                return;
            }

            var genericCollection = (ICollection<TElement>)collection;
            foreach (TElement value in values)
            {
                if (!genericCollection.Contains(value))
                {
                    genericCollection.Add(value);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable Create() => _createCollection();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable Create(IEnumerable<object> values)
        {
            var collection = (ICollection<TElement>)Create();
            if (_targetShadow)
            {
                return collection;
            }

            foreach (TElement value in values)
            {
                collection.Add(value);
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Contains(object collection, object value)
            => !_targetShadow && ((ICollection<TElement>)collection).Contains((TElement)value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Remove(object collection, object value)
            => ((ICollection<TElement>)collection).Remove((TElement)value);
    }
}
