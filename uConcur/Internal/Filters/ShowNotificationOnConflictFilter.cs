using System;
using System.Collections.Generic;
using System.Web.Http.Filters;
using AutoMapper;
using log4net;
using RelativeTime;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;

namespace uConcur.Internal.Filters {
    public class ShowNotificationOnConflictFilter : ExceptionFilterAttribute {
        private readonly IUserService _userService;
        private readonly ILog _logger;

        public ShowNotificationOnConflictFilter(IUserService userService, ILog logger) {
            _userService = userService;
            _logger = logger;
        }

        public override void OnException(HttpActionExecutedContext context) {
            if (!PostSaveHelper.IsPostSave(context.ActionContext))
                return;

            var conflict = context.Exception as ContentConflictException;
            if (conflict == null)
                return;

            _logger.Warn(conflict.Message, conflict);
            var display = Mapper.Map<IContent, ContentItemDisplay>((IContent)conflict.Attempted);
            var changedAgo = DateTime.Now - conflict.Latest.UpdateDate;
            var changedByUser = _userService.GetUserById(((IContent)conflict.Latest).WriterId);
            // cannot use display.AddErrorNotification as it is not handled by JS properly
            display.Errors = new Dictionary<string, object> {
                { "", new[] { $"'{conflict.Attempted.Name}' was changed by user {changedByUser.Name} {changedAgo.ToHumanString()}. Please refresh the page and reapply your changes to avoid overwriting theirs." }}
            };
            context.Response = context.Request.CreateValidationErrorResponse(display);
            context.Exception = null;
        }
    }
}
