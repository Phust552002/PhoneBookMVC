using PhoneBook.Models;

public interface INoteRepository
{
    Task<List<Note>> GetNotesByUserAsync(int userId);
    Task<int> AddNoteAsync(int userId, string title, string description);
    Task<bool> UpdateNoteAsync(int noteId, int userId, string title, string description);
    Task<bool> DeleteNoteAsync(int noteId, int userId);
}