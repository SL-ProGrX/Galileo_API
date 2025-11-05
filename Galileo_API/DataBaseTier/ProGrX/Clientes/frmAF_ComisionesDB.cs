using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_ComisionesDB
    {
        private readonly IConfiguration _config;

        public frmAF_ComisionesDB(IConfiguration config)
        {
            _config = config;
        }

  

    }
}