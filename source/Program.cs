using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Uwp.Notifications;
using QbitNotifier;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WindowsFirewallHelper;

Settings _settings;

InitializeMinimalApi();

static string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    var address = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

    if(address == null)
        throw new Exception("No network adapters with an IPv4 address in the system!");

    return address.ToString();
}

void AddFirewallRule()
{
    var processName = Process.GetCurrentProcess().MainModule.FileName;
    var existingRule = FirewallManager.Instance.Rules.FirstOrDefault(r => r.ApplicationName == processName);

    if (existingRule != null)
    {
        existingRule.IsEnable = true;
        return;
    }

    var rule = FirewallManager.Instance.CreateApplicationRule(
        @"qBittorrent Notifier",
        FirewallAction.Allow,
        processName
    );

    rule.Direction = FirewallDirection.Inbound;
    FirewallManager.Instance.Rules.Add(rule);
}

void DisplayNotification(string title, string description, string url, string path)
{
    new ToastContentBuilder()
        .AddButton("Open WebUI", ToastActivationType.Protocol, url)
        .AddButton("Browse Path", ToastActivationType.Protocol, path)
        .AddText(title)
        .AddText(description)
        .Show();
}

void InitializeMinimalApi()
{
    InitializeSettings();
    AddFirewallRule();

    var builder = WebApplication.CreateBuilder();
    var app = builder
        .Build();

    app.MapGet("/", DisplayNotification);
    app.Run(_settings.Address);
}

void InitializeSettings()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appconfig.json", optional: true, reloadOnChange: true)
        .Build();

    _settings = configuration.Get<Settings>();

    if (string.IsNullOrEmpty(_settings.Url))
        _settings.Url = GetLocalIPAddress();
}
