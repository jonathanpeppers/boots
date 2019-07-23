# boots

![boots](icon.png)

boots is a dotnet global tool for "bootstrapping" vsix & pkg files.

boots is useful for "pinning" a version of Mono, Xamarin, etc. when building projects on [Azure DevOps Hosted Agents](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/agents?view=azure-devops). You don't get to _choose_ what versions of things are installed on each agent, so it makes sense to install things yourself for reproducible builds. It also allows you to install preview versions of things (or more recent!) before they come available on the Hosted Agents.

To use it:

    dotnet tool install --global boots --version 0.1.0.251-beta
    boots https://url/to/your/package

boots currently supports Windows & Mac OSX, therefore:

* On Windows - assumes the file is a `.vsix` and installs it into all instances of Visual Studio via `VSIXInstaller.exe`.
* On Mac OSX - assumes the file is a `.pkg` and installs it

## Some Examples

Install Mono, Xamarin.Android, and Xamarin.iOS on Mac OSX:

    boots https://download.mono-project.com/archive/6.0.0/macos-10-universal/MonoFramework-MDK-6.0.0.313.macos10.xamarin.universal.pkg
    boots https://aka.ms/xamarin-android-commercial-d16-2-macos
    boots https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10.3/72cb587a39c12dfaa20cd5a0b1eb60a908ff88a6/1/package/xamarin.ios-12.14.0.113.pkg

Install Xamarin.Android on Windows:

    boots https://aka.ms/xamarin-android-commercial-d16-2-windows

I got each URL from:

* [Mono Downloads](https://www.mono-project.com/download/stable/#download-mac)
* [Xamarin.Android README](https://github.com/xamarin/xamarin-android)
* [Xamarin.iOS Github Status](https://github.com/xamarin/xamarin-macios/commits/d16-2)

To _upgrade_ .NET Core on Mac OSX, assuming you have some version of .NET Core to start with:

    boots https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.301-macos-x64-installer

Url from: [.NET Core Downloads](https://dotnet.microsoft.com/download/dotnet-core/2.2)
