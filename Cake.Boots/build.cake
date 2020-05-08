var target = Argument("target", "Default");

// Just import both
#r "bin/Debug/netstandard2.0/Cake.Boots.dll"
#r "bin/Release/netstandard2.0/Cake.Boots.dll"

Task("Boots")
    .Does(async () =>
{
    var url = IsRunningOnWindows() ?
        "https://github.com/codecadwallader/codemaid/releases/download/v11.0/CodeMaid.v11.0.183.vsix" :
        "https://aka.ms/objective-sharpie";

    await Boots (url);

    if (!IsRunningOnWindows()) {
        //Let's really run through the gauntlet and install 6 .pkg files
        await Boots (Product.XamariniOS,     ReleaseChannel.Stable);
        await Boots (Product.XamarinMac,     ReleaseChannel.Stable);
        await Boots (Product.XamarinAndroid, ReleaseChannel.Stable);
        await Boots (Product.Mono,           ReleaseChannel.Preview);
        await Boots (Product.XamariniOS,     ReleaseChannel.Preview);
        await Boots (Product.XamarinMac,     ReleaseChannel.Preview);
        await Boots (Product.XamarinAndroid, ReleaseChannel.Preview);
    }
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
