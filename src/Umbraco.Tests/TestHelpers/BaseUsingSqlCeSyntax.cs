using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Persistence.SqlCe;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        protected IMapperCollection Mappers { get; private set; }

        protected ISqlContext SqlContext { get; private set; }

        internal TestObjects TestObjects = new TestObjects();

        protected Sql<ISqlContext> Sql()
        {
            return NPoco.Sql.BuilderFor(SqlContext);
        }

        [SetUp]
        public virtual void Initialize()
        {
            var services = TestHelper.GetRegister();

            var ioHelper = TestHelper.IOHelper;
            var logger = new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
            var typeFinder = TestHelper.GetTypeFinder();
            var typeLoader = new TypeLoader(typeFinder, NoAppCache.Instance,
                new DirectoryInfo(ioHelper.MapPath(Constants.SystemDirectories.TempData)),
                Mock.Of<ILogger<TypeLoader>>(),
                logger,
                false);

            var composition = new UmbracoBuilder(services, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());


            services.AddUnique<ILogger>(_ => Mock.Of<ILogger>());
            services.AddUnique<ILoggerFactory>(_ => NullLoggerFactory.Instance);
            services.AddUnique<IProfiler>(_ => Mock.Of<IProfiler>());
            services.AddUnique(typeLoader);

            composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            services.AddUnique<ISqlContext>(_ => SqlContext);

            var factory = Current.Factory = composition.CreateServiceProvider();

            var pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init(), pocoMappers);
            var sqlSyntax = new SqlCeSyntaxProvider();
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SQLCe as NPoco.DatabaseTypes.SqlServerDatabaseType, pocoDataFactory, new Lazy<IMapperCollection>(() => factory.GetRequiredService<IMapperCollection>()));
            Mappers = factory.GetRequiredService<IMapperCollection>();

            SetUp();
        }

        public virtual void SetUp()
        {}
    }
}
