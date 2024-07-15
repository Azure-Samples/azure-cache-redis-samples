param name string
param location string
param resourceToken string
param tags object

var prefix = '${name}-${resourceToken}'
//added for Redis Cache
var cacheServerName = '${prefix}-redisCache'
var webappSubnetName = 'webapp-subnet'
//added for Redis Cache
var cacheSubnetName = 'cache-subnet'
//added for Redis Cache
var cachePrivateEndpointName = 'cache-privateEndpoint'
//added for Redis Cache
var cachePvtEndpointDnsGroupName = 'cacheDnsGroup'
var abbrs = loadJsonContent('./abbreviations.json')
var redisAccessPolicyName = 'redisDataOwner'
var redisAccessPolicyAssignment = 'redisWebAppAssignment'

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2019-11-01' = {
  name: '${prefix}-vnet'
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: webappSubnetName
        properties: {
          addressPrefix: '10.0.1.0/24'
          delegations: [
            {
              name: '${prefix}-subnet-delegation-web'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: cacheSubnetName
        properties:{
          addressPrefix: '10.0.2.0/24'
        }
      }
    ]
  }
  resource webappSubnet 'subnets' existing = {
    name: webappSubnetName
  }
  //added for Redis Cache
  resource cacheSubnet 'subnets' existing = {
    name: cacheSubnetName
  }
}

// added for Redis Cache
resource privateDnsZoneCache 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.redis.cache.windows.net'
  location: 'global'
  tags: tags
  dependsOn:[
    virtualNetwork
  ]
}

 //added for Redis Cache
resource privateDnsZoneLinkCache 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
 parent: privateDnsZoneCache
 name: 'privatelink.redis.cache.windows.net-applink'
 location: 'global'
 properties: {
   registrationEnabled: false
   virtualNetwork: {
     id: virtualNetwork.id
   }
 }
}


resource cachePrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: cachePrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: virtualNetwork::cacheSubnet.id
    }
    privateLinkServiceConnections: [
      {
        name: cachePrivateEndpointName
        properties: {
          privateLinkServiceId: redisCache.id
          groupIds: [
            'redisCache'
          ]
        }
      }
    ]
  }
  resource cachePvtEndpointDnsGroup 'privateDnsZoneGroups' = {
    name: cachePvtEndpointDnsGroupName
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'privatelink-redis-cache-windows-net'
          properties: {
            privateDnsZoneId: privateDnsZoneCache.id
          }
        }
      ]
    }
  }
}

resource web 'Microsoft.Web/sites@2022-03-01' = {
  name: '${prefix}-app-service'
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
  }
  
  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      //"ENABLE_ORYX_BUILD" : "false", "SCM_DO_BUILD_DURING_DEPLOYMENT" : "false",
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
      ENABLE_ORYX_BUILD: 'false'
      RedisHostName: redisCache.properties.hostName
    }
  }

  resource logs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: {
        fileSystem: {
          level: 'Verbose'
        }
      }
      detailedErrorMessages: {
        enabled: true
      }
      failedRequestsTracing: {
        enabled: true
      }
      httpLogs: {
        fileSystem: {
          enabled: true
          retentionInDays: 1
          retentionInMb: 35
        }
      }
    }
  }

  resource webappVnetConfig 'networkConfig' = {
    name: 'virtualNetwork'
    properties: {
      subnetResourceId: virtualNetwork::webappSubnet.id
    }
  }

  dependsOn: [virtualNetwork]

}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${prefix}-service-plan'
  location: location
  tags: tags
  sku: {
    name: 'S1'
  }
  properties: {
    reserved: true
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: '${prefix}-workspace'
  location: location
  tags: tags
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

module applicationInsightsResources './core/monitor/applicationinsights.bicep' = {
  name: 'applicationinsights-resources'
  params: {
    name: '${prefix}-appinsights'
    dashboardName:'${prefix}-dashboard'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
  }
  
}

module keyvault 'core/security/keyvault.bicep' = {
  name:'${abbrs.keyVaultVaults}${resourceToken}'
  params:{
    location: location
    name:'${abbrs.keyVaultVaults}${resourceToken}'
  }
}

//added for Redis Cache
resource redisCache 'Microsoft.Cache/redis@2024-03-01' = {
  location:location
  name:cacheServerName
  properties:{
    sku: {
      capacity:1
      family:'C'
      name:'Standard'
    }
  }
}

resource redisAccessPolicy 'Microsoft.Cache/redis/accessPolicies@2024-03-01' = {
  name: redisAccessPolicyName
  parent: redisCache
  properties:{
    permissions:'+@all allkeys'
  }
}

resource redisAccessPolicyAssignmentName 'Microsoft.Cache/redis/accessPolicyAssignments@2024-03-01' = {
  name: redisAccessPolicyAssignment
  parent: redisCache
  properties: {
    accessPolicyName: redisAccessPolicy.name
    objectId: web.identity.principalId
    objectIdAlias: 'webapp'
  }
}


output WEB_URI string = 'https://${web.properties.defaultHostName}'
output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsightsResources.outputs.connectionString
