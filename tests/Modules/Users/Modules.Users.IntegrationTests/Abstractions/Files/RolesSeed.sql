INSERT INTO users."Roles" ("Name") VALUES ('customer') ON CONFLICT DO NOTHING;
INSERT INTO users."Roles" ("Name") VALUES ('seller') ON CONFLICT DO NOTHING;

INSERT INTO users."Permissions" ("Code") VALUES ('users:read') ON CONFLICT DO NOTHING;
INSERT INTO users."Permissions" ("Code") VALUES ('users:write') ON CONFLICT DO NOTHING;
INSERT INTO users."Permissions" ("Code") VALUES ('catalog:read') ON CONFLICT DO NOTHING;

INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:read', 'customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('catalog:read', 'customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:read', 'seller') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:write', 'seller') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('catalog:read', 'seller') ON CONFLICT DO NOTHING;

INSERT INTO users."RegistrationTypeRoles" ("Type", "RoleName") VALUES ('Customer', 'customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RegistrationTypeRoles" ("Type", "RoleName") VALUES ('Seller', 'seller') ON CONFLICT DO NOTHING;
