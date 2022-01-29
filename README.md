# Tadah.Arbiter
Fork of [PolygonGSArbiter](https://github.com/ProjectPolygon/PolygonGSArbiter) with the following changes:
- Messages *must* be signed
- Uses Tadah website routes
- 2009, 2013, and 2016 instead of 2010, 2011, and 2012
- Uses named pipes for script execution
- Proper TCP server
- RCCService support
- Messages must end with `<EOF>`

## Usage

Keys may be generated like so. Passphrase protected keys are currently not supported.
```shell
$ openssl genrsa -out private.pem 2048
$ openssl rsa -in private.pem -outform PEM -pubout -out public.pem
```

Messages made to the Arbiter must have the format `%signature%message`.

Example PHP implementation:

```php
$key = file_get_contents('./private.pem');
$message = json_encode($message); // containing keys 'Operation', 'JobId', ...

openssl_sign($message, $signature, $key, OPENSSL_ALGO_SHA256);
$signature = '%' . base64_encode($signature) . '%';
$message = $signature . $message . '<EOF>';

$socket = stream_socket_client('tcp://127.0.0.1:64989');
if ($socket)
{
    $sent = stream_socket_send($socket, $message);
    if ($sent > 0)
    {
        $response = fread($socket, 4096);
        var_dump(json_decode($response));
        stream_socket_shutdown($socket, STREAM_SHUT_RDWR)
    }
}
else
{
    // didn't connect
}
```

## License
Copyright (c) Tadah and Project Polygon 2022. Forked with permission. All rights reserved.
