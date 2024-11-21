using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.Threading.Tasks;

namespace SemanticKernelWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly LightPlugin _lightPlugin;
        private readonly Kernel _kernel;

        public ChatController(LightPlugin lightPlugin, Kernel kernel)
        {
            _lightPlugin = lightPlugin;
            _kernel = kernel;
        }

        [HttpPost]
        public async Task<IActionResult> Chat(string userInput)
        {
            Console.WriteLine("*******ChatController userInput********" +userInput);
            try
            {
                var result = await _lightPlugin.ChatAsync(userInput);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Return an error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
