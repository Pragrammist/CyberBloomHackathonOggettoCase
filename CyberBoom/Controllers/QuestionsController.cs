using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    public QuestionsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostQuestionDto question)
    {
        var dbWr = question.Adapt<Question>();
        await _applicationContext.Questions.AddAsync(dbWr);

        await _applicationContext.SaveChangesAsync();

        return Ok(new { dbWr.Id });
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] PutQuestionDto question)
    {
        var fReview = await _applicationContext.Questions.FirstAsync(r => r.Id == question.Id);

        fReview.Text = question.Text;

        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var question = await _applicationContext.Questions.FirstAsync(s => s.Id == id);

        return Ok(question);
    }

    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var questions = _applicationContext.Questions
            .AsNoTracking()
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        return Ok(questions);
    }
}
