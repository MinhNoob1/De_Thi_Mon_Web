using Dapper;
using DAL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class KhachHangDAL : BaseDAL
    {
        public KhachHangDAL(IConfiguration configuration) : base(configuration) { }

    }
}
