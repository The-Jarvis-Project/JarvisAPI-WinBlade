using Jarvis.API.Behaviors;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Jarvis.API
{
    /// <summary>
    /// An object representing an instance of the Jarvis API, or in this circumstance, a Jarvis Blade.
    /// </summary>
    public class JAPIService
    {
        /// <summary>
        /// The name of this blade instance.
        /// </summary>
        public string BladeName { get; private set; }

        private const int updateMs = 100, logMs = 90000, webRequestMs = 2000;
        private readonly Timer updateTimer, logTimer, webRequestTimer;

        private readonly List<IUpdate> updateBehaviors;
        private readonly List<IWebUpdate> webBehaviors;
        private readonly List<IStart> startBehaviors;
        private readonly List<IStop> stopBehaviors;

        private readonly List<string> updateNames;
        private readonly List<string> webNames;
        private readonly List<string> startNames;
        private readonly List<string> stopNames;

        private BladeMsg? request, response;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string 
            cmdUrl = "https://jarvislinker.azurewebsites.net/api/BladeCommands",
            responseUrl = "https://jarvislinker.azurewebsites.net/api/BladeResponses";

        private static JAPIService? singleton;

        /// <summary>
        /// Creates a new JAPIService instance and hangs up the current thread,
        /// forcing it to act as the main loop for the behavior system from then on.
        /// </summary>
        /// <param name="bladeName">The name of the blade instance to create</param>
        /// <param name="startProgram">Whether or not to start the main loop right after creation</param>
        public JAPIService(string bladeName, bool startProgram)
        {
            singleton = this;
            BladeName = bladeName;

            updateBehaviors = new List<IUpdate>();
            webBehaviors = new List<IWebUpdate>();
            startBehaviors = new List<IStart>();
            stopBehaviors = new List<IStop>();

            updateNames = new List<string>();
            webNames = new List<string>();
            startNames = new List<string>();
            stopNames = new List<string>();

            string behaviorsText = string.Empty;
            Type[] types = Assembly.GetCallingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsClass && types[i].GetInterface(nameof(IBehaviorBase)) != null)
                {
                    object? inst = Activator.CreateInstance(types[i]);
                    if (inst != null)
                    {
                        if (types[i].GetInterface(nameof(IUpdate)) != null)
                        {
                            updateBehaviors.Add((IUpdate)inst);
                            behaviorsText += "\n" + types[i].Name + " (Update)";
                        }
                        if (types[i].GetInterface(nameof(IWebUpdate)) != null)
                        {
                            webBehaviors.Add((IWebUpdate)inst);
                            behaviorsText += "\n" + types[i].Name + " (WebUpdate)";
                        }
                        if (types[i].GetInterface(nameof(IStart)) != null)
                        {
                            startBehaviors.Add((IStart)inst);
                            behaviorsText += "\n" + types[i].Name + " (Start)";
                        }
                        if (types[i].GetInterface(nameof(IStop)) != null)
                        {
                            stopBehaviors.Add((IStop)inst);
                            behaviorsText += "\n" + types[i].Name + " (Stop)";
                        }
                    }
                }
            }
            updateBehaviors.Sort(CompareBehaviors);
            webBehaviors.Sort(CompareBehaviors);
            startBehaviors.Sort(CompareBehaviors);
            stopBehaviors.Sort(CompareBehaviors);
            Log.Info("Initialized Behaviors: " + behaviorsText);

            updateTimer = new Timer(updateMs);
            logTimer = new Timer(logMs);
            webRequestTimer = new Timer(webRequestMs);

            if (startProgram) Start();
        }

        /// <summary>
        /// Compares two behaviors' priorities.
        /// </summary>
        /// <param name="x">The first behavior</param>
        /// <param name="y">The second behavior</param>
        /// <returns>The integer priority comparison</returns>
        public static int CompareBehaviors(IBehaviorBase x, IBehaviorBase y)
        {
            if (x.Priority < y.Priority) return -1;
            else if (x.Priority > y.Priority) return 1;
            return 0;
        }

        /// <summary>
        /// Call to manually invoke Start() on behaviors and start the service.
        /// </summary>
        public void Start()
        {
            Log.Info("Started");
            try
            {
                for (int i = 0; i < startBehaviors.Count; i++)
                    if (startBehaviors[i].Enabled)
                        startBehaviors[i].Start();
            }
            catch (Exception ex)
            {
                Log.Error("Error On Start: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace);
            }

            updateTimer.Start();
            logTimer.Start();
            webRequestTimer.Start();
            updateTimer.Elapsed += (_, _) => Update();
            logTimer.Elapsed += LogTimer_Elapsed;
            webRequestTimer.Elapsed += (_, _) => _ = WebUpdateAsync();

            _ = Internal.PostBladeToServer();
            InternalMainLoop().Wait();
        }

        /// <summary>
        /// Runs internally to keep this process alive.
        /// Forces this thread to hang forever, acting as an infinite loop.
        /// </summary>
        /// <returns>An asyncronous task represing the internal main loop</returns>
        private static async Task InternalMainLoop()
        {
            while (true) await Task.Delay(100);
        }

        /// <summary>
        /// Updates periodically and logs the state of the Jarvis API.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Arguments for when the event is called</param>
        private void LogTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            string log = "- Status Check -\nBehaviors: " +
                (updateBehaviors.Count + webBehaviors.Count + startBehaviors.Count + stopBehaviors.Count);
            Log.Info(log);
        }

        /// <summary>
        /// Call to manually invoke Update() on behaviors.
        /// </summary>
        public void Update()
        {
            try
            {
                for (int i = 0; i < updateBehaviors.Count; i++)
                    if (updateBehaviors[i].Enabled)
                        updateBehaviors[i].Update();
            }
            catch (Exception ex)
            {
                Log.Error("Error On Update: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Call to manually invoke WebUpdate() on behaviors.
        /// </summary>
        /// <returns>An asyncronous task representing a web update</returns>
        public async Task WebUpdateAsync()
        {
            webRequestTimer.Stop();
            HttpResponseMessage requestMsg = await client.GetAsync(cmdUrl),
                responseMsg = await client.GetAsync(responseUrl);
            if (requestMsg.IsSuccessStatusCode && responseMsg.IsSuccessStatusCode)
            {
                try
                {
                    request = null;
                    response = null;
                    string requestsJson = await requestMsg.Content.ReadAsStringAsync(),
                        responsesJson = await responseMsg.Content.ReadAsStringAsync();
                    List<BladeMsg>? allBladeRequests =
                        JsonConvert.DeserializeObject<List<BladeMsg>>(requestsJson);
                    List<BladeMsg>? allBladeResponses =
                        JsonConvert.DeserializeObject<List<BladeMsg>>(responsesJson);
                    if (allBladeRequests != null)
                    {
                        foreach (BladeMsg cur in allBladeRequests)
                        {
                            if (cur.Origin == BladeName)
                            {
                                request = cur;
                                break;
                            }
                        }
                    }
                    if (allBladeResponses != null)
                    {
                        foreach (BladeMsg cur in allBladeResponses)
                        {
                            if (cur.Origin == BladeName)
                            {
                                response = cur;
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < webBehaviors.Count; i++)
                        if (webBehaviors[i].Enabled)
                            webBehaviors[i].WebUpdate();
                }
                catch (Exception ex)
                {
                    Log.Error("Error On WebUpdate: " + ex.Message + "\n\n" +
                        ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace);
                }
            }
            else Log.Info("Web Update Failed\nCode: " + requestMsg.StatusCode.ToString());
            webRequestTimer.Start();
        }

        /// <summary>
        /// Call to manually invoke Stop() on behaviors and stop this service.
        /// </summary>
        public void Stop()
        {
            updateTimer.Dispose();
            logTimer.Dispose();
            webRequestTimer.Dispose();

            try
            {
                for (int i = 0; i < stopBehaviors.Count; i++)
                    if (stopBehaviors[i].Enabled)
                        stopBehaviors[i].Stop();

                client.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Error On Stop: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace);
            }
            Log.Info("Stopped");
        }

        /// <summary>
        /// Handles functions only used internally in the blade API.
        /// </summary>
        internal static class Internal
        {
            /// <summary>
            /// Posts a blade to the server allowing the Jarvis Service to add it to the list of blades tracked.
            /// </summary>
            /// <returns>An asyncronous task for this function</returns>
            public static async Task<bool> PostBladeToServer() => await TrySendResponse("--postblade");

            /// <summary>
            /// Sends a response BladeMsg to the server for the Jarvis Service to process.
            /// </summary>
            /// <param name="msg">The message to send</param>
            /// <returns>An asyncronous task for this function</returns>
            public static async Task<bool> TrySendResponse(string msg)
            {
                try
                {
                    if (singleton?.response == null)
                    {
                        BladeMsg dto = new BladeMsg
                        {
                            Origin = singleton?.BladeName,
                            Data = msg,
                        };
                        string json = JsonConvert.SerializeObject(dto);
                        StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                        return (await client.PostAsync(responseUrl, jsonContent)).IsSuccessStatusCode;
                    }
                    else
                    {
                        Log.Warn("Response already exists!");
                        return false;
                    }
                }
                catch (Exception x)
                {
                    Log.Warn("Couldn't send response: " + x.Message);
                    return false;
                }
            }

            /// <summary>
            /// Gets the current request this blade has pending, or null if none is available.
            /// </summary>
            /// <returns>The request pending or a null value</returns>
            public static BladeMsg? GetRequest()
            {
                if (singleton != null) return singleton.request;
                return null;
            }
        }
    }
}
