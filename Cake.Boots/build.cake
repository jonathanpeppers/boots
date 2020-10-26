var target = Argument("target", "Default");

// Just import both
#r "bin/Debug/netstandard2.0/Cake.Boots.dll"
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
    } else {
        // Let's really run through the gauntlet and install 6 .pkg files
        await Boots (Product.XamariniOS,     ReleaseChannel.Stable);
        await Boots (Product.XamarinMac,     ReleaseChannel.Stable);
        await Boots (Product.XamarinAndroid, ReleaseChannel.Stable);
        await Boots (Product.XamariniOS,     ReleaseChannel.Preview);
        await Boots (Product.XamarinMac,     ReleaseChannel.Preview);
        await Boots (Product.XamarinAndroid, ReleaseChannel.Preview);
    }
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
