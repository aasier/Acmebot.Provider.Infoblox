# Acmebot.Provider.Infoblox

## Descripción

Este proyecto expone una API HTTP compatible con el "Custom DNS API spec" de Key Vault Acmebot, pero utiliza Infoblox como backend real.  
Traducirá operaciones estándar de la API ("zones", "upsert record", "delete record") a llamadas WAPI de Infoblox.  
De este modo, puedes usar la integración automática de Acmebot con Infoblox sin modificar Acmebot.

---

## Configuración

Crea o edita `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Infoblox__BaseUrl": "https://tu-infoblox/wapi/v2.10",
    "Infoblox__Username": "usuario",
    "Infoblox__Password": "contraseña"
  }
}
```
- `Infoblox__BaseUrl` es la URL base de la API WAPI de Infoblox (sin barra final).
- Usa credenciales con permisos de escritura en DNS.

---

## Endpoints y Spec soportados

### 1. Listar zonas

**GET /zones**

**Respuesta:**
```json
[
  {
    "id": "example_com",
    "name": "example.com",
    "nameServers": ["x.x.x.x", "y.y.y.y"] // opcional, si Infoblox lo soporta
  }
]
```

---

### 2. Crear/actualizar registros TXT

**PUT /zones/{zoneId}/records/{recordName}**

**Cuerpo:**
```json
{
  "type": "TXT",
  "ttl": 60,
  "values": ["xxxxxx", "yyyyyy"]
}
```

- Se borrarán todos los registros TXT previos de ese nombre antes de añadir los nuevos.

---

### 3. Eliminar registros TXT

**DELETE /zones/{zoneId}/records/{recordName}**

- Elimina todos los registros TXT asociados a ese nombre (no requiere body).

---

## Ejemplos de uso

### Listar zonas

```sh
curl -H "x-functions-key: local" http://localhost:7071/api/zones
```

### Añadir varios TXT

```sh
curl -X PUT "http://localhost:7071/api/zones/example_com/records/_acme-challenge.example.com" \
  -H "Content-Type: application/json" \
  -H "x-functions-key: local" \
  -d '{ "type": "TXT", "ttl": 60, "values": ["valor1", "valor2"] }'
```

### Eliminar todos los TXT de un nombre

```sh
curl -X DELETE "http://localhost:7071/api/zones/example_com/records/_acme-challenge.example.com" \
  -H "x-functions-key: local"
```

---

## Autenticación

- Usa el parámetro `code=local` en la URL o el header `x-functions-key: local`.

---

## Estructura del proyecto

```
src/
  Acmebot.Provider.Infoblox/
    Program.cs
    FunctionApi.cs
    Infoblox/
      InfobloxClient.cs
      InfobloxService.cs
    Models/
      ZoneResponse.cs
      RecordRequest.cs
    local.settings.json
```

---

## Pruebas locales

1. Inicia la función:
   ```sh
   func start
   ```

2. Haz peticiones como en los ejemplos de arriba.

3. Observa los logs para cualquier error o detalle.

---

## Notas técnicas

- El endpoint `/zones` responde con `{ id, name }` adaptando los datos de Infoblox; `id` es el FQDN reemplazando `.` por `_`.
- El endpoint PUT elimina primero todos los TXT previos para ese nombre y luego añade uno por cada valor.
- El endpoint DELETE borra todos los TXT para ese nombre.
- El backend usa la WAPI de Infoblox, autenticando por Basic Auth.

---

## FAQ y soporte

¿Problemas?  
- Verifica credenciales y permisos en Infoblox.
- Revisa los logs de Azure Functions para detalles de errores.

¿Dudas o contribuciones?  
Abre un issue o PR en este repositorio.
