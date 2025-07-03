# Acmebot.Provider.Infoblox

Infoblox provider for Key Vault Acmebot. Automates ACME DNS record management in Infoblox via its REST API (WAPI).

## Configuration

1. Create a `local.settings.json` file (or set environment variables in Azure):

```
Infoblox__BaseUrl=https://your-infoblox/wapi/v2.10
Infoblox__Username=username
Infoblox__Password=password
```

2. Main endpoints exposed by the Function App:

- **GET /api/zones**  
  Returns the list of DNS zones.
- **PUT /api/zones/{zone}/records/{name}**  
  Adds TXT records (expects `{ "type": "TXT", "ttl": 300, "values": ["xxxx"] }` as body).
- **DELETE /api/zones/{zone}/records/{name}**  
  Removes TXT records (expects `{ "type": "TXT", "ttl": 300, "values": ["xxxx"] }` as body).

3. Build and run locally:

```sh
dotnet build
func start
```

## Deployment

Publish as a .NET Isolated Azure Function App.  
`Program.cs` already includes DI and configuration injection.

---

## Authentication: Using Function Key (`x-functions-key`)

This Function App uses **Function Key authentication** (`AuthorizationLevel.Function`).  
This means that **all requests** must include a valid Function Key to access the endpoints.

You can pass the Function Key in two ways:

- **As a URL parameter**  
  ```
  https://<your-app>.azurewebsites.net/api/zones?code=<function-key>
  ```

- **In the HTTP header**  
  ```
  x-functions-key: <function-key>
  ```

> You can view and manage your Function Keys in the Azure Portal:  
> Function App > Functions > [Your Function] > "Function keys"

---

## Example: Add a TXT Record

Suppose you want to add a TXT record for `sub.domain.com` with value `myvalue` in the `domain.com` zone:

```sh
curl -X PUT "https://<your-app>.azurewebsites.net/api/zones/domain.com/records/sub.domain.com?code=<function-key>" \
  -H "Content-Type: application/json" \
  -d '{ "type": "TXT", "ttl": 300, "values": ["myvalue"] }'
```

You can also send the key in the header:

```sh
curl -X PUT "https://<your-app>.azurewebsites.net/api/zones/domain.com/records/sub.domain.com" \
  -H "Content-Type: application/json" \
  -H "x-functions-key: <function-key>" \
  -d '{ "type": "TXT", "ttl": 300, "values": ["myvalue"] }'
```

---

## Testing and Diagnostics

- You can test endpoints with `curl`, Postman, or directly from Key Vault Acmebot.
- Check Azure Portal logs for activity and errors.

---

## Recommended Project Structure

```
src/
  Acmebot.Provider.Infoblox/
    Acmebot.Provider.Infoblox.csproj
    Program.cs
    host.json
    local.settings.json
    FunctionApi.cs
    Infoblox/
      InfobloxClient.cs
      InfobloxService.cs
    ...
```

---

Questions?  
Open an issue in this repository.