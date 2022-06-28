# Tadah.Arbiter
Manages Tadah game servers

## Notes

This is really just a fork of [PolygonGSArbiter](https://github.com/ProjectPolygon/PolygonGSArbiter) with the following changes:
- Messages *must* be signed
- Uses Tadah website routes
- 2008 and 2016 client support
- LuaPipes
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
- `RenewRccServiceJobLease (string JobId, int ExpirationInSeconds)`
- `CloseAllJobs (void)`
- `CloseAllRccServiceProcesses (void)`

## License
Copyright (c) Tadah and Project Polygon 2022. All rights reserved.

Forked with permission. Not for public use.
