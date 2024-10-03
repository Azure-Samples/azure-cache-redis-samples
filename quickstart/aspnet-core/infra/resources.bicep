param name string
param location string
param resourceToken string
param tags object

var prefix = '${name}-${resourceToken}'
var cacheServerName = '${prefix}-redisCache'
var webappSubnetName = 'webapp-subnet'
var cacheSubnetName = 'cache-subnet'
var cachePrivateEndpointName = 'cache-privateEndpoint'
var cachePvtEndpointDnsGroupName = 'cacheDnsGroup'

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
  resource cacheSubnet 'subnets' existing = {
    name: cacheSubnetName
  }
}

resource privateDnsZoneCache 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.redis.cache.windows.net'
  location: 'global'
  tags: tags
  dependsOn:[
    virtualNetwork
  ]
}

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

resource redisCache 'Microsoft.Cache/redis@2024-03-01' = {
  location:location
  name:cacheServerName
  properties:{
    redisConfiguration:{
      'aad-enabled':'true'
    }
    sku: {
      capacity:1
      family:'C'
      name:'Standard'
    }
  }
  identity:{
    type:'SystemAssigned'
  }
}

resource redisAccessPolicyAssignmentName 'Microsoft.Cache/redis/accessPolicyAssignments@2024-03-01' = {
  name: redisAccessPolicyAssignment
  parent: redisCache
  properties: {
    accessPolicyName: 'Data Owner'
    objectId: web.identity.principalId
    objectIdAlias: 'webapp'
  }
}

output WEB_URI string = 'https://${web.properties.defaultHostName}'

