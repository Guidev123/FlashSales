INSERT INTO users."Roles" ("Name") VALUES ('Customer') ON CONFLICT DO NOTHING;
INSERT INTO users."Roles" ("Name") VALUES ('Seller') ON CONFLICT DO NOTHING;

INSERT INTO users."Permissions" ("Code") VALUES ('users:read') ON CONFLICT DO NOTHING;
INSERT INTO users."Permissions" ("Code") VALUES ('users:write') ON CONFLICT DO NOTHING;
INSERT INTO users."Permissions" ("Code") VALUES ('catalog:read') ON CONFLICT DO NOTHING;

INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:read', 'Customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('catalog:read', 'Customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:read', 'Seller') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('users:write', 'Seller') ON CONFLICT DO NOTHING;
INSERT INTO users."RolePermissions" ("PermissionCode", "RoleName") VALUES ('catalog:read', 'Seller') ON CONFLICT DO NOTHING;

INSERT INTO users."RegistrationTypeRoles" ("Type", "RoleName") VALUES ('Customer', 'Customer') ON CONFLICT DO NOTHING;
INSERT INTO users."RegistrationTypeRoles" ("Type", "RoleName") VALUES ('Seller', 'Seller') ON CONFLICT DO NOTHING;
