namespace Jarvis.API
{
    /// <summary>
    /// System for sending and receiving messages from JarvisLinker.
    /// </summary>
    public static class ComSystem
    {
        /// <summary>
        /// Sends a blade message.
        /// </summary>
        /// <param name="msg">The text to send</param>
        /// <returns>An asyncronous function represeting the sending of the message</returns>
        public static async Task<bool> SendResponse(string msg) => await JAPIService.Internal.TrySendResponse(msg);

        /// <summary>
        /// Gets the current blade command if one exists.
        /// </summary>
        /// <returns>The blade message command or null</returns>
        public static BladeMsg? Command() => JAPIService.Internal.GetCmd();

        /// <summary>
        /// Deletes the current blade command if one exists.
        /// </summary>
        /// <returns>An asyncronous function represeting the deletion of the command</returns>
        public static Task<bool> ConsumeCmd() => JAPIService.Internal.ConsumeCmd();
    }
}
