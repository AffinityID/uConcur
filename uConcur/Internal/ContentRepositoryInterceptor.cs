using System;
using AshMind.Extensions;
using Castle.DynamicProxy;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace uConcur.Internal {
    public class ContentRepositoryInterceptor : IInterceptor {
        private readonly IDatabaseUnitOfWork _unitOfWork;

        public ContentRepositoryInterceptor(IDatabaseUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public void Intercept(IInvocation invocation) {
            if (invocation.Method.Name == nameof(IContentRepository.AddOrUpdate)) {
                ProcessAddOrUpdate(invocation);
                return;
            }

            if (invocation.Method.Name == nameof(IUnitOfWorkRepository.PersistUpdatedItem)) {
                ProcessPersistUpdatedItem(invocation);
                return;
            }

            invocation.Proceed();
        }

        private void ProcessAddOrUpdate(IInvocation invocation) {
            var content = (IContent) invocation.Arguments[0];
            if (content.HasIdentity) {
                _unitOfWork.RegisterChanged(content, (IUnitOfWorkRepository) invocation.Proxy);
                return;
            }
            invocation.Proceed();
        }

        private void ProcessPersistUpdatedItem(IInvocation invocation) {
            var content = (IContent)invocation.Arguments[0];
            EnsureNoConflict((IContentRepository)invocation.InvocationTarget, content);
            invocation.Proceed();
        }

        private void EnsureNoConflict(IContentRepository repository, IContent content) {
            var contentUpdateDate = content.GetUpdateDateOverride() ?? content.UpdateDate;
            var latestUpdatedDate = _unitOfWork.Database
                .ExecuteScalar<DateTime>("SELECT TOP 1 updateDate FROM cmsDocument WHERE nodeId = @Id AND newest = 1", new { content.Id })
                .TruncateToSeconds();
            
            if (contentUpdateDate != latestUpdatedDate) {
                var latest = repository.Get(content.Id);
                throw Conflict(content, contentUpdateDate, latest, latestUpdatedDate);
            }
        }

        private Exception Conflict(IContent attempted, DateTime attemptedDate, IContent latest, DateTime latestDate) {
            return new ContentConflictException(
                $"{attempted.ContentType.Alias} {attempted.Name} ({attempted.Id}) was changed since loaded.\r\nOriginal date was {attemptedDate}, latest date is {latestDate} (by user {latest.WriterId}).",
                attempted, attemptedDate, latest, latestDate
            );
        }
    }
}
