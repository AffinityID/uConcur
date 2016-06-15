using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using log4net;
using uConcur.Internal;
using uConcur.Internal.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;

namespace uConcur {
    public class StartupHandler : ApplicationEventHandler {
        private readonly ILog _logger = LogManager.GetLogger(typeof(StartupHandler));

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) {
            var configuration = GlobalConfiguration.Configuration;
            configuration.Filters.Add(new CopyExtraDataOnSaveFilter(
                LogManager.GetLogger(typeof(CopyExtraDataOnSaveFilter))
            ));
            configuration.Filters.Add(new ShowNotificationOnConflictFilter(
                applicationContext.Services.UserService,
                LogManager.GetLogger(typeof(ShowNotificationOnConflictFilter))
            ));

            ReplaceRepositoryFactory(applicationContext);
        }

        private static DatabaseSchemaHelper CreateDatabaseSchemaHelper(ApplicationContext applicationContext) {
            return new DatabaseSchemaHelper(
                applicationContext.DatabaseContext.Database,
                ResolverBase<LoggerResolver>.Current.Logger,
                applicationContext.DatabaseContext.SqlSyntax
            );
        }
        
        private void ReplaceRepositoryFactory(ApplicationContext applicationContext) {
            var proxyGenerator = new ProxyGenerator();
            var contentService = applicationContext.Services.ContentService;
            var contentServiceType = contentService.GetType();

            var factoryField = FindFactoryField(contentServiceType);
            var factory = (RepositoryFactory)factoryField.GetValue(contentService);
            var factoryProxy = proxyGenerator.CreateClassProxyWithTarget(factory, new RepositoryFactoryInterceptor(factory, proxyGenerator));
            factoryField.SetValue(contentService, factoryProxy);
            _logger.Info($"Replaced repository factory of type '{factory.GetType().FullName}' with a proxy using '{typeof(RepositoryFactoryInterceptor).FullName}'.");
        }

        [NotNull]
        private FieldInfo FindFactoryField(Type currentType, Type startingType = null) {
            startingType = startingType ?? currentType;
            var candidates = currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.FieldType == typeof(RepositoryFactory))
                .ToArray();

            if (candidates.Length > 1)
                throw new NotSupportedException($"Found more than one RepositoryFactory field on '{startingType.FullName}': {string.Join(",", (object[])candidates)}");

            if (candidates.Length == 0) {
                if (currentType.BaseType == typeof(object))
                    throw new NotSupportedException($"Could not find RepositoryFactory field on '{startingType.FullName}.");

                return FindFactoryField(currentType.BaseType, startingType);
            }

            return candidates[0];
        }
    }
}
