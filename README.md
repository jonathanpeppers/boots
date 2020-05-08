# boots

![boots](icon.png)

| NuGet |
| --           |
| `boots` <br/> [![NuGet](https://img.shields.io/nuget/dt/boots.svg)](https://www.nuget.org/packages/boots) |
| `Cake.Boots` <br/> [![NuGet](https://img.shields.io/nuget/dt/Cake.Boots.svg)](https://www.nuget.org/packages/Cake.Boots) |

| Azure DevOps | App Center | Github Actions | Bitrise |
| --           | --         | --             | --      |
| [![DevOps](https://jopepper.visualstudio.com/Jon%20Peppers%20OSS/_apis/build/status/jonathanpeppers.boots?branchName=master)](https://jopepper.visualstudio.com/Jon%20Peppers%20OSS/_build/latest?definitionId=8&branchName=master) | [![AppCenter](https://build.appcenter.ms/v0.1/apps/87931b9c-e617-4fb7-bfa9-9bfd74f39abb/branches/master/badge)][appcenter] | [![Github Actions](https://github.com/jonathanpeppers/boots/workflows/GitHub%20Actions/badge.svg)](https://github.com/jonathanpeppers/boots/actions) | [![Bitrise](https://app.bitrise.io/app/bb148b2cc62339da/status.svg?token=TEhuHdoNElmh2w8uQ-mYcQ&branch=master)](https://app.bitrise.io/app/bb148b2cc62339da) |

`boots` is a [.NET global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) for "bootstrapping" vsix & pkg files.

`boots` is useful for pinning a version of Mono, Xamarin, etc. when building projects on [Azure DevOps Hosted Agents](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/agents?view=azure-devops). You don't get to _choose_ what versions of things are installed on each agent, so it makes sense to install things yourself for reproducible builds. It also allows you to install preview versions of things (or more recent!) before they come preinstalled on Hosted build agents.

## Use it

    dotnet tool install --global boots
    boots https://url/to/your/package

`boots` currently supports Windows & Mac OSX, therefore:

* On Windows - assumes the file is a `.vsix` and installs it into all instances of Visual Studio via `VSIXInstaller.exe`.
* On Mac OSX - assumes the file is a `.pkg` and installs it

[@motz](https://www.twitch.tv/jamesmontemagno) gives a quick walkthrough on [Twitch.tv](https://clips.twitch.tv/embed?clip=FunAgitatedCourgetteSmoocherZ):

[![Twitch](https://clips-media-assets2.twitch.tv/AT-cm%7C502459489-preview-260x147.jpg)](https://clips.twitch.tv/embed?clip=FunAgitatedCourgetteSmoocherZ)

### Use the Azure Pipeline Extension Task

Install the extension into your DevOps instance and add the task to a build or release, or use it from YAML:

```yaml
steps:
- task: Boots@1
  displayName: Install Xamarin.Android
  inputs:
    uri: https://aka.ms/xamarin-android-commercial-d16-4-windows
```

You can install the [Boots Extension from the VS Marketplace](https://marketplace.visualstudio.com/items?itemName=pjcollins.azp-utilities-boots).

See the [Boots Task Extension Source](https://github.com/pjcollins/azure-web-extensions#use-in-your-yaml-pipeline) for more details.

If you don't want to use the extension, alternatively you can:

```yaml
variables:
  DOTNET_CLI_TELEMETRY_OPTOUT: true
steps:
- script: |
    dotnet tool install --global boots
    boots https://aka.ms/xamarin-android-commercial-d16-4-windows
```

`DOTNET_CLI_TELEMETRY_OPTOUT` is optional.

`DOTNET_SKIP_FIRST_TIME_EXPERIENCE` is also a good idea if you are running on a .NET Core older than 3.0.

## Some Examples

Install Mono, Xamarin.Android, and Xamarin.iOS on Mac OSX:

    boots https://download.mono-project.com/archive/6.4.0/macos-10-universal/MonoFramework-MDK-6.4.0.198.macos10.xamarin.universal.pkg
    boots https://aka.ms/xamarin-android-commercial-d16-4-macos
    boots https://download.visualstudio.microsoft.com/download/pr/5a678460-107f-4fcf-8764-80419bc874a0/3f78c6826132f6f8569524690322adba/xamarin.ios-13.8.1.17.pkg

Install Xamarin.Android on Windows:

    boots https://aka.ms/xamarin-android-commercial-d16-4-windows

I got each URL from:

* [Mono Downloads](https://www.mono-project.com/download/stable/#download-mac)
* [Xamarin.Android README](https://github.com/xamarin/xamarin-android)
* [Xamarin.iOS Github Status](https://github.com/xamarin/xamarin-macios/commits/d16-4)

### New Xamarin Hotness

By querying the Visual Studio updater manifests, `boots` 1.0.2.x allows you to install the latest versions of Xamarin or Mono from the stable or preview channels.

Some examples:
```
dotnet tool install --global boots --version 1.0.2.421
boots --stable Mono
boots --preview XamarinAndroid
boots --preview XamariniOS
boots --preview XamarinMac
```
This would install the latest stable Mono and the latest previews for Xamarin.Android, Xamarin.iOS, and Xamarin.Mac.

You can also do this from a `cake` script:

```csharp
#addin nuget:?package=Cake.Boots&version=1.0.2.421

Task("Boots")
    .Does(async () =>
    {
        if (!IsRunningOnWindows ()) {
            await Boots (Product.XamarinMac, ReleaseChannel.Stable);
            await Boots (Product.XamariniOS, ReleaseChannel.Preview);
        }
        await Boots (Product.XamarinAndroid, ReleaseChannel.Preview);
    });
```
If you omit the second `ReleaseChannel` parameter, it will default to `ReleaseChannel.Stable`.

> NOTE! if you need to install Mono, do this in a separate process from the rest of your Cake build.

For example:

```csharp
Task("Mono")
    .Does(async () =>
    {
        await Boots (Product.Mono);
    });
```

Then invoke Cake twice:

```bash
./build.sh -t Mono
./build.sh -t Boots
```

### System.CommandLine

`boots` now uses `System.CommandLine`, so we get rich help text for free:

```
> boots
At least one of --url, --stable, or --preview must be used

boots:
  boots 1.0.2.421 File issues at: https://github.com/jonathanpeppers/boots/issues

Usage:
  boots [options]

Options:
  --url <url>            A URL to a pkg or vsix file to install
  --stable <stable>      Install the latest *stable* version of a product from VS manifests. Options include: Xamarin.Android, Xamarin.iOS, Xamarin.Mac, and Mono.
  --preview <preview>    Install the latest *preview* version of a product from VS manifests. Options include: Xamarin.Android, Xamarin.iOS, Xamarin.Mac, and Mono.
  --version              Display version information
```

*NOTE: using a URL is still going to be the most stable & reproducible option. It is possible a Xamarin or Mono update could come along and break your build.* 👀

### App Center

`samples/HelloForms.sln` is a "Hello World" Xamarin.Forms project configured with `boots` installing newer versions than what is available on [App Center][appcenter]:

![AppCenter](docs/AppCenter.png)

See [`appcenter-pre-build.sh`](samples/HelloForms.Android/appcenter-pre-build.sh) in this repo for an example of setting up `boots`. See the [App Center docs](https://aka.ms/docs/build/custom/scripts) for further detail about custom build scripts.

### GitHub Actions

[Github Actions][actions] is currently in beta, but I was able to get `boots` to work on both Windows & macOS.

See [`actions.yml`](.github/workflows/actions.yml) for an example.

### Cake

You can use `boots` from a [Cake](https://cakebuild.net/) script, which is helpful if you need other logic to decide what needs to be installed.

```csharp
#addin nuget:?package=Cake.Boots

Task("Boots")
    .Does(async () =>
    {
        var platform = IsRunningOnWindows() ? "windows" : "macos";
        await Boots ($"https://aka.ms/xamarin-android-commercial-d16-4-{platform}");
    });
```

### Other CI Systems

`boots` has been tested, and appears to work fine on:

* [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)
* [App Center][appcenter]
* [AppVeyor](https://www.appveyor.com/)
* [Bitrise](https://www.bitrise.io/)
* [Github Actions][actions]
* [Travis CI](https://travis-ci.org/)

Any build environment that can be configured to run .NET Core 2.1, can run `boots`. If you have success on other CI systems, let us know!

[appcenter]: https://appcenter.ms
[actions]: https://github.com/features/actions
