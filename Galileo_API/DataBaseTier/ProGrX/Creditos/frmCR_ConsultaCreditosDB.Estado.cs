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
        #region Estado

        public ErrorDto<EmpresaEnlaceResultDto> ConsultaVersionEmpresa(int codEmpresa)
        {
            var lista = EmpresaEnlaceObtener(codEmpresa);
            return lista.Count > 0
                ? DbHelper.CreateOkResponse(lista[0])
                : DbHelper.CreateErrorResponse("No se encontró información de la empresa.", -1, new EmpresaEnlaceResultDto());
        }

        public List<EmpresaEnlaceResultDto> EmpresaEnlaceObtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<EmpresaEnlaceResultDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select 
                        cod_empresa_enlace,
                        Nombre,
                        SysCrdPlanPago,
                        SysDocVersion,
                        SysTesVersion, 
                        SYS_CCSS_IND,
                        ec_visible_patrimonio,
                        ec_visible_fondos,
                        ec_visible_creditos,
                        ec_visible_fianzas,
                        estadoCuenta
                  from dbo.sif_empresa");

            return result.Code == 0 ? result.Result ?? new List<EmpresaEnlaceResultDto>() : new List<EmpresaEnlaceResultDto>();
        }

        #endregion
    }
}

