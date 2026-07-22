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
        #region Patrimonio

        /// <summary>
        /// Obtiene el patrimonio de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPatrimonioData>> CR_Patrimonio_Obtener(int codEmpresa, string cedula, string tipo)
        {
            return DbHelper.ExecuteListQuery<CrPatrimonioData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT TOP 30
                    Ah.*,
                    ISNULL(Doc.Descripcion, '') AS DocDesc,
                    ISNULL(Con.Descripcion, '') AS ConDesc,
                    CASE Ah.Tipo
                        WHEN 'O' THEN 'Obrero'
            WHEN 'P' THEN 'Patronal'
            WHEN 'X' THEN 'AP.Custodia'
            WHEN 'C' THEN 'Capitalización'
            WHEN 'E' THEN 'Extraordinario'
            ELSE Ah.Tipo
                    END AS Tipo
                FROM Ahorro_Detallado Ah
                LEFT JOIN SIF_Documentos Doc 
                       ON Ah.Tcon = Doc.Tipo_Documento
                LEFT JOIN SIF_Conceptos Con 
                       ON Ah.cod_Concepto = Con.cod_Concepto
                WHERE Ah.Cedula = @Cedula
      AND ((@Tipo = 'T' AND Ah.Tipo IN ('O', 'P', 'C', 'E', 'X')) OR Ah.Tipo = @Tipo)
                ORDER BY Ah.Fecha DESC;",
                new { Cedula = cedula, Tipo = tipo });
        }

        /// <summary>
        /// Obtiene los periodos visibles para un socio
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<ExcPeriodosVisiblesData>> EXC_Periodos_Visibles_Obtener(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<ExcPeriodosVisiblesData>(
                codEmpresa,
                "spEXC_Periodos_Visibles",
                new { Cedula = cedula });
        }

        #endregion
    }
}

