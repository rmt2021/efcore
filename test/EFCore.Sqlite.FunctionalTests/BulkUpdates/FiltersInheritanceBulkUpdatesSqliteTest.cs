﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class FiltersInheritanceBulkUpdatesSqliteTest : FiltersInheritanceBulkUpdatesTestBase<FiltersInheritanceBulkUpdatesSqliteFixture>
{
    public FiltersInheritanceBulkUpdatesSqliteTest(FiltersInheritanceBulkUpdatesSqliteFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql(
            @"DELETE FROM ""Animals"" AS ""a""
WHERE ""a"".""CountryId"" = 1 AND ""a"".""Name"" = 'Great spotted kiwi'");
    }

    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM ""Animals"" AS ""a""
WHERE ""a"".""Discriminator"" = 'Kiwi' AND ""a"".""CountryId"" = 1 AND ""a"".""Name"" = 'Great spotted kiwi'");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            @"DELETE FROM ""Countries"" AS ""c""
WHERE (
    SELECT COUNT(*)
    FROM ""Animals"" AS ""a""
    WHERE ""a"".""CountryId"" = 1 AND ""c"".""Id"" = ""a"".""CountryId"" AND ""a"".""CountryId"" > 0) > 0");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM ""Countries"" AS ""c""
WHERE (
    SELECT COUNT(*)
    FROM ""Animals"" AS ""a""
    WHERE ""a"".""CountryId"" = 1 AND ""c"".""Id"" = ""a"".""CountryId"" AND ""a"".""Discriminator"" = 'Kiwi' AND ""a"".""CountryId"" > 0) > 0");
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

        AssertSql();
    }

    public override async Task Update_where_hierarchy(bool async)
    {
        await base.Update_where_hierarchy(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Animals"" AS ""a""
    SET ""Name"" = 'Animal'

WHERE ""a"".""CountryId"" = 1 AND ""a"".""Name"" = 'Great spotted kiwi'");
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_derived(bool async)
    {
        await base.Update_where_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Animals"" AS ""a""
    SET ""Name"" = 'Kiwi'

WHERE ""a"".""Discriminator"" = 'Kiwi' AND ""a"".""CountryId"" = 1 AND ""a"".""Name"" = 'Great spotted kiwi'");
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Countries"" AS ""c""
    SET ""Name"" = 'Monovia'

WHERE (
    SELECT COUNT(*)
    FROM ""Animals"" AS ""a""
    WHERE ""a"".""CountryId"" = 1 AND ""c"".""Id"" = ""a"".""CountryId"" AND ""a"".""CountryId"" > 0) > 0");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Countries"" AS ""c""
    SET ""Name"" = 'Monovia'

WHERE (
    SELECT COUNT(*)
    FROM ""Animals"" AS ""a""
    WHERE ""a"".""CountryId"" = 1 AND ""c"".""Id"" = ""a"".""CountryId"" AND ""a"".""Discriminator"" = 'Kiwi' AND ""a"".""CountryId"" > 0) > 0");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query(async);

        AssertExecuteUpdateSql();
    }

    protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
