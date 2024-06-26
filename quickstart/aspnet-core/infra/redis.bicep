param name string
param location string

param cacheSKUName string = 'Basic'
param cacheSKUFamily string = 'C'
param cacheSKUCapacity int = 0

resource cache 'Microsoft.Cache/redis@2024-04-01-preview' = {
  name: name
  location: location
  tags: {
    displayName: 'cache'
  }
  properties: {
    sku: {
      name: cacheSKUName
      family: cacheSKUFamily
      capacity: cacheSKUCapacity
    }
    redisConfiguration: {
      'aad-enabled': 'true'
    }
  }
}

output redisKey string = cache.listKeys().primaryKey

