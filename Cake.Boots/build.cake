var target = Argument("target", "Default");

// NOTE: only Release builds work
#r "bin/Release/netstandard2.0/Cake.Boots.dll"

// NOTE: always update Mono in a separate process, run Cake twice.
Task("Mono")
    .Does(async () =>
{
    await Boots (Product.Mono, ReleaseChannel.Preview);
});

Task("Boots")
    .Does(async () =>
{
    string vs = Environment.GetEnvironmentVariable("AGENT_JOBNAME") == "vs2019" ? "2019" : "2022";
    var url = IsRunningOnWindows() ?
        $"https://github.com/codecadwallader/codemaid/releases/download/v12.0/CodeMaid.VS{vs}.v12.0.300.vsix" :
        "https://aka.ms/objective-sharpie";

    await Boots (url);

    if (IsRunningOnWindows()) {
        // Install a Firefox .msi twice
        var firefox = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/82.0/win64/en-US/Firefox%20Setup%2082.0.msi";
        await Boots (firefox);
        await Boots (firefox, fileType: FileType.msi);
    } else {
        // Let's really run through the gauntlet and install 6 .pkg files
        await Boots (Product.XamariniOS,     ReleaseChannel.Stable);
        await Boots (Product.XamarinMac,     ReleaseChannel.Stable);
        await Boots (Product.XamariniOS,     ReleaseChannel.Preview);
        await Boots (Product.XamarinMac,     ReleaseChannel.Preview);
        await Boots (Product.XamariniOS,     ReleaseChannel.Alpha);
        await Boots (Product.XamarinMac,     ReleaseChannel.Alpha);

        var settings = new BootsSettings {
            Channel = ReleaseChannel.Stable,
            Product = Product.XamarinAndroid,
            Timeout = TimeSpan.FromSeconds (200),
            ReadWriteTimeout = TimeSpan.FromMinutes (10),
            NetworkRetries = 1,
        };
        await Boots (settings);
        settings.Channel = ReleaseChannel.Preview;
        await Boots (settings);
        settings.Channel = ReleaseChannel.Alpha;
        await Boots (settings);
    }
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
