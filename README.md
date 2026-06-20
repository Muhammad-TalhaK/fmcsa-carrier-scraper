# FMCSA Carrier Data Scraper

> A C# console tool that queries the public FMCSA SAFER registry to identify active US trucking carriers and export their contact details to Excel — built for lead generation and market research in the freight industry.

![C#](https://img.shields.io/badge/C%23-512BD4?style=flat&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)
![Excel](https://img.shields.io/badge/Excel-217346?style=flat&logo=microsoftexcel&logoColor=white)

## Overview

Given a starting MC number, the tool walks the public FMCSA SAFER carrier-snapshot endpoint, parses each carrier record, filters for **active carriers that have a phone number**, and writes the qualified results to an Excel sheet. It runs continuously until stopped, with sensible request pacing.

## Why it's interesting (the engineering)

- **Resilient HTTP client** — realistic browser headers and optional proxy/VPN support to handle real-world responses.
- **HTML parsing & extraction** — pulls raw carrier pages into structured records (legal name, USDOT status, entity type, phone, …).
- **Responsible pacing** — built-in rate limiting plus graceful handling and back-off on `403` / blocking responses.
- **Async pipeline** — a live, interruptible run loop with incremental Excel export so progress is never lost.

## Features

- Sequential MC-number scanning from a user-supplied starting point
- Field extraction into structured records
- Filtering for active carriers with valid contact info
- Incremental write-out to Excel as results are found
- Throttled requests with back-off on blocking responses
- Clean, interruptible stop command

## Tech

`C#` · `.NET` · `HttpClient` · `async/await` · Excel export

## Demo

> _Add a short terminal-recording GIF showing it streaming qualified carriers, plus a sample (anonymized) Excel screenshot._

## Running it (overview)

Proxy/VPN settings and the output path are supplied via configuration/environment (**not committed**). Run the tool, enter a starting MC number, and it streams qualified carriers into an Excel file until you stop it.

---

> **About this repository.** Proxy/VPN credentials and any environment-specific configuration are externalized and not included. The scraping, parsing, and export logic are intact.

> ⚖️ **Responsible use.** This reads **publicly available** records from the FMCSA SAFER system, with request pacing to avoid load on the service. Review and respect the source site's terms of use and applicable data and communication regulations (e.g., do-not-call rules) before contacting anyone.
