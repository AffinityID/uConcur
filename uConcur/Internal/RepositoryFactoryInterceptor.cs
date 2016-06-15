using Castle.DynamicProxy;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace uConcur.Internal {
    public class RepositoryFactoryInterceptor : IInterceptor {
        private readonly RepositoryFactory _factory;
        private readonly ProxyGenerator _proxyGenerator;

        public RepositoryFactoryInterceptor(RepositoryFactory factory, ProxyGenerator proxyGenerator) {
            _factory = factory;
            _proxyGenerator = proxyGenerator;
        }

        public void Intercept(IInvocation invocation) {
            if (invocation.Method.Name == nameof(RepositoryFactory.CreateContentRepository)) {
                var unitOfWork = (IDatabaseUnitOfWork)invocation.Arguments[0];
                var repository = _factory.CreateContentRepository(unitOfWork);
                invocation.ReturnValue = _proxyGenerator.CreateInterfaceProxyWithTarget(
                    typeof(IContentRepository), new[] { typeof(IUnitOfWorkRepository) }, repository,
                    new ContentRepositoryInterceptor(unitOfWork)
                );
                return;
            }

            invocation.Proceed();
        }
    }
}