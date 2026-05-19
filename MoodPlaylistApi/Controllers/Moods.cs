using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodsController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet] 

        public async Task<IActionResult> GetMoods()
        {
            var moods = await _context.Moods.ToListAsync();
            return Ok(moods);
        }
    }

}
