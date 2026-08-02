using Microsoft.Extensions.Configuration;
using MoodPlaylistApi.Helpers;

namespace MoodPlaylistApi.Tests.Helpers;

[Collection("StaticHelperSettings")]
public sealed class HelperTests
{
    [Fact(DisplayName = "Hash helper configuration loads the configured secret")]
    public void Configure_HashSecretProvided_SetsSecretKey()
    {
        var configuration = Configuration(new Dictionary<string, string?>
        {
            ["HashHelper:SecretKey"] = "hash-secret"
        });

        HashHelperSettings.Configure(configuration);

        Assert.Equal("hash-secret", HashHelperSettings.SecretKey);
    }

    [Fact(DisplayName = "Hash helper configuration rejects a missing secret")]
    public void Configure_HashSecretMissing_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => HashHelperSettings.Configure(Configuration([])));

        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact(DisplayName = "Code generator preserves the prefix and adds a ten-character suffix")]
    public async Task Generate_PrefixProvided_ReturnsExpectedCodeShape()
    {
        const string prefix = "USR-";

        var code = await CodeGenerator.Generate(prefix);

        Assert.StartsWith(prefix, code);
        Assert.Equal(prefix.Length + 10, code.Length);
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}

[CollectionDefinition("StaticHelperSettings", DisableParallelization = true)]
public sealed class StaticHelperSettingsCollection;
