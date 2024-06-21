using MySql.Data.MySqlClient;
using System.Data;

namespace order.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionstring;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionstring = _configuration.GetConnectionString("MySqlConnection");
        }

        public IDbConnection CreateConnection() => new MySqlConnection(_connectionstring);
    }
}
