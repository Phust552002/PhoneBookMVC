using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PhoneBook.Models;

namespace PhoneBook.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteRepository _repo;

        public NoteController(INoteRepository repo) => _repo = repo;

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("list")]
        public async Task<IActionResult> GetNotes()
        {
            var notes = await _repo.GetNotesByUserAsync(CurrentUserId);
            return Ok(notes);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNote([FromBody] NoteRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "Tiêu đề không được để trống." });

            var id = await _repo.AddNoteAsync(CurrentUserId, req.Title.Trim(), req.Description?.Trim() ?? "");
            return Ok(new { NoteId = id });
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateNote([FromBody] NoteUpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "Tiêu đề không được để trống." });

            var ok = await _repo.UpdateNoteAsync(req.NoteId, CurrentUserId, req.Title.Trim(), req.Description?.Trim() ?? "");
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("delete/{noteId}")]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var ok = await _repo.DeleteNoteAsync(noteId, CurrentUserId);
            return ok ? Ok() : NotFound();
        }
    }

    public class NoteRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class NoteUpdateRequest : NoteRequest
    {
        public int NoteId { get; set; }
    }
}