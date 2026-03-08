using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaAsientosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXUtilEliminaAsientosDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXUtilEliminaAsientosDb(
            PortalDB portalDb,
            MSecurityMainDb mSecurityMainDb)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mSecurityMainDb;
        }

        /// <summary>
        /// Obtine los tipos de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(
            int codEmpresa,
            int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT tipo_asiento as item,
                    RTRIM(tipo_asiento) + ' - ' + RTRIM(descripcion) AS descripcion
                      FROM CntX_Tipos_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad",
                    new { cod_contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Calcula los asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="tipo_asiento"></param>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<int> Cntx_Util_Asientos_Calcular(int codEmpresa,int cod_contabilidad,string tipo_asiento,
                DateTime desde,DateTime hasta,int anio,int mes)
        {
            var response = new ErrorDto<int>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var total = cn.ExecuteScalar<int>(
                @"SELECT COUNT(*)
          FROM Cntx_Asientos
          WHERE anio = @anio
          AND mes = @mes
          AND tipo_asiento = @tipo_asiento
          AND fecha_asiento BETWEEN @desde AND @hasta
          AND fecha_aplicado IS NULL
          AND cod_contabilidad = @cod_contabilidad
          AND modulo = 20",
                new
                {
                    cod_contabilidad,
                    tipo_asiento,
                    desde,
                    hasta,
                    anio,
                    mes
                });

                response.Result = total;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina los asientos
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cntx_Util_Asientos_Eliminar(CntxEliminarAsientosRequestDto request)
        {
            var response = new ErrorDto<bool>();

            if (!request.cod_empresa.HasValue)
            {
                response.Code = -1;
                response.Description = "El campo cod_empresa es obligatorio.";
                return response;
            }

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(request.cod_empresa.Value));

                cn.Open();

                var sql = @"

                    DELETE Cntx_Asientos_detalle
                    WHERE tipo_asiento = @tipo_asiento
                    AND num_asiento BETWEEN @desde AND @hasta
                    AND cod_contabilidad = @cod_contabilidad

                    DELETE Cntx_Asientos
                    WHERE tipo_asiento = @tipo_asiento
                    AND num_asiento BETWEEN @desde AND @hasta
                    AND cod_contabilidad = @cod_contabilidad
                    AND fecha_aplicado IS NULL
                    AND anio = @anio
                    AND mes = @mes
                    AND modulo = 20";

                cn.Execute(sql, request);

                _mSecurityMainDb.Bitacora(
                    new Galileo.Models.Security.BitacoraInsertarDto
                    {
                        EmpresaId = request.cod_empresa.Value,
                        Usuario = request.usuario,
                        Movimiento = "Elimina Asientos - WEB",
                        DetalleMovimiento =
                            $"Tipo:{request.tipo_asiento} D:{request.desde} H:{request.hasta}",
                        Modulo = 20
                    });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene el periodo actual
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CntxPeriodoActualDto> Cntx_PeriodoActual_Obtener(int codEmpresa,int cod_contabilidad)
        {
            var response = new ErrorDto<CntxPeriodoActualDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var periodo = cn.QueryFirstOrDefault<CntxPeriodoActualDto>(
                @"SELECT TOP 1
              anio,
              mes
          FROM CntX_Periodos
          WHERE cod_contabilidad = @cod_contabilidad
          AND estado = 'P'
          ORDER BY anio ASC, mes ASC",
                new { cod_contabilidad });

                response.Result = periodo;
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