using System;
using System.Runtime.CompilerServices;
using AshMind.Extensions;
using JetBrains.Annotations;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace uConcur.Internal {
    public static class InternalExtensions {
        private const string UpdateDateOverrideKey = "uconcur:UpdateDate";

        [CanBeNull]
        public static string GetUpdateDateOverrideString([NotNull] this ContentItemSave item) {
            Argument.NotNull(nameof(item), item);
            return (string)item.AdditionalData.GetValueOrDefault(UpdateDateOverrideKey);
        }

        [CanBeNull]
        public static DateTime? GetUpdateDateOverride([NotNull] this IContent content) {
            Argument.NotNull(nameof(content), content);
            return (DateTime?)content.AdditionalData.GetValueOrDefault(UpdateDateOverrideKey);
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
    }
}