using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_CxC
{
    public class FrmAfCdControlDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdControlDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// En lista los resultados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfcdCuentaDto>> Listar(int codEmpresa, AfcdCuentaFiltroDto filtro)
        {
                var fechaInicio = filtro.todas == true
                 ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                 : filtro.fecha_inicio;

                var fechaFin = filtro.todas == true
                    ? new DateTime(2300, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                    : filtro.fecha_fin;

                return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var data = conn.Query<AfcdCuentaDto>(
                        "spAFI_CD_Cuenta_List",
                        new
                        {
                            Comite = NormalizarValorFiltro(filtro.comite),
                            Emite = NormalizarValorFiltro(filtro.tipo),
                            FInicio = fechaInicio,
                            FCorte = fechaFin,
                            Proceso = NormalizarValorFiltro(filtro.proceso),
                            Estado = NormalizarValorFiltro(filtro.estado),
                            TesoreriaId = filtro.tesoreria_id
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return data;
                });
            }

        private static string NormalizarValorFiltro(string? valor)
        {
            var filtro = valor?.Trim() ?? string.Empty;
            return filtro.Equals("TODOS", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : filtro;
        }

        /// <summary>
        /// En lista los tipos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tipos(int codEmpresa)
        {
            const string sql = @"
            SELECT 
                CodTipoCuenta AS item,
                NombreTipoCuenta AS descripcion
            FROM AFI_CD_TIPO_CUENTA
            WHERE Activo = 1
            ORDER BY CodTipoCuenta
        ";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(sql).ToList(); 
            });

        }

        /// <summary>
        /// En lista los procesos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Procesos(int codEmpresa)
        {
            const string sql = @"
            SELECT 
                CodTipoProceso AS item,
                NombreTipoProceso AS descripcion
            FROM AFI_CD_TIPO_PROCESO
            WHERE Activo = 1
            ORDER BY CodTipoProceso
        ";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// En lista los estados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Estados(int codEmpresa)
        {
            const string sql = @"
            SELECT 
                CodEstado AS item,
                NombreEstado AS descripcion
            FROM AFI_CD_TIPOS_ESTADOS_CUENTAS
            WHERE Activo = 1
            ORDER BY CodEstado
        ";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });

        }

        /// <summary>
        /// En lista los comites
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Comites(int codEmpresa)
        {
            const string sql = @"
            SELECT 
                COD_COMITE AS item,
                DESCRIPCION AS descripcion
            FROM AFI_CD_COMITES
            WHERE ACTIVO = 1
            ORDER BY COD_COMITE
        ";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
    }
}
