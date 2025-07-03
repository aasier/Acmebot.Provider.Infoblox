# Acmebot.Provider.Infoblox

Infoblox provider for Key Vault Acmebot. Automates ACME DNS record management in Infoblox via its REST API (WAPI).

---

## Configuration

1. **Create a `local.settings.json` file (or set environment variables in Azure):**

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Infoblox__BaseUrl": "https://your-infoblox/wapi/v2.10",
    "Infoblox__Username": "your_user",
    "Infoblox__Password": "your_password"
  }
}
```

2. **Endpoints exposed by the Function App:**

- **GET /api/zones**  
  Returns the list of DNS zones.
- **PUT /api/zones/{zone}/records/{name}**  
  Adds a TXT record. Body example:  
  ```json
  { "type": "TXT", "ttl": 300, "values": ["xxxx"] }
  ```
- **DELETE /api/zones/{zone}/records/{name}**  
  Deletes a TXT record. Body example:  
  ```json
  { "type": "TXT", "ttl": 300, "values": ["xxxx"] }
  ```

3. **Build and run locally:**

```sh
cd src/Acmebot.Provider.Infoblox
dotnet restore
dotnet build
func start
```

---

## Authentication: Using the Function Key (`x-functions-key`)

Endpoints require a valid Function Key (`AuthorizationLevel.Function`).

You can pass the key:

- As a URL parameter:  
  ```
  http://localhost:7071/api/zones?code=local
  ```
- As a header:  
  ```
  x-functions-key: local
  ```

---

## Usage Examples

### 1. List DNS Zones

```sh
curl "http://localhost:7071/api/zones?code=local"
```

### 2. Add a TXT Record

Suppose you want to add a TXT record for `sub.domain.com` with value `myvalue` in the zone `domain.com`:

```sh
curl -X PUT "http://localhost:7071/api/zones/domain.com/records/sub.domain.com?code=local" \
  -H "Content-Type: application/json" \
  -d '{ "type": "TXT", "ttl": 300, "values": ["myvalue"] }'
```

### 3. Delete a TXT Record

Delete the `myvalue` TXT record for `sub.domain.com` in `domain.com`:

```sh
curl -X DELETE "http://localhost:7071/api/zones/domain.com/records/sub.domain.com?code=local" \
  -H "Content-Type: application/json" \
  -d '{ "type": "TXT", "ttl": 300, "values": ["myvalue"] }'
```

---

## How It Works (Infoblox Integration)

- **Add TXT:**  
  Calls Infoblox WAPI `POST /record:txt` with body:
  ```json
  {
    "name": "sub.domain.com",
    "text": "myvalue",
    "ttl": 300
  }
  ```
- **Delete TXT:**  
  - Looks up the TXT record for `name` and `text` via `GET /record:txt?name=sub.domain.com`
  - Finds the matching `_ref` with matching `"text": "myvalue"`
  - DELETEs `/record:txt/{_ref}`

---

## Project Structure

```
src/
  Acmebot.Provider.Infoblox/
    Acmebot.Provider.Infoblox.csproj
    Program.cs
    host.json
    local.settings.json
    FunctionApi.cs
    Models/
      RecordRequest.cs
    Infoblox/
      InfobloxClient.cs
      InfobloxService.cs
```

---

## Troubleshooting

- **Missing `Task<>`, `HttpClient`, etc. errors:**  
  Make sure you have the correct `using` statements in each `.cs` file:
  ```csharp
  using System.Threading.Tasks;
  using System.Net.Http;
  using System.Collections.Generic;
  ```
- **401 Unauthorized:**  
  Ensure you are passing the correct function key (`code=local` or from Azure Portal).
- **Infoblox API errors:**  
  - Double-check WAPI endpoint, credentials, and that your user has permissions to create/delete TXT records.
  - Check logs in Azure or local Functions host for detailed error messages.

---

## Testing Locally

1. **Start the function app:**
   ```sh
   func start
   ```
2. **Run the example cURL commands above.**
3. **Check the terminal for logs or errors.**
4. **Verify changes in your Infoblox grid (UI or API).**

---

## Questions?

Open an issue in this repository.
