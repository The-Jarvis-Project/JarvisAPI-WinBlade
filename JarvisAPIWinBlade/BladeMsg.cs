namespace Jarvis.API
{
    /// <summary>
    /// Represents a message sent to or from a blade.
    /// </summary>
    public class BladeMsg
    {
        /// <summary>
        /// The blade this message corresponds to.
        /// </summary>
        public string? Origin { get; set; }

        /// <summary>
        /// The data sent to or from the blade.
        /// </summary>
        public string? Data { get; set; }
    }
}
