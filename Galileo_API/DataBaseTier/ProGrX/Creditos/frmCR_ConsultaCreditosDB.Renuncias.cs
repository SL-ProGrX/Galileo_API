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
        #region Renuncias
        /// <summary>
        /// Obtiene las renuncias en tránsito de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiRenunciaTransitoData>> AFI_ConsultaRenunciaTransito(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<AfiRenunciaTransitoData>(
                codEmpresa,
                "spAFI_ConsultaRenunciaTransito",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene las renuncias de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiRenunciasConsultaData>> AFI_Renuncias_Consulta(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<AfiRenunciasConsultaData>(
                codEmpresa,
                "spAFI_Renuncias_Consulta",
                new { Cedula = cedula });
        }

        #endregion
    }
}

