using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    public ReviewsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostReviewDto review)
    {
        var dbWr = review.Adapt<Review>();

        await _applicationContext.Reviews.AddAsync(dbWr);

        await _applicationContext.SaveChangesAsync();

        return Ok(new { dbWr.Id });
    }


    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] PutReviewDto review)
    {
        var fReview = await _applicationContext.Reviews.FirstAsync(r => r.Id == review.Id);

        fReview.Text = review.Text;
        fReview.Score = review.Score;
        fReview.Date = review.Date;

        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var review = await _applicationContext.Reviews.FirstAsync(s => s.Id == id);

        var user = await _applicationContext.Users.FirstAsync(s => s.Id == review.UserId);

        return Ok(new { review, user });
    }

    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.Reviews
            .AsNoTracking()
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        // var userIds = reviews.Select(u => u.UserId).ToArray();
        // var users = _applicationContext.Users.Where(u => userIds.Contains(u.Id));

        return Ok(reviews);
    }
}
