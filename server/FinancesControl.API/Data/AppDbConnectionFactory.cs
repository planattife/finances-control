using System.Data;
using Npgsql;

namespace FinancesControl.API.Data;

public class AppDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public AppDbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
}
