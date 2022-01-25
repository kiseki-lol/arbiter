# Tadah.Arbiter
Fork of PolygonGSArbiter with the following changes:
- Messages *must* be signed
- Uses Tadah website routes
- 2009, 2013, and 2016 instead of 2010, 2011, and 2012
- Uses named pipes for script execution

## TODO
- Fix TCP connections
- RCCService support for 2016. SOAP, etc.

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
$message = json_encode($message); // containing keys 'Operation', 'JobID', ...

openssl_sign($message, $signature, $key, OPENSSL_ALGO_SHA256);
$signature = '%' . base64_encode($signature) . '%';
$message = $signature . $message;

$socket = fsockopen('127.0.0.1', 64989);
fwrite($socket, $message);
fclose($socket);
```

## License
Copyright (c) Tadah and Polygon 2022. All rights reserved.