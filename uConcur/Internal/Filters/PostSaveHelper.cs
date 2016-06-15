using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using AshMind.Extensions;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace uConcur.Internal.Filters {
    public static class PostSaveHelper {
        private static readonly Func<ContentItemSave, IContent> PersistedContentGetter = typeof(ContentItemSave)
            .GetProperty("PersistedContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .GetGetMethod(true)
            .CreateDelegate<Func<ContentItemSave, IContent>>();

        public static bool IsPostSave(HttpActionContext context) {
            return context.ControllerContext.Controller is ContentController
                && context.ActionDescriptor.ActionName == nameof(ContentController.PostSave);
        }

        public static ContentItemSave GetContentItemSave(HttpActionContext context) => (ContentItemSave)context.ActionArguments.Values.Single();
        public static IContent GetPersistedContent(ContentItemSave item) => PersistedContentGetter(item);
    }
}
