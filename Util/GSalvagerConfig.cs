using Microsoft.Extensions.Configuration;

namespace GSalvager.Util
{
  internal static class GSalvagerConfig
  {
    // Read the appsettings.json using IConfiguration
    private static readonly IConfiguration _configuration;

    private const string InventoryPositionObjectKey = "InventoryPosition";
    private const string InventoryPositionStartXKey = "StartX";
    private const string InventoryPositionStartYKey = "StartY";
    private const string InventoryPositionWidthKey = "Width";
    private const string InventoryPositionHeightKey = "Height";

    private const string StartOnLaunchKey = "StartOnLaunch";
    private const string GameNameKey = "GameName";

    static GSalvagerConfig()
    {
      // Build configuration from appsettings.json
      _configuration = new ConfigurationBuilder()
          .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Make sure we set the base path for the file
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .Build();
    }

    // Properties to hold the configuration values
    public static string GameName => GetConfigValue<string>(GameNameKey, throwIfNotFound: true)!;

    // Inventory Position Settings
    public static int InvPosStartX => GetConfigValue<int>($"{InventoryPositionObjectKey}:{InventoryPositionStartXKey}", throwIfNotFound: true);
    public static int InvPosStartY => GetConfigValue<int>($"{InventoryPositionObjectKey}:{InventoryPositionStartYKey}", throwIfNotFound: true);
    public static int InvPosWidth => GetConfigValue<int>($"{InventoryPositionObjectKey}:{InventoryPositionWidthKey}", throwIfNotFound: true);
    public static int InvPosHeight => GetConfigValue<int>($"{InventoryPositionObjectKey}:{InventoryPositionHeightKey}", throwIfNotFound: true);
    public static bool StartOnLaunch => GetConfigValue<bool>(StartOnLaunchKey, false);

    // A generic method to get the value of the given config key
    private static T? GetConfigValue<T>(string key, T defaultValue = default, bool throwIfNotFound = false)
    {
      var value = _configuration[key];
      if (string.IsNullOrEmpty(value))
      {
        if (throwIfNotFound)
          throw new NullReferenceException($"Unable to find key ({key}) which is not allowed to be missing! Please add it and try again!");

        if (typeof(T) == typeof(string)) return (T)(object)string.Empty;

        Console.WriteLine($"Warning: Configuration key '{key}' is missing or empty. Using default value: {defaultValue} (type = {typeof(T).Name})");
        return defaultValue;
      }

      try
      {
        return (T)Convert.ChangeType(value, typeof(T));  // Convert the value to the required type
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Error converting configuration key '{key}' to type {typeof(T).Name}.", ex);
      }
    }
  }
}