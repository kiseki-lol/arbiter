# Tadah.Arbiter
Manages Tadah game servers

Fork of [ProjectPolygon/PolygonGSArbiter](https://github.com/ProjectPolygon/PolygonGSArbiter)

## CI/CD
Tadah.Arbiter is available on the Tadah CI. All builds are compiled as release, and a build occurs per push.

- [win-x86](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_WinX86)
- [win-x64](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_WinX64)
- [linux-x64](https://ci.tadah.sipr/buildConfiguration/Tadah_Arbiter_LinuxX64)

## Notes
- Messages *must* be signed
- Uses Tadah website routes
- [2011](https://git.tadah.sipr/tadah/taipei) and [2016](https://git.tadah.sipr/tadah/tampa) client support
- Script execution
- Proper TCP server
- RCCService support
- Messages must end with `<<<EOF>>>`
- Better logging system
- .NET 6
- Linux support (w/ Wine)

## Usage
Keys may be generated like so. Passphrase protected keys are currently not supported.
```shell
$ openssl genrsa -out private.pem 2048
$ openssl rsa -in private.pem -outform PEM -pubout -out public.pem
```

Messages made to the Arbiter must have the format `%signature%timestamp:message<EOF>`. Only the `timestamp:message` part has to be signed. `timestamp` must represent the current unix timestamp. Requests older than five seconds are discarded.

Example PHP implementation:

```php
$key = file_get_contents('./private.pem');
$message = time() . ':' . json_encode($message); // containing keys 'Operation', 'JobId', ...

openssl_sign($message, $signature, $key, OPENSSL_ALGO_SHA256);
$signature = '%' . base64_encode($signature) . '%';
$message = $signature . $message . '<<<EOF>>>';

$socket = stream_socket_client('tcp://127.0.0.1:64989');
if ($socket)
{
    $sent = stream_socket_sendto($socket, $message);
    if ($sent > 0)
    {
        $response = fread($socket, 4096);
        var_dump(json_decode($response));
        stream_socket_shutdown($socket, STREAM_SHUT_RDWR);
    }
}
else
{
    // didn't connect
}
```

## Operations
- `OpenJob (string JobId, int PlaceId, int Version)`
- `CloseJob (string JobId)`
- `ExecuteScript (string JobId, string Script)`
- `RenewTampaServerJobLease (string JobId, int ExpirationInSeconds)`
- `CloseAllJobs (void)`
- `CloseAllTampaServerProcesses (void)`

## License
Copyright (c) Tadah and Project Polygon 2022. All rights reserved.

Forked with permission. Not for public use.
