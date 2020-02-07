#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

dotnet tool install --global boots --version 1.0.2.421
boots --preview Mono
boots --preview XamarinAndroid
boots --preview XamariniOS
