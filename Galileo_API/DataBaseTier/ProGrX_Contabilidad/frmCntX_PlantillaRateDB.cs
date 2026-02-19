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

        /// <summary>
        /// Metodo para scroll 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<CntxPlantillaRateDto>CntxPlantillaRate_Scroll_Obtener( int codEmpresa,int scrollCode,int? codPlantilla)
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
        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Consulta_Obtener(int codEmpresa,int codPlantilla)
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
        public ErrorDto CntxPlantillaRate_Guardar(int codEmpresa,bool existe,CntxPlantillaRateDto request)
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
        public ErrorDto CntxPlantillaRate_Eliminar(int codEmpresa,string usuario,int codPlantilla)
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
        public ErrorDto<List<DropDownListaGenericaModel>>TiposAsiento_Obtener(int codEmpresa)
        {
            var response =
                new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                sql.AppendLine("SELECT");
                sql.AppendLine("  Tipo_Asiento AS item,");
                sql.AppendLine("  descripcion");
                sql.AppendLine("FROM CntX_Tipos_Asientos");
                sql.AppendLine("WHERE cod_contabilidad = 2");
                sql.AppendLine("ORDER BY Tipo_Asiento");

                var result =
                    cn.Query<DropDownListaGenericaModel>(
                        sql.ToString(),
                        new { codEmpresa })
                    .ToList();

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

        /// <summary>
        /// Obtiene las plantillas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>Plantillas_Buscar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                sql.AppendLine("SELECT");
                sql.AppendLine("  cod_plantilla AS item,");
                sql.AppendLine("  descripcion");
                sql.AppendLine("FROM CntX_Plantilla_Rate");
                sql.AppendLine("WHERE cod_contabilidad = @codEmpresa");
                sql.AppendLine("ORDER BY cod_plantilla");

                var result = cn.Query<DropDownListaGenericaModel>(
                    sql.ToString(),
                    new { codEmpresa }
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

        /// <summary>
        /// Obtiene las unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                sql.AppendLine("SELECT");
                sql.AppendLine("  cod_unidad AS item,");
                sql.AppendLine("  descripcion");
                sql.AppendLine("FROM CntX_Unidades");
                sql.AppendLine("WHERE cod_contabilidad = 2");
                sql.AppendLine("ORDER BY cod_unidad");

                var result = cn.Query<DropDownListaGenericaModel>(
                    sql.ToString(),
                    new { codEmpresa }
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

        /// <summary>
        /// Obtiene las divisas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                sql.AppendLine("SELECT");
                sql.AppendLine("  cod_divisa AS item,");
                sql.AppendLine("  descripcion");
                sql.AppendLine("FROM CntX_Divisas");
                sql.AppendLine("WHERE cod_contabilidad = 2");
                sql.AppendLine("ORDER BY cod_divisa");

                var result = cn.Query<DropDownListaGenericaModel>(
                    sql.ToString(),
                    new { codEmpresa }
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

        /// <summary>
        /// Obtiene los centros de costos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa,string codUnidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                sql.AppendLine("SELECT");
                sql.AppendLine("  C.cod_centro_costo AS item,");
                sql.AppendLine("  C.descripcion");
                sql.AppendLine("FROM CntX_Centro_Costos C");
                sql.AppendLine("INNER JOIN CntX_Unidades_CC U");
                sql.AppendLine("  ON C.cod_centro_costo = U.cod_centro_costo");
                sql.AppendLine("  AND C.cod_contabilidad = U.cod_contabilidad");
                sql.AppendLine("WHERE C.cod_contabilidad = @codEmpresa");
                sql.AppendLine("  AND U.cod_unidad = @codUnidad");
                sql.AppendLine("ORDER BY C.cod_centro_costo");

                var result = cn.Query<DropDownListaGenericaModel>(
                    sql.ToString(),
                    new { codEmpresa, codUnidad }
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




    }
}
