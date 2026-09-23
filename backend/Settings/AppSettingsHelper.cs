namespace ECommerceApi.Settings
{
    public static class AppSettingsHelper
    {
        // Binds a section of appsettings (plus any environment-specific overrides) to a
        // typed settings class, and fails at startup with a clear message if it's missing.
        public static T GetRequiredSettings<T>(this IConfiguration config, string sectionName) where T : class
        {
            var settings = config.GetSection(sectionName).Get<T>();

            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"Missing configuration section '{sectionName}'. Add it to appsettings.json or appsettings.{{Environment}}.json.");
            }

            return settings;
        }
    }
}
