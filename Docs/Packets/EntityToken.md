| Data Type | Description               |
|-----------|---------------------------|
| number    |                           |
| string    | Base64 Encoded something. |
| string    | JSON defined below        |

## JSON Data

| Key | Type   | Description        | Example                                                         |
|-----|--------|--------------------|-----------------------------------------------------------------|
| i   | string | Issued at?         | 2023-01-01T00:00:00                                             |
| idp | string | Identity Provider? | Custom                                                          |
| e   | string | Expiration?        | 2023-01-01T00:00:00                                             |
| fi  | string | First Issued?      | 2023-01-01T00:00:00                                             |
| tid | string | Tenant ID?         | A-Z a-z 0-9 -> 11x                                              |
| idi | string | Identity ID?       | game_{GUID}_cfg                                                 |
| h   | string | Hash?              | 16x A-F 0-9                                                     |
| ec  | string | Entity Chain?      | title_player_account!C{0-9*15}/{0-9*5}/{A-F0-9*16}/{A-F0-9*16}/ |
| ei  | string | Entity ID?         | A-F 0-9 -> 16x                                                  |
| et  | string | Entity Type?       | title_player_account                                            |