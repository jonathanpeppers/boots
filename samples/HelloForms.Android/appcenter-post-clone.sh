#!/usr/bin/env bash

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots --version 0.1.0.251-beta

# Workaround instead of restarting shell
export PATH="$PATH:~/.dotnet/tools"

boots https://download.mono-project.com/archive/6.0.0/macos-10-universal/MonoFramework-MDK-6.0.0.313.macos10.xamarin.universal.pkg
boots https://aka.ms/xamarin-android-commercial-d16-2-macos
