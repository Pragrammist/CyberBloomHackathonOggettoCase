using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ReactionsController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    public ReactionsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostReactionDto reaction)
    {
        var dbWr = reaction.Adapt<Reaction>();

        await _applicationContext.Reactions.AddAsync(dbWr);

        await _applicationContext.SaveChangesAsync();

        return Ok(new { dbWr.Id });
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var fReview = await _applicationContext.Reactions.FirstAsync(r => r.Id == id);

        _applicationContext.Reactions.Remove(fReview);

        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var reaction = await _applicationContext.Reactions.FirstAsync(s => s.Id == id);

        return Ok(reaction);
    }

    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reactions = _applicationContext.Reactions.AsNoTracking().Skip(offset).Take(limit);

        return Ok(reactions);
    }
}
