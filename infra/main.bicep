@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('Base name used for all resources')
param baseName string = 'koffiemachine'

@description('SQL Server administrator login')
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

// --- SQL Database ---
module sql 'modules/sqlDatabase.bicep' = {
  name: 'sql-deployment'
  params: {
    location: location
    baseName: baseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
  }
}

// --- App Service (API) ---
module appService 'modules/appService.bicep' = {
  name: 'appservice-deployment'
  params: {
    location: location
    baseName: baseName
    sqlConnectionString: sql.outputs.connectionString
  }
}

// --- Static Web App (Frontend) ---
module staticWebApp 'modules/staticWebApp.bicep' = {
  name: 'staticwebapp-deployment'
  params: {
    location: location
    baseName: baseName
    apiBaseUrl: 'https://${appService.outputs.webAppDefaultHostName}'
  }
}

// --- Outputs ---
output apiUrl string = 'https://${appService.outputs.webAppDefaultHostName}'
output frontendUrl string = 'https://${staticWebApp.outputs.staticWebAppDefaultHostName}'
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
