@description('The Azure region for the resources')
param location string

@description('Base name for the resources')
param baseName string

@description('SQL Server connection string')
@secure()
param sqlConnectionString string = ''

// App Service Plan (Linux, B1 Basic tier)
// B1 is used because Free (F1) tier has quota restrictions on new Azure subscriptions.
// B1 costs ~$13/month, well within the $200 free trial credit.
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${baseName}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// Web App (.NET 9, Linux)
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${baseName}-api'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: sqlConnectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output webAppDefaultHostName string = webApp.properties.defaultHostName
