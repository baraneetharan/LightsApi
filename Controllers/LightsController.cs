using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class LightController : ControllerBase
{
    private readonly LightContext _context;

    public LightController(LightContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Light>>> GetLights()
    {
        return await _context.Lights.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Light>> GetLight(int id)
    {
        var light = await _context.Lights.FindAsync(id);

        if (light == null)
        {
            return NotFound();
        }

        return light;
    }

    [HttpPost]
    public async Task<ActionResult<Light>> PostLight(Light light)
    {
        _context.Lights.Add(light);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLight), new { id = light.Id }, light);
    }

    // POST: api/LightModels/multiple
        [HttpPost("multiple")]
        public async Task<ActionResult<IEnumerable<Light>>> PostMultipleLightModels(IEnumerable<Light> lights)
        {
            _context.Lights.AddRange(lights);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLight", lights);
        }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutLight(int id, Light light)
    {
        if (id != light.Id)
        {
            return BadRequest();
        }

        _context.Entry(light).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLight(int id)
    {
        var light = await _context.Lights.FindAsync(id);
        if (light == null)
        {
            return NotFound();
        }

        _context.Lights.Remove(light);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
