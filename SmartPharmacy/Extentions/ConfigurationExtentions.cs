namespace SmartPharmacy.PL.Extentions
{
    public static class ConfigurationExtentions
    {
        /// <summary>
        /// A missing connection string surfaces deep inside whichever component happens to open a
        /// connection first - on this project that is Hangfire, whose stack trace says nothing
        /// about configuration. Failing here instead names the setting that is actually missing.
        /// </summary>
        public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
        {
            var connectionString = configuration.GetConnectionString(name);

            if (!string.IsNullOrWhiteSpace(connectionString))
                return connectionString;

            throw new InvalidOperationException(
                $"Connection string '{name}' is missing or empty. " +
                $"Set it in user-secrets for local development, or as the environment variable " +
                $"'ConnectionStrings__{name}' on the host. Note the double underscore: a single " +
                $"underscore is silently ignored by the configuration binder.");
        }
    }
}
