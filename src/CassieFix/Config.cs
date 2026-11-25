namespace CassieFix
{
    using Exiled.API.Interfaces;

    /// <summary>
    /// EXILED configuration for CassieFix.
    /// </summary>
    public sealed class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        public bool Debug { get; set; }
    }
}
