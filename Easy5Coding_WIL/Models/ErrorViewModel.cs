namespace Easy5Coding_WIL.Models
{
    /// <summary>
    /// Represents error details for displaying on the error page.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the request ID associated with the current error.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indicates whether the RequestId should be shown on the error page.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
