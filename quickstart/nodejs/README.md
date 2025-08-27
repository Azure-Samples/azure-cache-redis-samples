# Node.js Redis Sample

This is a sample application demonstrating how to use Azure Managed Redis with a Node.js application.

## Prerequisites

- Node.js LTS
- Azure subscription
- Azure Managed Redis instance with Entra ID authentication enabled and appropriate data access policy assignments


## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/Azure-Samples/azure-cache-redis-samples.git
   cd azure-cache-redis-samples/quickstart/nodejs
   ```

2. Install dependencies:

   ```bash
   cd quickstart/nodejs
   npm install
   ```

3. Copy `sample.env` to a `.env` file and add your Azure Managed Redis endpoint:

   ```env
   AZURE_MANAGED_REDIS_ENDPOINT="<REDIS_HOST_NAME>:<PORT>"

   ```

4. Build and run the application:

   ```bash
   npm run build && npm start
   ```

5. Review results:

    ```console
    Ping result: PONG
    Set result: OK
    Get result: Hello! The cache is working from Node.js!
    ```

