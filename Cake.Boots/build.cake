var target = Argument("target", "Default");

// Just import both
#r "bin/Debug/netstandard2.0/Cake.Boots.dll"
#r "bin/Release/netstandard2.0/Cake.Boots.dll"

Task("Boots")
    .Does(async () =>
{
    var url = IsRunningOnWindows() ?
        "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/SteveCadwallader/vsextensions/CodeMaid/11.0.183/vspackage" :
        "https://aka.ms/objective-sharpie";

    await Boots (url);
});

Task("Default")
    .IsDependentOn("Boots");

RunTarget(target);
