param name string
param location string = resourceGroup().location
param tags object = {}
param identityName string
param containerRegistryName string
param containerAppsEnvironmentName string
param applicationInsightsName string
param exists bool
param openAiSku object = {
  name:'S0'
}

var embeddingModelName = 'text-embedding-ada-002'
var embeddingDeploymentCapacity = 30
var redisPort = 10000

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' existing = {
  name: containerRegistryName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppsEnvironmentName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(subscription().id, resourceGroup().id, identity.id, 'acrPullRole')
  properties: {
    roleDefinitionId:  subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
  }
}

module fetchLatestImage '../modules/fetch-container-image.bicep' = {
  name: '${name}-fetch-image'
  params: {
    exists: exists
    name: name
  }
}

resource app 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: name
  location: location
  tags: union(tags, {'azd-service-name':  'OutputCacheDallESample' })
  dependsOn: [ acrPullRole]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress:  {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: identity.id
        }
      ]
      secrets: [
      ]
    }
    template: {
      containers: [
        {
          image: fetchLatestImage.outputs.?containers[?0].?image ?? 'cathyxwang/outputcachedallesample:latest'
          name: 'main'
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
            {
              name: 'PORT'
              value: '8080'
            }
            {
              name: 'apiKey'
              value: cognitiveAccount.listKeys().key1
            }
            {
              name: 'AZURE_OPENAI_ENDPOINT'
              value: cognitiveAccount.properties.endpoint
            }
            {
              name: 'AOAIResourceName'
              value: cognitiveAccount.name
            }
            {
              name: 'AOAIEmbeddingDeploymentName'
              value: textembeddingdeployment.name
            }
            {
              name: 'apiUrl'
              value: '${cognitiveAccount.properties.endpoint}openai/deployments/Dalle3/images/generations?api-version=2024-02-01'
            }
            {
              name: 'RedisCacheConnection'
              value: '${redisCache.properties.hostName}:10000,password=${redisdatabase.listKeys().primaryKey},ssl=True,abortConnect=False'
            }
            {
              name:'SemanticCacheAzureProvider'
              value: 'rediss://:${redisdatabase.listKeys().primaryKey}@${redisCache.properties.hostName}:10000'
            }
          ]
          resources: {
            cpu: json('1.0')
            memory: '2.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
  }
}

//azure open ai resource
resource cognitiveAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: '${name}-csaccount'
  location: location
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: '${name}-csaccount'
    publicNetworkAccess: 'Enabled'
  }
  sku: openAiSku
}

//ada text embedding service
resource textembeddingdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  name:'${name}-textembedding'
  parent: cognitiveAccount
  properties:{
    model: {
      format: 'OpenAI'
      name: embeddingModelName
      version: '2'
    }
  }
  sku: {
    name: 'Standard'
    capacity: embeddingDeploymentCapacity
  }
}

//azure cache for redis resource
resource redisCache 'Microsoft.Cache/redisEnterprise@2024-02-01' = {
  location:location
  name: '${name}-rediscache'
  sku:{
    capacity:2
    name: 'Enterprise_E10'
  }
}
resource redisdatabase 'Microsoft.Cache/redisEnterprise/databases@2024-02-01' = {
  name: 'default'
  parent: redisCache
  properties:{
    evictionPolicy:'NoEviction'
    clusteringPolicy:'EnterpriseCluster'
    modules:[
      {
        name: 'RediSearch'
      }
      {
        name:'RedisJSON'
      }
    ]
    port: redisPort
  }
}

output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output name string = app.name
output uri string = 'https://${app.properties.configuration.ingress.fqdn}'
output id string = app.id
