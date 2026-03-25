using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_CxC
{
    public class FrmAfCdControlDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdControlDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmAfCdControlDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// En lista los resultados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfcdCuentaDto>> Listar(int codEmpresa, AfcdCuentaFiltroDto filtro)
        {
            var response = new ErrorDto<List<AfcdCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var fechaInicio = filtro.todas == true
                 ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                 : filtro.fecha_inicio;

                var fechaFin = filtro.todas == true
                    ? new DateTime(2300, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                    : filtro.fecha_fin;


                var result = cn.Query<AfcdCuentaDto>(
                        "spAFI_CD_Cuenta_List",
                        new
                        {
                            Comite = string.IsNullOrEmpty(filtro.comite) ? "" : filtro.comite,
                            Emite = string.IsNullOrEmpty(filtro.tipo) ? "" : filtro.tipo, 
                            FInicio = fechaInicio,
                            FCorte = fechaFin,
                            Proceso = string.IsNullOrEmpty(filtro.proceso) ? "" : filtro.proceso,
                            Estado = string.IsNullOrEmpty(filtro.estado) ? "" : filtro.estado,
                            TesoreriaId = filtro.tesoreria_id
                        },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// En lista los tipos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tipos(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                CodTipoCuenta AS item,
                NombreTipoCuenta AS descripcion
            FROM AFI_CD_TIPO_CUENTA
            WHERE Activo = 1
            ORDER BY CodTipoCuenta
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// En lista los procesos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Procesos(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                CodTipoProceso AS item,
                NombreTipoProceso AS descripcion
            FROM AFI_CD_TIPO_PROCESO
            WHERE Activo = 1
            ORDER BY CodTipoProceso
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// En lista los estados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Estados(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                CodEstado AS item,
                NombreEstado AS descripcion
            FROM AFI_CD_TIPOS_ESTADOS_CUENTAS
            WHERE Activo = 1
            ORDER BY CodEstado
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// En lista los comites
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Comites(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                COD_COMITE AS item,
                DESCRIPCION AS descripcion
            FROM AFI_CD_COMITES
            WHERE ACTIVO = 1
            ORDER BY COD_COMITE
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}