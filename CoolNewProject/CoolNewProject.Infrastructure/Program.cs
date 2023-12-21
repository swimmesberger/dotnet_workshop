using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System.Text.Json;
using Sql = Pulumi.AzureNative.Sql;
// use specific version otherwise WorkspaceResourceId is not available
// see: https://github.com/pulumi/pulumi-azure-native/issues/748
using Insights = Pulumi.AzureNative.Insights.V20200202;

using OperationalInsights = Pulumi.AzureNative.OperationalInsights;
// ReSharper disable UnusedVariable

return await Pulumi.Deployment.RunAsync(() => {
    // stack name (dev, int, prod, ...)
    var stackName = Pulumi.Deployment.Instance.StackName;

    // stack configuration
    var azureConfig = new Config("azure-native");

    var defaultLocation = azureConfig.Require("location");

    // Create an Azure Resource Group
    var resourceGroupName = $"swisslog-{stackName}";
    var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs() {
        ResourceGroupName = resourceGroupName,
        Location = defaultLocation
    });

    // SQL Server
    var sqlConfig = azureConfig.RequireSecretObject<JsonElement>("sql");
    var sqlServerPassword = sqlConfig.Apply(e => e.GetProperty("password").GetString()!);
    var sqlServerName = $"swisslog-sql-{stackName}";
    var sqlServer = new Sql.Server(sqlServerName, new Sql.ServerArgs {
        ResourceGroupName = resourceGroup.Name,
        Location = defaultLocation,
        AdministratorLogin = "swisslog",
        AdministratorLoginPassword = sqlServerPassword,
        ServerName = sqlServerName,
        Version = "12.0"
    });

    // SQL Database
    var sqlSkuConfig = sqlConfig.Apply(e => e.GetProperty("sku"));
    var sqlDatabaseName = $"swisslog-sql-database-{stackName}";
    var sqlDatabase = new Sql.Database(sqlDatabaseName, new Sql.DatabaseArgs {
        ResourceGroupName = resourceGroup.Name,
        Location = defaultLocation,
        ServerName = sqlServer.Name,
        DatabaseName = sqlDatabaseName,
        Sku = new Sql.Inputs.SkuArgs {
            Name = sqlSkuConfig.Apply(s => s.GetProperty("name").GetString()!),
            Tier = sqlSkuConfig.Apply(s => s.GetProperty("tier").GetString()!),
            Capacity = sqlSkuConfig.Apply(s => s.GetProperty("capacity").GetInt32())
        },
        RequestedBackupStorageRedundancy = sqlConfig.Apply(s => s.GetProperty("backup-redundancy").GetString()!),
        ZoneRedundant = sqlConfig.Apply(s => s.GetProperty("zone-redundant").GetBoolean())
    });

    // AppService Plan
    var webConfig = azureConfig.RequireObject<JsonElement>("web");
    Input<string> appServicePlanId;
    string? usingServicePlan = null;
    if (webConfig.TryGetProperty("service-plan", out var servicePlanProp)) {
        usingServicePlan = servicePlanProp.GetString();
    }
    if (usingServicePlan != null) {
        appServicePlanId = GetAppServicePlan.Invoke(new GetAppServicePlanInvokeArgs() {
            Name = usingServicePlan,
            ResourceGroupName = "swisslog-general"
        }).Apply(r => r.Id);
    } else {
        var webSkuConfig = webConfig.GetProperty("sku");
        var appServicePlanName = $"swisslog-web-plan-{stackName}";
        var appServicePlan = new AppServicePlan(appServicePlanName, new AppServicePlanArgs {
                Name = appServicePlanName,
                ResourceGroupName = resourceGroup.Name,
                Kind = "app,linux",
                Reserved = true, // makes this a linux server
                Sku = new SkuDescriptionArgs {
                        Name = webSkuConfig.GetProperty("name").GetString()!,
                        Tier = webSkuConfig.GetProperty("tier").GetString()!,
                        Capacity = webSkuConfig.GetProperty("capacity").GetInt32()
                }
        });
        appServicePlanId = appServicePlan.Id;
    }


    //-- Workspace AppInsights for WebApp
    var insightsConfig = azureConfig.RequireObject<JsonElement>("workspace");
    var insightsSkuConfig = insightsConfig.GetProperty("sku");
    var insightsWorkspaceServiceName = $"swisslog-insights-workspace-{stackName}";
    var insightsWorkspace = new OperationalInsights.Workspace(insightsWorkspaceServiceName, new() {
        WorkspaceName = insightsWorkspaceServiceName,
        ResourceGroupName = resourceGroup.Name,
        Location = defaultLocation,
        RetentionInDays = 30,
        Sku = new OperationalInsights.Inputs.WorkspaceSkuArgs {
            Name = insightsSkuConfig.GetProperty("name").GetString()!
        }
    });

    //-- Service AppInsights for WebApp
    var insightsServiceName = $"swisslog-insights-{stackName}";
    var insightsService = new Insights.Component(insightsServiceName, new() {
        ResourceName = insightsServiceName,
        ResourceGroupName = resourceGroup.Name,
        Location = defaultLocation,
        ApplicationType = Insights.ApplicationType.Web,
        Kind = "web",
        WorkspaceResourceId = insightsWorkspace.Id
    });

    // AppService WebApp
    var appServiceName = $"swisslog-web-{stackName}";
    var appService = new WebApp(appServiceName, new WebAppArgs {
        Name = appServiceName,
        ResourceGroupName = resourceGroup.Name,
        Location = defaultLocation,
        ServerFarmId = appServicePlanId,
        SiteConfig = new SiteConfigArgs {
            // Available options: az webapp list-runtimes --os-type linux
            // Check .pipeline/deploy-main.pipeline.yml!
            // LinuxFxVersion = "DOTNETCORE:7.0",
            AppSettings = {
                new NameValuePairArgs { Name = "ASPNETCORE_ENVIRONMENT", Value = "Azure" },

                // connection setting for the app-insights service
                new NameValuePairArgs { Name = "APPINSIGHTS_INSTRUMENTATIONKEY", Value = insightsService.InstrumentationKey },
                new NameValuePairArgs { Name = "APPLICATIONINSIGHTS_CONNECTION_STRING", Value = insightsService.ConnectionString },

                // auto-instrumentation; Main extension, which controls runtime monitoring; ~3 for Linux
                new NameValuePairArgs { Name = "ApplicationInsightsAgent_EXTENSION_VERSION", Value = "~3" },

                // enables app-service snapshot debugger (https://learn.microsoft.com/en-us/azure/azure-monitor/snapshot-debugger/snapshot-debugger)
                new NameValuePairArgs { Name = "APPINSIGHTS_SNAPSHOTFEATURE_VERSION", Value = "1.0.0" },

                // app-service profiling
                new NameValuePairArgs { Name = "APPINSIGHTS_PROFILERFEATURE_VERSION", Value = "1.0.0" },
                // also required for profiling
                new NameValuePairArgs { Name = "DiagnosticServices_EXTENSION_VERSION", Value = "~3" },

                // controls if the binary-rewrite engine will be turned on
                new NameValuePairArgs { Name = "InstrumentationEngine_EXTENSION_VERSION", Value = "~1" },
                // requires InstrumentationEngine; enables SQL logging
                new NameValuePairArgs { Name = "XDT_MicrosoftApplicationInsights_BaseExtensions", Value = "~1" },
                new NameValuePairArgs { Name = "XDT_MicrosoftApplicationInsights_Mode", Value = "recommended" },

                // when set to 1 will ignore the SDK that’s added as a NuGet package
                new NameValuePairArgs { Name = "XDT_MicrosoftApplicationInsights_PreemptSdk", Value = "0" },

                // visual studio snapshot debugger; not needed
                new NameValuePairArgs { Name = "SnapshotDebugger_EXTENSION_VERSION", Value = "disabled" },
            },
            ConnectionStrings = {
                new ConnStringInfoArgs {
                    Name = "Main",
                    Type = ConnectionStringType.SQLAzure,
                    ConnectionString = Output.Format($"Server=tcp:{sqlServer.Name}.database.windows.net;initial catalog={sqlDatabase.Name};User Id={sqlServer.AdministratorLogin};Password={sqlServerPassword};Min Pool Size=0;Max Pool Size=30;Persist Security Info=true;")
                }
            }
        }
    });

    // Add SQL firewall exceptions
    var sqlFirewallRules = appService.PossibleOutboundIpAddresses.Apply(ips =>
        ips.Split(',').Select(ip => new Sql.FirewallRule($"swisslog-sql-firewall-{ip}", new Sql.FirewallRuleArgs {
            ResourceGroupName = resourceGroup.Name,
            StartIpAddress = ip,
            EndIpAddress = ip,
            ServerName = sqlServer.Name,
        })).ToList()
    );

    return new Dictionary<string, object?> {
        { "webAppName", appService.Name },
        { "resourceGroupName", resourceGroup.Name }
    };
});