using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoERPApi.Tests.Helpers
{
    internal class TestDatabaseFixture
    {
        public async Task Reset()
        {
            using var conn = new SqlConnection("YOUR_CONNECTION_STRING");
            await conn.OpenAsync();

            var cmd = new SqlCommand(File.ReadAllText("reset.sql"), conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}


