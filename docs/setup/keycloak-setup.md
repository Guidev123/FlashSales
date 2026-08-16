# Keycloak — Realm `flash-sales-dev` Setup

> Full recreation guide based on the realm export. Keycloak version: **26.2.5**

---

## 1. Docker Compose

```yaml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:26.2.5
    command: start-dev
    environment:
      KC_BOOTSTRAP_ADMIN_USERNAME: admin
      KC_BOOTSTRAP_ADMIN_PASSWORD: admin
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://keycloak-db:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: keycloak
      KC_HTTP_PORT: 8080
    ports:
      - "8080:8080"
    depends_on:
      keycloak-db:
        condition: service_healthy

  keycloak-db:
    image: postgres:16
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: keycloak
    volumes:
      - keycloak-db-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U keycloak"]
      interval: 5s
      retries: 5

volumes:
  keycloak-db-data:
```

Go to `http://localhost:8080` and log in with `admin / admin`.

---

## 2. Create the Realm

1. Top-left menu → **Create Realm**
2. **Realm name:** `flash-sales-dev`
3. **Enabled:** ON
4. **Create**

---

## 3. Realm Settings

Go to **Realm Settings** and configure the following tabs:

### Login tab

| Setting | Value |
|---|---|
| User registration | OFF — registration is handled exclusively by the API |
| Login with email | ON |
| Duplicate emails | OFF |
| Remember me | ON |
| Verify email | OFF |
| Password reset | ON |
| Edit username | OFF |

### Sessions tab

| Setting | Value |
|---|---|
| SSO Session Idle | 30 minutes |
| SSO Session Max | 10 hours |
| Access Token Lifespan | 5 minutes |

> The short access token lifespan (5 min) is intentional — roles and permissions are read from the database on every request via `ClaimsTransformation`, so the token doesn't need a long lifetime.

---

## 4. Realm Roles

Go to **Realm roles → Create role** and create the three roles below.

### `activated`
- **Role name:** `activated`
- Assigned by the API after the user completes the activation flow (`POST /api/v1/users` or `POST /api/v1/users/customer/activate`). The API middleware checks this role in the JWT to grant access to protected routes.

### `customer`
- **Role name:** `customer`
- Assigned in the domain database after activation. **Not assigned in Keycloak** — it exists in Keycloak only for naming consistency and as a reference in the public client mappers.

### `seller`
- **Role name:** `seller`
- Assigned in the domain database after `POST /api/v1/users/seller/activate`. Like `customer`, **not managed by Keycloak** — it lives in the API database.

> **Note:** `customer` and `seller` exist in Keycloak solely for the `flash-sales-public` mappers. Role assignment to users is managed exclusively by the API via the database.

---

## 5. Client: `flash-sales-public` (Frontend)

Go to **Clients → Create client**.

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-public` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | OFF (public client) |
| Standard flow | ON |
| Direct access grants | ON |
| Implicit flow | OFF |
| Service accounts | OFF |

> `Direct access grants` enabled allows the Resource Owner Password flow — useful for local testing with tools like Postman or integration scripts. Disable in production if not needed.

### Login Settings
| Field | Value |
|---|---|
| Valid redirect URIs | `http://localhost:3000/*` |
| Valid post logout URIs | `http://localhost:3000/*` |
| Web origins | `http://localhost:3000` |

### Protocol Mappers

After creating the client, go to **Clients → flash-sales-public → Client scopes → flash-sales-public-dedicated → Add mapper → By configuration**.

Create the following mappers:

---

#### Mapper: `birth_date`
| Field | Value |
|---|---|
| Mapper type | User Attribute |
| Name | `birth_date` |
| User Attribute | `birth_date` |
| Token Claim Name | `birth_date` |
| Claim JSON Type | String |
| Add to ID token | ON |
| Add to access token | ON |
| Add to userinfo | ON |

> Exposes the `birth_date` attribute set by the API via Admin API after registration.

---

#### Mapper: `activated`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `activated` |
| Token Claim Name | `activated` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token | ON |
| Add to access token | ON |
| Add to userinfo | ON |

> The API middleware reads this claim to check whether the user completed the activation flow. If absent → 403 with `account_not_activated`.

---

#### Mapper: `customer`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `customer` |
| Token Claim Name | `customer` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token | ON |
| Add to access token | ON |
| Add to userinfo | ON |

---

#### Mapper: `seller`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `seller` |
| Token Claim Name | `seller` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token | ON |
| Add to access token | ON |
| Add to userinfo | ON |

---

#### Mapper: `audience`
| Field | Value |
|---|---|
| Mapper type | Audience |
| Name | `audience` |
| Included Client Audience | `flash-sales-api` |
| Add to access token | ON |

> Ensures the JWT issued to the frontend includes `flash-sales-api` as the audience. The .NET API validates this field when verifying the token.

---

## 6. Client: `flash-sales-api` (Backend / Service Account)

Go to **Clients → Create client**.

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-api` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### After creating: assign Service Account roles

Go to **Clients → flash-sales-api → Service account roles → Assign role → Filter by clients → realm-management** and assign:

- `manage-users` — to create users, set attributes, and assign roles via Admin API
- `view-users` — to look up existing users

### Credentials

Go to **Clients → flash-sales-api → Credentials** and copy the `Client secret`. Set it in the API environment variables:

```
Keycloak__ClientId=flash-sales-api
Keycloak__ClientSecret=<secret>
Keycloak__Authority=http://localhost:8080/realms/flash-sales-dev
```

---

## 7. Identity Providers

### GitHub

Go to **Identity Providers → Add provider → GitHub**.

| Field | Value |
|---|---|
| Client ID | `<your GitHub App client_id>` |
| Client Secret | `<your GitHub App client_secret>` |
| Trust Email | OFF — GitHub does not guarantee `email_verified` in all cases |
| Sync mode | LEGACY |
| First Login Flow | `first broker login` (default) |

> **Authorization callback URL** to be set in the GitHub App:
> ```
> http://localhost:8080/realms/flash-sales-dev/broker/github/endpoint
> ```

### Google

Go to **Identity Providers → Add provider → Google**.

| Field | Value |
|---|---|
| Client ID | `<your Google OAuth client_id>` |
| Client Secret | `<your Google OAuth client_secret>` |
| Trust Email | OFF |
| Sync mode | LEGACY |
| First Login Flow | `first broker login` (default) |

> **Authorized redirect URI** to be set in the Google Cloud Console:
> ```
> http://localhost:8080/realms/flash-sales-dev/broker/google/endpoint
> ```

> **Trust Email OFF on both IdPs:** the email coming from social login is not automatically treated as verified. Account linking via re-authentication protects against unintended account merging.

---

## 8. First Broker Login Flow (Account Linking)

The default `first broker login` flow is already correctly set up for account linking. Verify under **Authentication → first broker login** that the authenticators are configured as follows:

| Authenticator | Requirement |
|---|---|
| Review Profile | REQUIRED |
| Create User If Unique | ALTERNATIVE |
| Handle Existing Account | ALTERNATIVE |
| ↳ Confirm Link Existing Account | REQUIRED (sub-flow) |
| ↳ Verify Existing Account By Email | ALTERNATIVE |
| ↳ Verify Existing Account By Re-authentication | ALTERNATIVE |

> This flow is triggered on the **first time** a user authenticates via a social provider. If the email already exists in the realm, Keycloak requires re-authentication with the existing account's password before linking the two identities. After linking, subsequent social logins work directly without any prompt.

---

## 9. Theme

The realm uses a custom login theme called `flash-sales` (`loginTheme: "flash-sales"`). To recreate it:

1. Create the `themes/flash-sales` folder inside the Keycloak directory (or mount it as a Docker volume)
2. Follow the standard Keycloak theme structure: `login/`, `account/`, `email/`
3. Go to **Realm Settings → Themes → Login theme** and select `flash-sales`

If the theme is not available, leave the field empty — the default Keycloak theme will be used.

---

## 10. Summary

| Component | Value |
|---|---|
| Realm | `flash-sales-dev` |
| User registration | OFF — API creates users via Admin API |
| Duplicate emails | OFF — email uniqueness enforced by Keycloak |
| Public client | `flash-sales-public` — used by the SPA frontend |
| Confidential client | `flash-sales-api` — used by the .NET API via Client Credentials |
| Identity Providers | GitHub + Google with First Broker Login flow |
| Role `activated` | Signals activation — verified by the API middleware in the JWT |
| Roles `customer` / `seller` | Exist in Keycloak for mappers — managed in the API database |
| Mapper `birth_date` | Custom attribute set by the API after registration |
| Mapper `audience` | Includes `flash-sales-api` as audience in the JWT |
| Account linking | Enabled via First Broker Login with re-authentication |

