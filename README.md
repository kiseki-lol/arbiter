# Kiseki.Arbiter

Manages Kiseki game servers

## TCP envelope format

```mermaid
graph TD

message_start["Message start\n(SOH, 1 byte)"]
message_size["Message size\n(uint16_t, 2 bytes)"]
signature_start["Signature start\n(SOH, 1 byte)"]
signature_size["Signature size\n(uint16_t, 2 bytes)"]
signature_data["Signature data\n (byte[signature_size])"]
signal_start["Signal start\n(SOH, 1 byte)"]
signal_size["Signal size\n(uint16_t, 2 bytes)"]
signal_data["Signal data\n (byte[signal_size]`)"]

subgraph envelope["Envelope format"]
direction LR
  message_start --> message_size --> message
end

subgraph message["Message format"]
direction LR
  signature_start --> signature_size --> signature_data --> signal_start
  signal_start --> signal_size --> signal_data
end
```

## License

Copyright (c) Kiseki 2023. All rights reserved. Not for public use.

Kiseki.Arbiter is adapted from [@ProjectPolygon/PolygonGSArbiter-foss](https://github.com/ProjectPolygon/PolygonGSArbiter-foss), a project licensed under the [MIT license](https://github.com/ProjectPolygon/PolygonGSArbiter-foss/blob/main/LICENSE).
