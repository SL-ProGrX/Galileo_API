using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Correo

        /// <summary>
        /// Obtiene los correos de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<SysMailLoadData>> Sys_Mail_Load(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<SysMailLoadData>(
                codEmpresa,
                "spSys_Mail_Load",
                new { Cedula = cedula });
        }


        #endregion
    }
}

