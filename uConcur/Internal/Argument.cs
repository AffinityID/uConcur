using System;
using JetBrains.Annotations;

namespace uConcur.Internal {
    // normally I would have used https://github.com/ashmind/Argument, but I want to save on a dependency
    internal static class Argument {
        public static T NotNull<T>([InvokerParameterName] string name, T value) {
            if (value == null)
                throw new ArgumentNullException(name);
            return value;
        }
    }
}
