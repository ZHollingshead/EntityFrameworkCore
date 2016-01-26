// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.NullSemantics;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.NullSemanticsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NullSemanticsQuerySqliteFixture : NullSemanticsQueryRelationalFixture<SqliteTestStore>
    {
        public static readonly string DatabaseName = "NullSemanticsQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public NullSemanticsQuerySqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlite(_connectionString);

                    using (var context = new NullSemanticsContext(_serviceProvider, optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            NullSemanticsModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });
        }

        public override NullSemanticsContext CreateContext(SqliteTestStore testStore, bool useRelationalNulls)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var sqliteOptions = optionsBuilder.UseSqlite(testStore.Connection);

            if (useRelationalNulls)
            {
                sqliteOptions.UseRelationalNulls();
            }

            var context = new NullSemanticsContext(_serviceProvider, optionsBuilder.Options);
            
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}