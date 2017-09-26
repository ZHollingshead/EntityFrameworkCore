// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalShadowEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _propertyValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object Entity => null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType)
            : base(stateManager, entityType)
        {
            _propertyValues = entityType.GetEmptyShadowValuesFactory()();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            MarkShadowPropertiesNotSet(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            _propertyValues = entityType.GetShadowValuesFactory()(valueBuffer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override T ReadShadowValue<T>(int shadowIndex)
            => _propertyValues.GetValue<T>(shadowIndex);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override object ReadPropertyValue(IPropertyBase propertyBase)
            => _propertyValues[propertyBase.GetShadowIndex()];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
            => _propertyValues[propertyBase.GetShadowIndex()] = value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable GetOrCreateCollection(INavigation navigation)
        {
            if (!(_propertyValues[navigation.GetShadowIndex()] is IEnumerable collection))
            {
                collection = navigation.GetShadowCollectionAccessor().Create();
                _propertyValues[navigation.GetShadowIndex()] = collection;
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CollectionContains(INavigation navigation, InternalEntityEntry value)
            => navigation.GetShadowCollectionAccessor().Contains(GetOrCreateCollection(navigation), value.Entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool AddToCollection(INavigation navigation, InternalEntityEntry value)
            => navigation.GetShadowCollectionAccessor().Add(GetOrCreateCollection(navigation), value.Entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveFromCollection(INavigation navigation, InternalEntityEntry value)
            => navigation.GetShadowCollectionAccessor().Remove(GetOrCreateCollection(navigation), value.Entity);
    }
}
