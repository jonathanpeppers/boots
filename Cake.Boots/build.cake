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
    var url = IsRunningOnWindows() ?
        "https://github.com/codecadwallader/codemaid/releases/download/v11.0/CodeMaid.v11.0.183.vsix" :
        "https://aka.ms/objective-sharpie";

    await Boots (url);

    if (IsRunningOnWindows()) {
        // Install a Firefox .msi twice
        var firefox = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/82.0/win64/en-US/Firefox%20Setup%2082.0.msi";
        await Boots (firefox);
        await Boots (firefox, fileType: FileType.msi);
        // Install .NET 6 twice
        var dotnet = "https://download.visualstudio.microsoft.com/download/pr/2290b039-85d8-4d95-85f7-edbd9fcd118d/a64bef89625bc61db2a6832878610214/dotnet-sdk-6.0.100-preview.2.21155.3-win-x64.exe";
        await Boots (dotnet);
        await Boots (dotnet, fileType: FileType.exe);
    } else {
        // Let's really run through the gauntlet and install 6 .pkg files
        await Boots (Product.XamariniOS,     ReleaseChannel.Stable);
        await Boots (Product.XamarinMac,     ReleaseChannel.Stable);
        await Boots (Product.XamariniOS,     ReleaseChannel.Preview);
        await Boots (Product.XamarinMac,     ReleaseChannel.Preview);

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
    }
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
