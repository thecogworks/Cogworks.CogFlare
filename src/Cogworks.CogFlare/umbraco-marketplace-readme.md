# CogFlare

A package that helps automatically purge CloudFlare cache with Umbraco 12-13

![Built With](https://img.shields.io/badge/Built%20With-.NET%207.0-blue)
![Built With](https://img.shields.io/badge/Built%20With-Angular-DD0031?logo=angular&logoColor=white)
![Cloudflare](https://img.shields.io/badge/Cache%20Provider-Cloudflare-F38020?logo=cloudflare&logoColor=white)
![Open Source](https://img.shields.io/badge/Open%20Source-❤-brightgreen)
![License](https://img.shields.io/github/license/thecogworks/Cogworks.CogFlare)
![GitHub Issues](https://img.shields.io/github/issues/thecogworks/Cogworks.CogFlare)
![Status](https://img.shields.io/badge/Status-Stable-success)
![Contributions](https://img.shields.io/badge/Contributions-Welcome-brightgreen?logo=github)

## Why is CogFlare Needed?

Most CMS platforms, including Umbraco, are designed to be **dynamic** and, by default, don’t cache full HTML pages. While they handle caching for assets like JavaScript, CSS, and images, HTML is often left uncached because dynamic pages frequently change. This ensures fresh content but comes with a cost: **every request hits the server**, even for unchanged pages, leading to unnecessary server load and slower response times.

**CogFlare** solves this problem by integrating Umbraco with **Cloudflare**, allowing you to leverage full-page HTML caching through their CDN. While Cloudflare can efficiently cache and serve HTML, the challenge lies in knowing when to purge cached content. Without proper purging, outdated or incorrect content may be served to users.

This is where CogFlare steps in. The package automatically monitors changes in your CMS:
- **When nodes or content are updated**, CogFlare identifies the affected pages and sends targeted purge requests to Cloudflare.
- Instead of purging the entire site, only the relevant pages are cleared, ensuring efficient caching and fresh content delivery.

By automating the caching and purging process, CogFlare provides the performance benefits of full-page caching without the complexities of managing it manually.

## Usage

### Basic Functionality

- Automatically purges Cloudflare cache when:
  - Content nodes are **published, unpublished, or deleted**.
  - Media items are **saved**.
- Ability to toggle the package functionality on/off in the settings.
- Ability to toggle logging on/off in the settings.

### Advanced Functionality

- Configure **Key Nodes** in the settings:
  - A **Key Node** is any content node that triggers a **FULL site cache purge** when it or its referenced nodes are changed (e.g., Site Settings, Navigation, Footers).
- Blocklist blocks that you don’t want to cache by specifying their aliases, with the **ability to automatically make form pages uncachable**.

### Backoffice Dashboard

- A dashboard has been added to the **Settings** section of the backoffice.
  - Currently, only **Admins** can access it.
- Features include:
  - A button to manually trigger a **FULL site cache purge**.
  - Viewing the current configuration for the package.

## Logs

- Logs are created whenever:
  - A node eligible for caching is changed.
  - A purge request to Cloudflare is made.

- Additional logs display the result of the purge request to Cloudflare:

# Installation

Install through dotnet CLI:
```c#
dotnet add package Cogworks.CogFlare
```

Or the NuGet Package Manager:
```c#
Install-Package Cogworks.CogFlare
```

Add these settings to the **appsettings.json**
```js
  "CogFlareSettings": {
    "IsEnabled": true,
    "ApiKey": "xxx",
    "Email": "xxx@xxx.com",
    "Endpoint": "https://api.cloudflare.com/client/v4/zones/[zoneId]/purge_cache",
    "Domain": "https://www.example.com",
    "EnableLogging": true, //optional
    "KeyNodes": "1234, 031089", // optional
    "BlockAliases": "formBlock, otherFormBlock", // optional
    "CacheTime": "2592000" // optional => will default to 1 month
  }
```

To add cache headers to your pages please add the view component in your Master or required page View
```razor
@await Component.InvokeAsync(ApplicationConstants.CogFlareCacheHeaders)
```

Ensure you include the correct using directive at the top of your file:
```razor
@using Cogworks.CogFlare.Core.Constants
```

By default the cache time will be set to 1 month. This can be overriden in the CogFlare Settings

## Umbraco Forms and Anti-Forgery Tokens with Full Page HTML Caching

Umbraco Forms include **anti-forgery tokens** by default. These tokens must be unique for each page to ensure forms function correctly. 

However, when implementing **full-page HTML caching**, this can cause issues: cached pages will reuse the same anti-forgery token, breaking the forms on those pages.

This package provides a solution to ensure both caching and forms can coexist seamlessly.

### Problem with Caching and Forms

If you use **full-page HTML caching** for a page containing an Umbraco form:
- The **anti-forgery token** will be cached along with the page's HTML.
- When users access the page, the token will no longer be unique, causing form submissions to fail.

### Workarounds

To resolve this issue, the package offers two options:

### **Option 1: Disable Anti-Forgery Tokens**
You can **disable anti-forgery tokens** for the affected pages:
1. This allows the page to remain cached while keeping the form functional.
2. **Caution**: Disabling anti-forgery tokens may reduce the security of form submissions.

### **Option 2: Use Blocklist Aliases to Disable Caching for Specific Pages**
This package includes a feature to **conditionally disable caching** for pages containing specific blocks, such as forms, to avoid the anti-forgery token issue.

1. **How It Works**:
   - In the `CogFlareSettings` in the appsettings, you can provide a **blocklist alias** (e.g., a block alias for the Umbraco form or any other block you don’t want to cache).
   - When rendering the page, the package checks for the presence of any of the specified block aliases.
   - If the page includes a block with one of these aliases, `private, no-cache, must-revalidate` will be set for the page, effectively disabling caching for that page.

2. **Configuration**:
   Add your `BlockAliases` to the `CogFlareSettings`:
```js
  "CogFlareSettings": {
    ...
    ...
    "BlockAliases": "formBlock, otherFormBlock"
  }
```


## Backoffice User:

```sh
Email: admin@admin.com
Password: 0123456789
```

## License

Licensed under the [MIT License](LICENSE.md)

&copy; 2024 [Cogworks](https://www.wearecogworks.com/)