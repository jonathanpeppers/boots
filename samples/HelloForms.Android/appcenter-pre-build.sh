#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots
boots https://download.mono-project.com/archive/6.4.0/macos-10-universal/MonoFramework-MDK-6.4.0.189.macos10.xamarin.universal.pkg
boots https://aka.ms/xamarin-android-commercial-d16-3-macos
