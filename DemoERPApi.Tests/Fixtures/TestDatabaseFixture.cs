using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace DemoERPApi.Tests.Fixtures;

public class TestDatabaseFixture
{
    private readonly string _connectionString =
        "Your_Test_Database_Connection_String";

    public async Task ResetAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sqlPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Scripts",
            "reset.sql"
        );

        var sql = await File.ReadAllTextAsync(sqlPath);

        using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}