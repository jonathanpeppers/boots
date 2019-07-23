#!/usr/bin/env bash

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots --version 0.1.0.251-beta

# Workaround instead of restarting shell
# see: https://github.com/dotnet/cli/issues/9114#issuecomment-494226139
export PATH="$PATH:~/.dotnet/tools"
export DOTNET_ROOT="$(dirname "$(readlink "$(command -v dotnet)")")"

boots https://download.mono-project.com/archive/6.0.0/macos-10-universal/MonoFramework-MDK-6.0.0.313.macos10.xamarin.universal.pkg
boots https://aka.ms/xamarin-android-commercial-d16-2-macos
