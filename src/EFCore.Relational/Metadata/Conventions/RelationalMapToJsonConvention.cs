﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures default settings for an entity mapped to a JSON column.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalMapToJsonConvention : IEntityTypeAnnotationChangedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalMapToJsonConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public RelationalMapToJsonConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name != RelationalAnnotationNames.JsonColumnName)
        {
            return;
        }

        var jsonColumnName = annotation?.Value as string;
        if (!string.IsNullOrEmpty(jsonColumnName))
        {
            var jsonColumnTypeMapping = ((IRelationalTypeMappingSource)Dependencies.TypeMappingSource).FindMapping(
                typeof(JsonElement))!;

            entityTypeBuilder.Metadata.SetJsonColumnTypeMapping(jsonColumnTypeMapping);
        }
        else
        {
            entityTypeBuilder.Metadata.SetJsonColumnTypeMapping(null);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var jsonEntityType in modelBuilder.Metadata.GetEntityTypes().Where(e => e.IsMappedToJson()))
        {
            foreach (var enumProperty in jsonEntityType.GetDeclaredProperties().Where(p => p.ClrType.IsEnum))
            {
                // by default store enums as strings - values should be human-readable
                enumProperty.Builder.HasConversion(typeof(string));
            }
        }
    }
}
