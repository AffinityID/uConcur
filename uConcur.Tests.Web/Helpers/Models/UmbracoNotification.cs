namespace uConcur.Tests.Web.Helpers.Models {
    public class UmbracoNotification {
        public UmbracoNotification(BootstrapAlertType alertType, string headline, string message) {
            AlertType = alertType;
            Headline = headline;
            Message = message;
        }

        public BootstrapAlertType AlertType { get; }
        public string Headline { get; }
        public string Message { get; }
    }
}
