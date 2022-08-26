# Tadah.Arbiter
Manages Tadah game servers

Fork of [ProjectPolygon/PolygonGSArbiter](https://github.com/ProjectPolygon/PolygonGSArbiter).

## CI/CD
Tadah.Arbiter is available on the Tadah CI. All builds are compiled as release, and a build occurs per push.

- [win-x86](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_WinX86)
- [win-x64](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_WinX64)
- [linux-x64](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_LinuxX64)

## Operations
- `OpenJob (string jobId, int placeId, ClientVersion version)`
- `CloseJob (string jobId)`
- `ExecuteScript (string jobId, string script)`
- `RenewTampaJobLease (string jobId, int expirationInSeconds)`
- `CloseAllJobs (int)`
- `CloseAllTampaProcesses (int)`
- `Thumbnail (AssetType type, int assetId, string? accessKey)`

## License
Copyright (c) Tadah and Project Polygon 2022. All rights reserved.

Forked with permission. Not for public use.
