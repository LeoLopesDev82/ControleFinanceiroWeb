using System;

namespace ControleFinanceiroWeb.Services.Security
{
    // Identifies this run of the application. Sessions carry the value, so
    // cookies issued by an earlier run are rejected and the PIN is asked for
    // again. The household starts the application when it is needed rather
    // than leaving it up, which makes a restart the moment to re-authenticate.
    public static class AppInstance
    {
        public static readonly string Id = Guid.NewGuid().ToString("N");
    }
}
