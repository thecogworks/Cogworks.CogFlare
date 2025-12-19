<img src="cogflare.png" 
     alt="Cogworks.CogFlare" 
     style="float:right;height:150px;width:150px" />

<h3 style="font-size:2.7rem;color:#ff">CogFlare</h3>

[![NuGet release](https://img.shields.io/nuget/v/Cogworks.CogFlare.svg)](https://www.nuget.org/packages/Cogworks.CogFlare/)
![NuGet Downloads](https://img.shields.io/nuget/dt/Cogworks.CogFlare)

![Built With](https://img.shields.io/badge/Built%20With-.NET%207.0-blue)
![Built With](https://img.shields.io/badge/Built%20With-Angular-DD0031?logo=angular&logoColor=white)
![Cloudflare](https://img.shields.io/badge/Cache%20Provider-Cloudflare-F38020?logo=cloudflare&logoColor=white)

![Open Source](https://img.shields.io/badge/Open%20Source-❤-brightgreen)
![License](https://img.shields.io/github/license/thecogworks/Cogworks.CogFlare)
![GitHub Issues](https://img.shields.io/github/issues/thecogworks/Cogworks.CogFlare)
![Status](https://img.shields.io/badge/Status-Stable-success)
![Contributions](https://img.shields.io/badge/Contributions-Welcome-brightgreen?logo=github)


A package that helps automatically purge CloudFlare cache with Umbraco 12-13

<h2 style="color:plum">Why is CogFlare Needed?</h2>

Most CMS platforms, including Umbraco, are designed to be **dynamic** and, by default, don’t cache full HTML pages. While they handle caching for assets like JavaScript, CSS, and images, HTML is often left uncached because dynamic pages frequently change. This ensures fresh content but comes with a cost: **every request hits the server**, even for unchanged pages, leading to unnecessary server load and slower response times.

**CogFlare** solves this problem by integrating Umbraco with **Cloudflare**, allowing you to leverage full-page HTML caching through their CDN. While Cloudflare can efficiently cache and serve HTML, the challenge lies in knowing when to purge cached content. Without proper purging, outdated or incorrect content may be served to users.

This is where CogFlare steps in. The package automatically monitors changes in your CMS:
- **When nodes or content are updated**, CogFlare identifies the affected pages and sends targeted purge requests to Cloudflare.
- Instead of purging the entire site, only the relevant pages are cleared, ensuring efficient caching and fresh content delivery.

By automating the caching and purging process, CogFlare provides the performance benefits of full-page caching without the complexities of managing it manually.

<h2 style="color:plum">Usage</h2>

<h3 style="color:salmon">Basic Functionality</h2>

- Automatically purges Cloudflare cache when:
  - Content nodes are **published, unpublished, or deleted**.
  - Media items are **saved**.
- Ability to toggle the package functionality on/off in the settings.
- Ability to toggle logging on/off in the settings.

<h3 style="color:salmon">Advanced Functionality</h2>

- Configure **Key Nodes** in the settings:
  - A **Key Node** is any content node that triggers a **FULL site cache purge** when it or its referenced nodes are changed (e.g., Site Settings, Navigation, Footers).
- Blocklist blocks that you don’t want to cache by specifying their aliases, with the **ability to automatically make form pages uncachable**.

<h3 style="color:salmon">Backoffice Dashboard</h2>

- A dashboard has been added to the **Settings** section of the backoffice.
  - Currently, only **Admins** can access it.
- Features include:
  - A button to manually trigger a **FULL site cache purge**.
  - Viewing the current configuration for the package.

<h2 style="color:plum">Logs</h2>

- Logs are created whenever:
  - A node eligible for caching is changed.
  - A purge request to Cloudflare is made.

![CogFlare Logs](images/cogflare-logs-1.png)

- Additional logs display the result of the purge request to Cloudflare:

![CogFlare CloudFlare Logs](images/cogflare-logs-2.png)

<h2 style="color:plum">Installation</h2>

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
    "ApiToken": "xxx",
    "Email": "xxx@xxx.com",
    "Endpoint": "https://api.cloudflare.com/client/v4/zones/[zoneId]/purge_cache",
    "Domain": "https://www.example.com",
    "EnableLogging": true, //optional
    "KeyNodes": "1234, 031089", // optional
    "KeyParentNodes": "1001",  // optional
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

<h2 style="color:plum">Umbraco Forms and Anti-Forgery Tokens with Full Page HTML Caching</h2>

Umbraco Forms include **anti-forgery tokens** by default. These tokens must be unique for each page to ensure forms function correctly. 

However, when implementing **full-page HTML caching**, this can cause issues: cached pages will reuse the same anti-forgery token, breaking the forms on those pages.

This package provides a solution to ensure both caching and forms can coexist seamlessly.

<h3 style="color:salmon">Problem with Caching and Forms</h3>

If you use **full-page HTML caching** for a page containing an Umbraco form:
- The **anti-forgery token** will be cached along with the page's HTML.
- When users access the page, the token will no longer be unique, causing form submissions to fail.

<h3 style="color:salmon">Workarounds</h3>

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

<h2 style="color:plum">App Settings Explained</h2>

Brief explaination on some appsettings

<h2 style="color:salmon">AuthenticationMethod</h2>

The **AuthenticationMethod** determines the security headers which are sent to Cloudflare.  

When the value is set to "Bearer", the following header is sent:

Authorization: Bearer [cloudflare_api_token]

For backwards compatibility, when the value is set to "Email" (or anything other than "Bearer"), the following headers are sent:

X-Auth-Email: [cloudeflare_email]
<br/>
X-Auth-Key: [cloudflare_api_key]

<h2 style="color:salmon">ApiToken</h2>

When using the "Bearer" AuthenticationMethod you will need to set an API Token. This is a different value than the API Key and will require you to generate one in the CloudFalre dashboard.


<h3 style="color:salmon">CacheTime</h3>

Set the cache duration (in seconds) for the `Cache-Control` header.

If you set it to `0`, it will use `no-cache, no-store, must-revalidate`, which effectively disables browser caching.  

This is recommended if you want to ensure browsers always fetch the latest version of a page.


<h3 style="color:salmon">CacheTimeEdge</h3>

Set how long (in seconds) Cloudflare should cache the response at the edge (i.e. on their servers).  

The value you enter here will be used for the `Edge-Cache` header.  

This only affects Cloudflare's edge caching — it doesn’t control browser caching.  

Useful for reducing load on your origin server while keeping things fast for end users.

<h3 style="color:salmon">KeyNodes</h3>

A **Key Node** is any content node that triggers a **FULL site cache purge** when it or its referenced nodes are changed (e.g., Site Settings, Navigation, Footers).

<h3 style="color:salmon">KeyParentNodes</h3>

The Key Parent Nodes setting allows you to specify important parent page IDs within your site structure. These are typically pages that aggregate or display content from their child pages — such as a "News" or "Blog" section that pulls in the latest posts.

When any child page under one of these specified key parent nodes is updated, added, or removed, the system will not only purge the changed page itself, but also any other pages that reference the key parent. This ensures that content listings or summaries (e.g., "Latest News") remain fresh and up to date.

### **Why Use This?**

This feature is especially useful for pages that dynamically reference child content, such as:

- News landing pages
- Blog indexes
- Category archives
- Homepages with latest articles

By using key parent nodes, you ensure that all related cached content is correctly invalidated whenever underlying data changes — preventing stale content from sticking around.

### **Example**
Let’s say you have a page with ID 1242 called "News", and it lists recent news articles. If you set 1242 as a key parent node, and a new article is published under it, any page that references the "News" page (like the homepage) will also be purged from cache — keeping your site content consistent.

## Backoffice User:

```sh
Email: admin@admin.com
Password: 0123456789
```

<h2 style="color:plum">License</h2>

Licensed under the [MIT License](LICENSE.md)

&copy; 2025 [Cogworks](https://www.wearecogworks.com/)