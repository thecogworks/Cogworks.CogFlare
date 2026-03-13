# CogFlare configuration reference

This document describes every property on the `CogFlareSettings` configuration record, with types, defaults, examples and notes.

---

## `ApiKey`
- Type: string
- Required: Optional (required when using `Email`/`X-Auth-Key` authentication)
- Default: `""`
- Description: Cloudflare API key. Used for older Cloudflare authentication where `X-Auth-Email`/`X-Auth-Key` headers are required.
- When to use: Use when `AuthenticationMethod` is not `Bearer` and your Cloudflare account requires an API key.
- Example: `"0123456789abcdef0123456789abcdef01234"`
- Notes/Caveats: Prefer `ApiToken` (`Bearer`) where possible; API keys have broader privileges and are less fine-grained.

## `ApiToken`
- Type: string
- Required: Optional (required when using `Bearer` authentication)
- Default: `""`
- Description: Cloudflare API token used in Bearer authentication (Authorization: Bearer ...).
- When to use: Use when `AuthenticationMethod` is set to `Bearer` and you have created a token in Cloudflare with minimal required permissions for purging.
- Example: `"v1.abcdef..."`
- Notes/Caveats: Recommended over `ApiKey` for scoped access.

## `Email`
- Type: string
- Required: Optional (required when using `Email`/`X-Auth-Key` authentication)
- Default: `""`
- Description: Account email used with `ApiKey` when `AuthenticationMethod` is not `Bearer`.
- When to use: Only when you authenticate with `X-Auth-Email` and `X-Auth-Key` headers.
- Example: `"admin@example.com"`

## `Endpoint`
- Type: string
- Required: Optional but typically should be set
- Default: `""`
- Description: Full Cloudflare API endpoint to call for purging (normally includes the zone id and `/purge_cache`).
- When to use: Set to your zone purge endpoint.
- Example: `"https://api.cloudflare.com/client/v4/zones/0123456789abcdef/purge_cache"`
- Notes/Caveats: If empty, purging will not work; validate this value when configuring the package.

## `KeyNodes`
- Type: string (comma-separated IDs)
- Required: Optional
- Default: `""`
- Description: Comma-separated list of node IDs that are considered Key Nodes. Changes to these or their referenced content trigger a full-site purge.
- When to use: Use for site-wide configuration pages (site settings, navigation, footers) where changes must invalidate the entire site cache.
- Example: `"1234,5678"
- Notes/Caveats: IDs should be plain integers without spaces for reliable parsing.

## `KeyParentNodes`
- Type: string (comma-separated IDs)
- Required: Optional
- Default: `""`
- Description: Parent node IDs which act as important containers (e.g., blogs/news). When a child under a key parent changes, pages referencing that key parent will be purged.
- When to use: Use for listing/landing pages that aggregate child content where stale listings must be kept fresh.
- Example: `"1242,1300"`

## `Domain`
- Type: string
- Required: Optional but recommended
- Default: `""`
- Description: Public site domain used to construct full URLs for purge requests (e.g. `https://www.example.com`).
- When to use: Set when your site needs full absolute URLs for purging rather than relative paths.
- Example: `"https://www.example.com"`

## `IsEnabled`
- Type: bool
- Required: Optional
- Default: `false`
- Description: Master switch for enabling or disabling CogFlare behaviour.
- When to use: Disable to temporarily stop automated purges (useful during maintenance).
- Example: `true`

## `EnableLogging`
- Type: bool
- Required: Optional
- Default: `true`
- Description: Enables additional logging for purge events and internal decisions.
- When to use: Enable while debugging or during initial setup; can be disabled to reduce log verbosity.
- Example: `false`

## `BlockAliases`
- Type: string (comma-separated aliases)
- Required: Optional
- Default: `""`
- Description: Aliases of CMS blocks that should disable caching for the page when present (e.g., forms).
- When to use: Use to prevent caching pages containing forms or other dynamic blocks that must not be cached.
- Example: `"formBlock,contactFormBlock"`
- Notes/Caveats: Matching is by alias; ensure block aliases match exactly.

## `CacheTime`
- Type: string (seconds)
- Required: Optional
- Default: `""` (package defaults to 1 month if left empty)
- Description: Value (in seconds) for `Cache-Control` max-age for browser caching. If `0` the package will set `no-cache, no-store, must-revalidate`.
- When to use: Control browser caching independently of Cloudflare edge caching.
- Example: `"2592000"` (30 days)
- Notes/Caveats: The settings model defaults to empty string; runtime behaviour treats empty as 1 month.

## `CacheTimeEdge`
- Type: string (seconds)
- Required: Optional
- Default: `""`
- Description: How long Cloudflare should cache the response at the edge (Edge-Cache duration).
- When to use: Tune to reduce origin load while leaving browser caching shorter or different.
- Example: `"86400"` (1 day)

## `EnableBidirectionalRelations`
- Type: bool
- Required: Optional
- Default: `false`
- Description: When `true`, CogFlare resolves relations in both directions. On content change it purges:
  - the changed page
  - pages that reference the changed page (reverse relations)
  - pages that the changed page references (forward relations)
- When to use: Enable when templates or content pickers create bidirectional dependencies where a change on one node should invalidate cached content on nodes it references (for example, articles selected on author pages or related content blocks where both sides display aggregated summaries).
- Example: `true`
- Notes/Caveats:
  - Why it exists: Some sites use content pickers and aggregated displays where referenced content also summarises or lists the referencing content (e.g., author pages showing latest articles). The default reverse-only approach misses these forward dependencies and can leave stale content.
  - Old behaviour: With this setting `false` (default), CogFlare only purges the changed page and pages that reference it. Pages that the changed page references are not purged.
  - Behaviour when enabled: CogFlare additionally resolves and purges pages the changed page references, ensuring caches remain consistent when relationships are effectively bidirectional.
  - Performance considerations: Enabling bidirectional resolution may require extra relation lookups and may increase the number of URLs to purge, which slightly increases Cloudflare API usage and CPU/time for relation resolution. Enable only when necessary.

### Examples
- Article references Author
  - If Author changes:
    - Both modes purge `/author/jane-smith` and `/articles/how-to-write-better-code` (because articles reference the author).
  - If Article changes:
    - Default (false): purges `/articles/how-to-write-better-code` only.
    - Bidirectional (true): purges `/articles/how-to-write-better-code` and `/author/jane-smith` (because article references author and author page displays derived content).

- Related content block that shows summaries on both sides
  - If either side changes, enable bidirectional to ensure both pages are purged.

## `AuthenticationMethod`
- Type: string
- Required: Optional
- Default: `""` (behaviour depends on value)
- Description: Determines which header scheme to use for Cloudflare requests. Use `Bearer` to send `Authorization: Bearer [token]`. Any other value (commonly `Email`) will use `X-Auth-Email` and `X-Auth-Key` headers.
- When to use: Use `Bearer` when you have a scoped API token; use `Email`/`X-Auth-Key` when relying on an API key + email.
- Example: `"Bearer"`

## `UrlBatchSize`
- Type: int
- Required: Optional
- Default: `45`
- Description: Number of URLs to include in a single Cloudflare purge request. Many Cloudflare plans limit purge requests to ~50 URLs; CogFlare splits larger lists into batches.
- When to use: Reduce batch size if you encounter API limits; increase carefully to reduce number of requests.
- Example: `30`
- Notes/Caveats: Historically referred to as `PurgeBatchSize` in older docs; the configuration key is `UrlBatchSize` in the current model.

---

## Built-in behaviour (no configuration required)

### URL alias purging (`umbracoUrlAlias`)

Umbraco allows editors to define alternate URL aliases on any content node via the built-in `umbracoUrlAlias` property. For example, a page whose primary URL is `/about-us` might also be reachable at `/about`.

CogFlare automatically reads this property whenever a node is purged. If any aliases are present, each alias URL is added to the purge request alongside the primary URL. No configuration is needed; this is always active as long as the property exists on the node.

**Example**

A page has:
- Primary URL: `/about-us`
- `umbracoUrlAlias`: `about`

When that page is published or saved, CogFlare sends a purge request covering both `/about-us` and `/about`.

---

## Notes on defaults vs model
- The `CogFlareSettings` record initialises string properties to `string.Empty`; runtime behaviour may treat some empty values as meaningful defaults (for example, `CacheTime` falls back to 1 month if empty). Always verify runtime behaviour in your environment.

## Troubleshooting tips
- If purges are not happening, verify `Endpoint`, `AuthenticationMethod` and `ApiToken`/`ApiKey`/`Email` values.
- If too many URLs are purged, consider disabling `EnableBidirectionalRelations` or tuning `KeyNodes`/`KeyParentNodes` to limit broad purges.
- Use `EnableLogging` while debugging to inspect which URLs CogFlare resolves and purges.

---

If you want a shorter quick reference, copy the `CogFlareSettings` JSON from the top of this repository's `README.md` and adjust values as required.
