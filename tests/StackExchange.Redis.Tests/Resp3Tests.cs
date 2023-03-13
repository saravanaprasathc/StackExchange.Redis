using StackExchange.Redis.Tests.Helpers;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace StackExchange.Redis.Tests;

public sealed class Resp3Tests : TestBase
{
    public Resp3Tests(ITestOutputHelper output) : base(output) { }

    [Theory]
    // specify nothing
    [InlineData("someserver", false)]
    // specify *just* the protocol; sure, we'll believe you
    [InlineData("someserver,protocol=resp3", true)]
    [InlineData("someserver,protocol=resp3,$HELLO=", false)]
    [InlineData("someserver,protocol=resp3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,protocol=3", true)]
    [InlineData("someserver,protocol=3,$HELLO=", false)]
    [InlineData("someserver,protocol=3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,protocol=2", false)]
    [InlineData("someserver,protocol=2,$HELLO=", false)]
    [InlineData("someserver,protocol=2,$HELLO=BONJOUR", false)]
    // specify a pre-6 version - only used if protocol specified
    [InlineData("someserver,version=5.9", false)]
    [InlineData("someserver,version=5.9,$HELLO=", false)]
    [InlineData("someserver,version=5.9,$HELLO=BONJOUR", false)]
    [InlineData("someserver,version=5.9,protocol=resp3", true)]
    [InlineData("someserver,version=5.9,protocol=resp3,$HELLO=", false)]
    [InlineData("someserver,version=5.9,protocol=resp3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,version=5.9,protocol=3", true)]
    [InlineData("someserver,version=5.9,protocol=3,$HELLO=", false)]
    [InlineData("someserver,version=5.9,protocol=3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,version=5.9,protocol=2", false)]
    [InlineData("someserver,version=5.9,protocol=2,$HELLO=", false)]
    [InlineData("someserver,version=5.9,protocol=2,$HELLO=BONJOUR", false)]
    // specify a post-6 version; attempt by default
    [InlineData("someserver,version=6.0", true)]
    [InlineData("someserver,version=6.0,$HELLO=", false)]
    [InlineData("someserver,version=6.0,$HELLO=BONJOUR", true)]
    [InlineData("someserver,version=6.0,protocol=resp3", true)]
    [InlineData("someserver,version=6.0,protocol=resp3,$HELLO=", false)]
    [InlineData("someserver,version=6.0,protocol=resp3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,version=6.0,protocol=3", true)]
    [InlineData("someserver,version=6.0,protocol=3,$HELLO=", false)]
    [InlineData("someserver,version=6.0,protocol=3,$HELLO=BONJOUR", true)]
    [InlineData("someserver,version=6.0,protocol=2", false)]
    [InlineData("someserver,version=6.0,protocol=2,$HELLO=", false)]
    [InlineData("someserver,version=6.0,protocol=2,$HELLO=BONJOUR", false)]
    [InlineData("someserver,version=7.2", true)]
    [InlineData("someserver,version=7.2,$HELLO=", false)]
    [InlineData("someserver,version=7.2,$HELLO=BONJOUR", true)]
    public void ParseFormatConfigOptions(string configurationString, bool tryResp3)
    {
        var config = ConfigurationOptions.Parse(configurationString);
        Assert.Equal(configurationString, config.ToString(true)); // check round-trip
        Assert.Equal(configurationString, config.Clone().ToString(true)); // check clone
        Assert.Equal(tryResp3, config.TryResp3());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryConnect(bool resp3)
    {
        var options = ConfigurationOptions.Parse(GetConfiguration());
        options.Protocol = resp3 ? "resp3" : "resp2";
        using var muxer = await ConnectionMultiplexer.ConnectAsync(options, Writer);
        await muxer.GetDatabase().PingAsync();

        Assert.Equal(resp3, muxer.GetServerEndPoint(muxer.GetEndPoints().Single()).IsResp3);
    }
}
