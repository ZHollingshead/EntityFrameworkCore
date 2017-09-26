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
    public class InternalMixedEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _shadowValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalMixedEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity)
            : base(stateManager, entityType)
        {
            Entity = entity;
            _shadowValues = entityType.GetEmptyShadowValuesFactory()();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            MarkShadowPropertiesNotSet(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalMixedEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            Entity = entity;
            _shadowValues = entityType.GetShadowValuesFactory()(valueBuffer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object Entity { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override T ReadShadowValue<T>(int shadowIndex)
            => _shadowValues.GetValue<T>(shadowIndex);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override object ReadPropertyValue(IPropertyBase propertyBase)
            => !propertyBase.IsShadowProperty
                ? base.ReadPropertyValue(propertyBase)
                : _shadowValues[propertyBase.GetShadowIndex()];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            if (!propertyBase.IsShadowProperty)
            {
                base.WritePropertyValue(propertyBase, value);
            }
            else
            {
                _shadowValues[propertyBase.GetShadowIndex()] = value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable GetOrCreateCollection(INavigation navigation)
        {
            if (!navigation.IsShadowProperty)
            {
                return base.GetOrCreateCollection(navigation);
            }

            if (!(_shadowValues[navigation.GetShadowIndex()] is IEnumerable collection))
            {
                collection = navigation.GetShadowCollectionAccessor().Create();
                _shadowValues[navigation.GetShadowIndex()] = collection;
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CollectionContains(INavigation navigation, InternalEntityEntry value)
            => navigation.IsShadowProperty
                ? navigation.GetShadowCollectionAccessor().Contains(GetOrCreateCollection(navigation), value.Entity)
                : base.CollectionContains(navigation, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool AddToCollection(INavigation navigation, InternalEntityEntry value)
            => navigation.IsShadowProperty
                ? navigation.GetShadowCollectionAccessor().Add(GetOrCreateCollection(navigation), value.Entity)
                : base.AddToCollection(navigation, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveFromCollection(INavigation navigation, InternalEntityEntry value)
        {
            if (navigation.IsShadowProperty)
            {
                navigation.GetShadowCollectionAccessor().Remove(GetOrCreateCollection(navigation), value.Entity);
            }
            else
            {
                base.RemoveFromCollection(navigation, value);
            }
        }
    }
}
