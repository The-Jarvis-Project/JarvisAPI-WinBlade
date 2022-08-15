using Jarvis.API;
using Jarvis.API.Behaviors;

namespace APITestApp
{
    public class TestBehavior : IStart
    {
        public bool Enabled { get; set; } = true;

        public int Priority => 100;

        public void Start()
        {
            Log.Info("test log info");
        }
    }
}
