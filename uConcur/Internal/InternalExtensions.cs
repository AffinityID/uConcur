using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace uConcur.Internal {
    public static class InternalExtensions {
        private const string UpdateDateOverrideKey = "uconcur:UpdateDate";

        [CanBeNull]
        public static string GetUpdateDateOverrideString([NotNull] this ContentItemSave item) {
            Argument.NotNull(nameof(item), item);
            return (string)GetValueOrDefault(item.AdditionalData, UpdateDateOverrideKey);
        }

        [CanBeNull]
        public static DateTime? GetUpdateDateOverride([NotNull] this IContent content) {
            Argument.NotNull(nameof(content), content);
            return (DateTime?)GetValueOrDefault(content.AdditionalData, UpdateDateOverrideKey);
        }

        public static DateTime GetUpdateDateForConcurrencyCheck([NotNull] this IContent content) {
            Argument.NotNull(nameof(content), content);
            return content.GetUpdateDateOverride() ?? content.UpdateDate;
        }

        [CanBeNull]
        public static void SetUpdateDateOverride([NotNull] this IContent content, [CanBeNull] DateTime? value) {
            Argument.NotNull(nameof(content), content);
            if (value == null) {
                content.AdditionalData.Remove(UpdateDateOverrideKey);
                return;
            }

            content.AdditionalData[UpdateDateOverrideKey] = value;
        }

        // normally I have this in AshMind.Extensions, but wanted to avoid the dependency is this plugin
        private static object GetValueOrDefault(IDictionary<string, object> additionalData, string key) {
            object value;
            return additionalData.TryGetValue(key, out value) ? value : null;
        }
    }
}