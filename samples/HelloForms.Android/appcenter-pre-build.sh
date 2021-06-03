#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

printenv

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots --version 1.0.2.421
boots --preview Mono
boots --preview Xamarin.Android
boots --preview Xamarin.iOS

# set $JI_JAVA_HOME to point to a JDK 11
echo '##vso[task.setvariable variable=JI_JAVA_HOME]$(JAVA_HOME_11_X64)'
