using Azure.AI.OpenAI;
using Azure;
using Redis.OM;
using Redis.OM.Vectorizers;

namespace OutputCacheDallESample
{
    public static class GenerateImageSC
    {
        private static RedisConnectionProvider? _provider;
        public static async Task GenerateImageSCAsync(HttpContext context, string _prompt, IConfiguration _config)
        {
            string imageURL;

            string? endpoint = _config["AZURE_OPENAI_ENDPOINT"];
            string? key = _config["apiKey"];
            string? semanticCacheAzureProvider = _config["SemanticCacheAzureProvider"];
            string? AOAIResourceName = _config["AOAIResourceName"];
            string? AOAIEmbeddingDeploymentName = _config["AOAIEmbeddingDeploymentName"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(semanticCacheAzureProvider) || string.IsNullOrEmpty(AOAIResourceName) || string.IsNullOrEmpty(AOAIEmbeddingDeploymentName))
            {
                Console.WriteLine("Please set the follow environment variables:");
                Console.WriteLine("Azure Open AI endpoint: AZURE_OPENAI_ENDPOINT");
                Console.WriteLine("AOAI api key: apiKey");
                Console.WriteLine("Azure Cache for Redis Enterprise connection string: SemanticCacheAzureProvider");
                Console.WriteLine("Azure Open AI Resource name: AOAIResourceName");
                Console.WriteLine("Azure Open AI embedding deployment name: AOAIEmbeddingDeploymentName");
            }
            else 
            {
                _provider = new RedisConnectionProvider(semanticCacheAzureProvider);
                var cache = _provider.AzureOpenAISemanticCache(key, AOAIResourceName, AOAIEmbeddingDeploymentName, 1536);

                if (cache.GetSimilar(_prompt).Length > 0)
                {
                    imageURL = cache.GetSimilar(_prompt)[0];
                    await context.Response.WriteAsync("<!DOCTYPE html><html><body> " +
                                                      $"<img src=\"{imageURL}\" alt=\"AI Generated Picture {_prompt}\" width=\"460\" height=\"345\">" +
                                                      " </body> </html>");
                }
                else
                {
                    OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

                    Response<ImageGenerations> imageGenerations = await client.GetImageGenerationsAsync(
                        new ImageGenerationOptions()
                        {
                            Prompt = _prompt,
                            Size = ImageSize.Size256x256,
                        });

                    // Image Generations responses provide URLs you can use to retrieve requested images
                    imageURL = imageGenerations.Value.Data[0].Url.AbsoluteUri;

                    await cache.StoreAsync(_prompt, imageURL);

                    await context.Response.WriteAsync("<!DOCTYPE html><html><body> " +
                    $"<img src=\"{imageURL}\" alt=\"AI Generated Picture {_prompt}\" width=\"460\" height=\"345\">" +
                    " </body> </html>");
                }
            }            
        }
    }
}
