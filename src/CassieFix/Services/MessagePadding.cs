namespace CassieFix.Services
{
    /// <summary>
    /// Pads short outgoing CASSIE text for the affected client version.
    /// </summary>
    public static class MessagePadding
    {
        /// <summary>
        /// Minimum CASSIE content length accepted by the affected client.
        /// </summary>
        public const int MinimumLength = 1024;

        /// <summary>
        /// Returns network text without changing the audible words.
        /// </summary>
        /// <param name="content">Original announcement text.</param>
        /// <returns>Original or space-padded text.</returns>
        public static string ForClient(string content)
        {
            if (string.IsNullOrEmpty(content) || content.Length >= MinimumLength)
            {
                return content;
            }

            return content.PadRight(MinimumLength, ' ');
        }
    }
}
