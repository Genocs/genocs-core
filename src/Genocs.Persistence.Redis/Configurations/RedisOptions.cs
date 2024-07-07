namespace Genocs.Persistence.Redis.Configurations;

/// <summary>
/// The Redis Options.
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "redis";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost";

    /// <summary>
    /// Redis instance.
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// The database Id.
    /// </summary>
    public int Database { get; set; }
}