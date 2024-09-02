# Kiseki.Arbiter

Provisions Aya servers for a Kiseki instance

## TCP message format

```mermaid
graph TD

jwt_start["Message start\n(SOH, 1 byte)"]
jwt_size["JWT size\n(uint16_t, 2 bytes)"]
jwt_data["JWT data\n (byte[jwt_size])"]

subgraph message["Message format"]
direction LR
  jwt_start --> jwt_size --> jwt_data
end
```

## License

Kiseki.Arbiter is licensed under the [AGPLv3 license](https://github.com/kiseki-lol/arbiter/blob/trunk/LICENSE.md). A copy of it has been included with Kiseki.Arbiter.
