#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots

# Workaround instead of restarting shell
# see: https://github.com/dotnet/cli/issues/9114#issuecomment-494226139
export PATH="$PATH:~/.dotnet/tools"
export DOTNET_ROOT="$(dirname "$(readlink "$(command -v dotnet)")")"

https://download.mono-project.com/archive/6.4.0/macos-10-universal/MonoFramework-MDK-6.4.0.189.macos10.xamarin.universal.pkg
boots https://aka.ms/xamarin-android-commercial-d16-3-macos
