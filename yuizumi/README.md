## Prerequisites

* [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1)

## Build

```
$ dotnet restore
$ dotnet msbuild
```

## Run

Verify.dll takes three arguments: your nbt, the source mdl, and the target mdl.
`-` is interpreted as an empty model.

```
$ dotnet verify/bin/Debug/netcoreapp2.0/Verify.dll FA001.nbt - FA001_tgt.mdl
$ dotnet verify/bin/Debug/netcoreapp2.0/Verify.dll FD001.nbt FD001_tgt.mdl -
$ dotnet verify/bin/Debug/netcoreapp2.0/Verify.dll FR001.nbt FR001_src.mdl FR001_tgt.mdl
```
