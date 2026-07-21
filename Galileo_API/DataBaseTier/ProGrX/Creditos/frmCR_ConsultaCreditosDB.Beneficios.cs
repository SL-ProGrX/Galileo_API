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
        #region Beneficios

        /// <summary>
        /// Obtiene los beneficios de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneficiosConsultaData>> AFI_Beneficios_Consulta(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<AfiBeneficiosConsultaData>(
                codEmpresa,
                "spAFI_Beneficios_Consulta",
                new { Cedula = cedula });
        }

        #endregion
    }
}

