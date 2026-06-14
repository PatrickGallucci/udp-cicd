# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.7.0] - 2026-06-14

### Added

- **Multi-platform resources.** A single `udp.yml` can now declare resources
  across three control planes, selected by a per-type `Platform` tag on the
  resource registry:
  - **Microsoft Entra** (Microsoft Graph, tenant scope): `entra_groups` and
    `entra_apps` (app registrations with optional service principal). Create is
    idempotent — an existing object with the same display name is patched.
  - **Azure** (ARM via Bicep through the `az` CLI): `azure_resource_groups`,
    `azure_storage_accounts`, and a generic `azure_deployments` escape hatch that
    deploys any author-supplied `.bicep` file at subscription or resource-group
    scope. A top-level `azure:` block provides subscription/location defaults.
- `IResourcePlatformProvider` abstraction with `EntraResourceProvider` and
  `AzureResourceProvider`; the deploy loop dispatches non-Fabric items to their
  provider while the Fabric path is unchanged.
- Platform-aware name validation (Fabric character rules no longer apply to
  Entra/Azure names; storage account names are checked for the 3–24 lowercase
  alphanumeric rule).
- New example [`examples/10-azure-and-entra`](examples/10-azure-and-entra) and a
  [Multi-platform guide](docs/guide/multi-platform.md).

### Changed

- `ResourceTypeInfo.FabricType` renamed to `ProviderType` (the platform-native
  type identifier); `FabricType` retained as a compatibility alias. The forward
  `ItemTypeMap` (field → provider type) now spans all platforms; the reverse map
  used by `generate` remains Fabric-scoped.

### Notes

- The deploy orchestration is still workspace-centric: a deployment containing
  only Entra/Azure resources will still create a Fabric workspace. Mixed and
  Fabric-only deployments are unaffected.
