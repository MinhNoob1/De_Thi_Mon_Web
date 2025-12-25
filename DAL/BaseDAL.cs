using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class BaseDAL
    {
        protected readonly string connectionString;

        public BaseDAL(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("LiteCommerceDB")
                ?? throw new InvalidOperationException("Chuỗi kết nối 'LiteCommerceDB' không tìm thấy trong cấu hình.");
        }

        public async Task<IDbConnection> OpenConnectionAsync()
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
