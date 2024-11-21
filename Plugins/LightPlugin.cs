using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class LightPlugin
{
    private readonly LightContext _context;
    private readonly Kernel _kernel;

    public LightPlugin(LightContext context, Kernel kernel)
    {
        _context = context;
        _kernel = kernel;
    }

    [KernelFunction("get all lights")]
    [Description("Gets a list of lights and their current state")]
    public async Task<List<Light>> GetLightsAsync()
    {
        Console.WriteLine("*******LightPlugin GetLightsAsync********");
        return await _context.Lights.ToListAsync();
    }

    [KernelFunction("get light by id")]
    [Description("Gets a single light by its ID")]
    public async Task<Light?> GetLightByIdAsync(int id)
    {
        var light = await _context.Lights.FindAsync(id);
        return light;
    }

    [KernelFunction("create light")]
    [Description("Creates a new light")]
    public async Task<Light> CreateLightAsync(Light light)
    {
        _context.Lights.Add(light);
        await _context.SaveChangesAsync();
        return light;
    }

    [KernelFunction("create multiple lights")]
    [Description("Creates multiple new lights")]
    public async Task<IEnumerable<Light>> CreateMultipleLightsAsync(IEnumerable<Light> lights)
    {
        _context.Lights.AddRange(lights);
        await _context.SaveChangesAsync();
        return lights;
    }

    [KernelFunction("update light")]
    [Description("Updates an existing light")]
    public async Task<Light?> UpdateLightAsync(int id, Light light)
    {
        if (id != light.Id)
        {
            throw new ArgumentException("ID mismatch");
        }

        _context.Entry(light).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return light;
    }

    [KernelFunction("delete light")]
    [Description("Deletes a light by its ID")]
    public async Task<bool> DeleteLightAsync(int id)
    {
        var light = await _context.Lights.FindAsync(id);
        if (light == null)
        {
            return false;
        }

        _context.Lights.Remove(light);
        await _context.SaveChangesAsync();

        return true;
    }

    [KernelFunction("change state")]
    [Description("Changes the state of the light")]
    public async Task<Light?> ChangeStateAsync(int id, bool isOn)
    {
        var light = await _context.Lights.FindAsync(id);
        if (light == null) return null;

        light.IsOn = isOn;
        _context.Entry(light).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task<string> ChatAsync(string userInput)
    {
        Console.WriteLine("*******LightPlugin userInput********" + userInput);

        if (userInput.Contains("get all lights"))
        {
            var lights = await GetLightsAsync();
            return string.Join("\n", lights.Select(light => $"Light ID: {light.Id}, Name: {light.Name}, State: {(light.IsOn.GetValueOrDefault() ? "On" : "Off")}"));
        }
        else if (userInput.Contains("get light by id"))
        {
            int id = ExtractIdFromUserInput(userInput);
            var light = await GetLightByIdAsync(id);
            return light != null ? $"Light ID: {light.Id}, Name: {light.Name}, State: {(light.IsOn.GetValueOrDefault() ? "On" : "Off")}" : "Light not found";
        }
        else if (userInput.StartsWith("create light", StringComparison.OrdinalIgnoreCase))
        {
            string lightName = ExtractLightNameFromUserInput(userInput);
            var newLight = new Light { Name = lightName, IsOn = false }; // Assuming new lights are created with state 'Off'
            var createdLight = await CreateLightAsync(newLight);
            return $"Created Light ID: {createdLight.Id}, Name: {createdLight.Name}, State: {(createdLight.IsOn.GetValueOrDefault() ? "On" : "Off")}";
        }
        else if (userInput.Contains("create multiple lights"))
        {
            // Assuming user input contains details needed to create multiple lights.
            // This would need to be parsed from user input.
            var newLights = new List<Light> { /* Assign properties here */ };
            var createdLights = await CreateMultipleLightsAsync(newLights);
            return "Created multiple lights successfully.";
        }
        else if (userInput.Contains("update light"))
        {
            int id = ExtractIdFromUserInput(userInput);
            var updatedLight = new Light { Id = id, /* Assign other properties here */ };
            var updatedResult = await UpdateLightAsync(id, updatedLight);
            return $"Updated Light ID: {updatedResult.Id}, Name: {updatedResult.Name}, State: {(updatedResult.IsOn.GetValueOrDefault() ? "On" : "Off")}";
        }
        else if (userInput.Contains("delete light"))
        {
            int id = ExtractIdFromUserInput(userInput);
            bool isDeleted = await DeleteLightAsync(id);
            return isDeleted ? "Light deleted successfully" : "Light not found";
        }
        else if (userInput.Contains("change state"))
        {
            int id = ExtractIdFromUserInput(userInput);
            bool newState = ExtractStateFromUserInput(userInput);
            var stateResult = await ChangeStateAsync(id, newState);
            return stateResult != null ? $"Changed state of Light ID: {stateResult.Id} to {(stateResult.IsOn.GetValueOrDefault() ? "On" : "Off")}" : "Light not found";
        }

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage(userInput);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var completionResult = await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);
        Console.WriteLine(completionResult.Items.ToString());
        history.AddMessage(completionResult.Role, completionResult.Content ?? string.Empty);

        return completionResult.Content;
    }

    private int ExtractIdFromUserInput(string userInput)
    {
        var match = Regex.Match(userInput, @"#(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
        {
            return id;
        }
        throw new ArgumentException("Invalid user input. No valid ID found.");
    }

    private string ExtractLightNameFromUserInput(string userInput)
    {
        // Example userInput: "Create light Table Lamp"
        var match = Regex.Match(userInput, @"create light (.+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        throw new ArgumentException("Invalid user input. No valid light name found.");
    }

    private bool ExtractStateFromUserInput(string userInput)
    {
        if (userInput.Contains("On"))
        {
            return true;
        }
        if (userInput.Contains("Off"))
        {
            return false;
        }
        throw new ArgumentException("Invalid user input. No valid state found.");
    }
}