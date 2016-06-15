using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace uConcur {
    [Serializable]
    public class ContentConflictException : Exception {
        public ContentConflictException(string message, IContentBase attempted, DateTime attemptedDate, IContentBase latest, DateTime latestDate) : base(message) {
            Attempted = attempted;
            AttemptedDate = attemptedDate;
            Latest = latest;
            LatestDate = latestDate;
        }

        public ContentConflictException(string message, IContentBase attempted, DateTime attemptedDate, IContentBase latest, DateTime latestDate, Exception inner) : base(message, inner) {
            Attempted = attempted;
            AttemptedDate = attemptedDate;
            Latest = latest;
            LatestDate = latestDate;
        }

        protected ContentConflictException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public IContentBase Attempted { get; }
        public DateTime AttemptedDate { get; }

        public IContentBase Latest  { get; }
        public DateTime LatestDate { get; }
    }
}
