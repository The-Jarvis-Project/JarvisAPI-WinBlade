namespace Jarvis.API
{
    public static class ComSystem
    {
        public static async Task<bool> SendResponse(string msg) => await JAPIService.Internal.TrySendResponse(msg);

        public static BladeMsg? Request() => JAPIService.Internal.GetRequest();
    }
}
