using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.Security; 
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaDB
    {

        public FrmCxCConsultaDB(IConfiguration config)
        {

        }

    
        public static ErrorDto CxCClientesClasifica_Guardar(int codEmpresa, string usuario, CxCConsultaData datos)
        {
       

            return Ok(); 
        }

        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

    }
}

