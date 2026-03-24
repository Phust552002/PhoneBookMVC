using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PhoneBook.Models;
using System.Data;

public class NoteRepository : INoteRepository
{
    private readonly string _connectionString;

    public NoteRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("HanhChinhSo")!;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    // SP: Phonebook_GetNotesByUser
    public async Task<List<Note>> GetNotesByUserAsync(int userId)
    {
        using var conn = CreateConnection();
        var result = await conn.QueryAsync<Note>(
            "Phonebook_GetNotesByUser",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
        return result.ToList();
    }

    // SP: Phonebook_InsertNote
    public async Task<int> AddNoteAsync(int userId, string title, string description)
    {
        using var conn = CreateConnection();
        var noteId = await conn.ExecuteScalarAsync<int>(
            "Phonebook_InsertNote",
            new { UserId = userId, Title = title, Description = description },
            commandType: CommandType.StoredProcedure);
        return noteId;
    }

    // SP: Phonebook_UpdateNote
    public async Task<bool> UpdateNoteAsync(int noteId, int userId, string title, string description)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "Phonebook_UpdateNote",
            new { NoteId = noteId, UserId = userId, Title = title, Description = description },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    // SP: Phonebook_DeleteNote
    public async Task<bool> DeleteNoteAsync(int noteId, int userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "Phonebook_DeleteNote",
            new { NoteId = noteId, UserId = userId },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}