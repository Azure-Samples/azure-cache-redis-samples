using Azure.AI.OpenAI;
using Azure;

namespace OutputCacheDallESample
{
    public static class GenerateImageSDK
    {
        public static async Task GenerateImageSDKAsync(HttpContext context, string _prompt, IConfiguration _config)
        {
            string? endpoint = _config["AZURE_OPENAI_ENDPOINT"];
            string? key = _config["apiKey"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Please set the Azure Open AI endpoint and key");
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
                string imageURL = imageGenerations.Value.Data[0].Url.AbsoluteUri;
                //await context.Response.WriteAsync($"<img src={imageURL}/>");
                await context.Response.WriteAsync("<!DOCTYPE html><html><body> " +
                $"<img src=\"{imageURL}\" alt=\"Flowers in Chania\" width=\"460\" height=\"345\">" +
                " </body> </html>");
            }          
        }
    }
}
