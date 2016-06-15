using System;
using System.Globalization;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AshMind.Extensions;
using log4net;
using Umbraco.Core.Persistence;

namespace uConcur.Internal.Filters {
    public class CopyExtraDataOnSaveFilter : ActionFilterAttribute {
        private readonly ILog _logger;

        public CopyExtraDataOnSaveFilter(ILog logger) {
            _logger = logger;
        }

        public override void OnActionExecuting(HttpActionContext context) {
            if (!PostSaveHelper.IsPostSave(context))
                return;

            var contentItem = PostSaveHelper.GetContentItemSave(context);
            var updateDateString = contentItem.GetUpdateDateOverrideString();
            if (updateDateString == null)
                return;

            var updateDate = DateTime.ParseExact(updateDateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var content = PostSaveHelper.GetPersistedContent(contentItem);
            content.SetUpdateDateOverride(updateDate);
            _logger.DebugFormat("Set UpdateDateOverride on content {0} to '{1}'.", content.Id, updateDate);

        }
    }
}
