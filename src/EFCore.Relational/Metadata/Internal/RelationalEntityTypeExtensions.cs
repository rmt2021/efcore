// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalEntityTypeExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int MaxEntityTypesSharingTable = 128;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IForeignKey> FindDeclaredReferencingRowInternalForeignKeys(
        this IEntityType entityType,
        StoreObjectIdentifier storeObject)
    {
        if (entityType.IsMappedToJson())
        {
            yield break;
        }

        foreach (var foreignKey in entityType.GetDeclaredReferencingForeignKeys())
        {
            var dependentPrimaryKey = foreignKey.DeclaringEntityType.FindPrimaryKey();
            if (dependentPrimaryKey == null)
            {
                yield break;
            }

            if (!foreignKey.PrincipalKey.IsPrimaryKey()
                || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                || !foreignKey.Properties.SequenceEqual(dependentPrimaryKey.Properties)
                || !IsMapped(foreignKey, storeObject))
            {
                continue;
            }

            yield return foreignKey;
        }

        static bool IsMapped(IReadOnlyForeignKey foreignKey, StoreObjectIdentifier storeObject)
            => (StoreObjectIdentifier.Create(foreignKey.DeclaringEntityType, storeObject.StoreObjectType) == storeObject
                    || foreignKey.DeclaringEntityType.GetMappingFragments(storeObject.StoreObjectType).Any(f => f.StoreObject == storeObject))
                && (StoreObjectIdentifier.Create(foreignKey.PrincipalEntityType, storeObject.StoreObjectType) == storeObject
                    || foreignKey.PrincipalEntityType.GetMappingFragments(storeObject.StoreObjectType).Any(f => f.StoreObject == storeObject));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<ITableMappingBase> GetViewOrTableMappings(this IEntityType entityType)
        => (IEnumerable<ITableMappingBase>?)(entityType.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.ViewMappings)
                ?? entityType.FindRuntimeAnnotationValue(RelationalAnnotationNames.TableMappings))
            ?? Enumerable.Empty<ITableMappingBase>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IProperty> GetNonPrincipalSharedNonPkProperties(this IEntityType entityType, ITableBase table)
    {
        var principalEntityTypes = new HashSet<IEntityType>();
        PopulatePrincipalEntityTypes(table, entityType, principalEntityTypes);
        foreach (var property in entityType.GetProperties())
        {
            if (property.IsPrimaryKey())
            {
                continue;
            }

            var propertyMappings = table.FindColumn(property)!.PropertyMappings;
            if (propertyMappings.Count() > 1
                && propertyMappings.Any(pm => principalEntityTypes.Contains(pm.TableMapping.EntityType)))
            {
                continue;
            }

            yield return property;
        }

        static void PopulatePrincipalEntityTypes(ITableBase table, IEntityType entityType, HashSet<IEntityType> entityTypes)
        {
            foreach (var linkingFk in table.GetRowInternalForeignKeys(entityType))
            {
                entityTypes.Add(linkingFk.PrincipalEntityType);
                PopulatePrincipalEntityTypes(table, linkingFk.PrincipalEntityType, entityTypes);
            }
        }
    }
}
