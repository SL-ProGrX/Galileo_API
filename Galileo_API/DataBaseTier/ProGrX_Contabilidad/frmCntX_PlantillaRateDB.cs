using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaRateDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 21; // módulo contabilidad

        public FrmCntXPlantillaRateDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXPlantillaRateDb(
            PortalDB portalDb,
            MSecurityMainDb securityDb)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = securityDb;
        }

        #region ===================== HELPER =====================

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarDropDownQuery(
            int codEmpresa,
            string sql,
            object? parametros = null)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    parametros
                ).ToList();

                response.Result = result;
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        /// <summary>
        /// Metodo para scroll 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Scroll_Obtener(
            int codEmpresa,
            int scrollCode,
            int? codPlantilla)
        {
            string query =
                "SELECT TOP 1 CodPlantilla FROM CNTX_PLANTILLA_RATE ";

            if (codPlantilla.HasValue)
            {
                if (scrollCode == 1)
                    query += "WHERE CodPlantilla > @codPlantilla ORDER BY CodPlantilla ASC";
                else
                    query += "WHERE CodPlantilla < @codPlantilla ORDER BY CodPlantilla DESC";
            }

            var codResult = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new { codPlantilla });

            if (codResult.Result == 0)
            {
                return new ErrorDto<CntxPlantillaRateDto>
                {
                    Code = -2,
                    Description = "No se encontraron registros"
                };
            }

            return CntxPlantillaRate_Consulta_Obtener(
                codEmpresa,
                codResult.Result);
        }

        /// <summary>
        /// Obtiene las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Consulta_Obtener(
            int codEmpresa,
            int codPlantilla)
        {
            string queryCab = @"
                SELECT *
                FROM CNTX_PLANTILLA_RATE
                WHERE CodPlantilla = @codPlantilla";

            var result = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                queryCab,
                new CntxPlantillaRateDto(),
                new { codPlantilla });

            if (result.Result == null)
                return result!;

            string queryDet = @"
                SELECT *
                FROM CNTX_PLANTILLA_RATE_DETALLE
                WHERE CodPlantilla = @codPlantilla
                ORDER BY NumLinea";

            var detalle =
                DbHelper.ExecuteListQuery<CntxPlantillaRateDetalleDto>(
                    _portalDb,
                    codEmpresa,
                    queryDet,
                    new { codPlantilla });

            result.Result.Detalle =
                detalle.Result ??
                new List<CntxPlantillaRateDetalleDto>();

            return result!;
        }

        /// <summary>
        /// Guarda las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="existe"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntxPlantillaRate_Guardar(
            int codEmpresa,
            bool existe,
            CntxPlantillaRateDto request)
        {
            string usuario = request.RegistroUsuario ?? "";

            if (!existe)
            {
                const string insert = @"
                    INSERT INTO CNTX_PLANTILLA_RATE
                    (CodPlantilla, Descripcion, TipoAsiento,
                     Consecutivo, RegistroFecha, RegistroUsuario)
                    VALUES
                    (@CodPlantilla, @Descripcion, @TipoAsiento,
                     @Consecutivo, GETDATE(), @Usuario)";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    insert,
                    new
                    {
                        request.CodPlantilla,
                        request.Descripcion,
                        request.TipoAsiento,
                        request.Consecutivo,
                        Usuario = usuario
                    });

                if (resp.Code < 0)
                    return resp;
            }
            else
            {
                const string update = @"
                    UPDATE CNTX_PLANTILLA_RATE SET
                        Descripcion = @Descripcion,
                        TipoAsiento = @TipoAsiento,
                        Consecutivo = @Consecutivo,
                        ModificaFecha = GETDATE(),
                        ModificaUsuario = @Usuario
                    WHERE CodPlantilla = @CodPlantilla";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    update,
                    new
                    {
                        request.CodPlantilla,
                        request.Descripcion,
                        request.TipoAsiento,
                        request.Consecutivo,
                        Usuario = usuario
                    });

                if (resp.Code < 0)
                    return resp;

                DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    @"DELETE CNTX_PLANTILLA_RATE_DETALLE
                      WHERE CodPlantilla = @CodPlantilla",
                    new { request.CodPlantilla });
            }

            foreach (var d in request.Detalle)
            {
                const string insertDet = @"
                    INSERT INTO CNTX_PLANTILLA_RATE_DETALLE
                    (CodPlantilla, NumLinea, CodCuenta,
                     CodUnidad, CodCentroCosto, CodDivisa,
                     Detalle, Debitos, Creditos)
                    VALUES
                    (@CodPlantilla, @NumLinea, @CodCuenta,
                     @CodUnidad, @CodCentroCosto, @CodDivisa,
                     @Detalle, @Debitos, @Creditos)";

                DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    insertDet,
                    new
                    {
                        request.CodPlantilla,
                        d.NumLinea,
                        d.CodCuenta,
                        d.CodUnidad,
                        d.CodCentroCosto,
                        d.CodDivisa,
                        d.Detalle,
                        d.Debitos,
                        d.Creditos
                    });
            }

            _mSecurityMainDb.Bitacora(
                new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Movimiento = existe
                        ? "Modifica - WEB"
                        : "Registra - WEB",
                    DetalleMovimiento =
                        $"Plantilla Rate Id: {request.CodPlantilla}",
                    Modulo = vModulo
                });

            return new ErrorDto
            {
                Code = 0,
                Description =
                    "Informacion guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto CntxPlantillaRate_Eliminar(
            int codEmpresa,
            string usuario,
            int codPlantilla)
        {
            DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE CNTX_PLANTILLA_RATE_DETALLE
                  WHERE CodPlantilla = @CodPlantilla",
                new { codPlantilla });

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE CNTX_PLANTILLA_RATE
                  WHERE CodPlantilla = @CodPlantilla",
                new { codPlantilla });

            if (resp.Code < 0)
                return resp;

            _mSecurityMainDb.Bitacora(
                new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Movimiento = "Elimina - WEB",
                    DetalleMovimiento =
                        $"Plantilla Rate Id: {codPlantilla}",
                    Modulo = vModulo
                });

            return new ErrorDto
            {
                Code = 0,
                Description =
                    "Registro eliminado satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene los tipo de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento_Obtener(int codEmpresa)
        {
            var sql = @"
                SELECT
                  Tipo_Asiento AS item,
                  descripcion
                FROM CntX_Tipos_Asientos
                WHERE cod_contabilidad = 2
                ORDER BY Tipo_Asiento";

            return EjecutarDropDownQuery(codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Plantillas_Buscar(int codEmpresa)
        {
            var sql = @"
                SELECT
                  cod_plantilla AS item,
                  descripcion
                FROM CntX_Plantilla_Rate
                WHERE cod_contabilidad = @codEmpresa
                ORDER BY cod_plantilla";

            return EjecutarDropDownQuery(codEmpresa, sql, new { codEmpresa });
        }

        /// <summary>
        /// Obtiene las unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa)
        {
            var sql = @"
                SELECT
                  cod_unidad AS item,
                  descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = 2
                ORDER BY cod_unidad";

            return EjecutarDropDownQuery(codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene las divisas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa)
        {
            var sql = @"
                SELECT
                  cod_divisa AS item,
                  descripcion
                FROM CntX_Divisas
                WHERE cod_contabilidad = 2
                ORDER BY cod_divisa";

            return EjecutarDropDownQuery(codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene los centros de costos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, string codUnidad)
        {
            var sql = @"
                SELECT
                  C.cod_centro_costo AS item,
                  C.descripcion
                FROM CntX_Centro_Costos C
                INNER JOIN CntX_Unidades_CC U
                  ON C.cod_centro_costo = U.cod_centro_costo
                  AND C.cod_contabilidad = U.cod_contabilidad
                WHERE C.cod_contabilidad = @codEmpresa
                  AND U.cod_unidad = @codUnidad
                ORDER BY C.cod_centro_costo";

            return EjecutarDropDownQuery(
                codEmpresa,
                sql,
                new { codEmpresa, codUnidad });
        }
    }
}
