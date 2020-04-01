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
        await Boots (Product.Mono);
        await Boots (Product.XamarinAndroid);
        await Boots (Product.XamariniOS);
        await Boots (Product.XamarinMac, ReleaseChannel.Preview);
    }
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
