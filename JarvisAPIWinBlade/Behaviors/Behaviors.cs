namespace Jarvis.API.Behaviors
{
    /// <summary>
    /// The base behavior that other behaviors inherit from.
    /// </summary>
    public interface IBehaviorBase
    {
        /// <summary>
        /// Whether or not the behavior should be enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Specifies when the behavior should be called relative to other behaviors.
        /// </summary>
        int Priority { get; }
    }

    /// <summary>
    /// Implement this behavior to provide start functionality.
    /// </summary>
    public interface IStart : IBehaviorBase
    {
        /// <summary>
        /// Called when the service starts (Use JAPIService.Start() to call manually).
        /// </summary>
        void Start();
    }

    /// <summary>
    /// Implement this behavior to provide stop functionality.
    /// </summary>
    public interface IStop : IBehaviorBase
    {
        /// <summary>
        /// Called when the service stops (Use JAPIService.Stop() to call manually).
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Implement this behavior to provide functionality in the update loop.
    /// </summary>
    public interface IUpdate : IBehaviorBase
    {
        /// <summary>
        /// Called every time the service updates (Use JAPIService.Update() to call manually).
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Implement this behavior to provide functionality in the web update loop.
    /// </summary>
    public interface IWebUpdate : IBehaviorBase
    {
        /// <summary>
        /// Called when the request and response are updated (Use JAPIService.WebUpdate() to call manually).
        /// </summary>
        void WebUpdate();
    }
}
