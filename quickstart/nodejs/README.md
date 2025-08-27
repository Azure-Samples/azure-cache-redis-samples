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

3. Copy `sample.env` to a `.env` file and add your Azure Managed Redis endpoint. This endpoint can be found in the Azure portal. It includes the port:

   ```ini
   REDIS_ENDPOINT=<redis-host-with-port>
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

**Note**: This quickstart code uses a fail fast `reconnectStrategy` which is suitable only in sample code. The purpose is to quickly demonstrate the functionality without getting stuck in reconnection loops if your endpoint or authentication is not correctly configured. In production code, a more robust `reconnectStrategy` should be implemented.
