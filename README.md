`GoogleDomains.Ddns.Client`
===========================

**Deprecated**: I don't use this any more. I no longer need DDNS and Google Domains is down the toilet anyway.

This is a Windows client for Google Domains's Dynamic DNS feature. I wrote it for my own use. It runs as a Windows service.

Installation
------------

1. Download [the release](https://github.com/benjamin-hodgson/GoogleDomains.Ddns.Client/releases)
2. Unzip the release somewhere
3. Edit `appsettings.json`: update the `Domain`, `Username`, and `Password` fields to match the configuration in Google Domains.
    * If you want detailed logging, uncomment the `EventLog` section
4. `sc create "Google Domains DDNS Client" binpath=path\to\GoogleDomains.Ddns.Client.exe` 
5. Start the service.
