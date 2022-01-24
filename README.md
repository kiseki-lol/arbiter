# Tadah.Arbiter
Fork of PolygonGSArbiter with the following changes:
- Messages *must* be signed
- Uses Tadah website routes
- 2009, 2013, and 2016 instead of 2010, 2011, and 2012

Keys may be generated like so. Passphrase protected keys are currently not supported.
```shell
$ openssl genrsa -out private.pem 2048
$ openssl rsa -in private.pem -outform PEM -pubout -out public.pem
```

## License
Copyright (c) Tadah and Polygon 2022. All rights reserved.