using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesCopiaEsquemaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb BitacoraDb;

        public FrmTesCopiaEsquemaDB(IConfiguration config)
        {
            BitacoraDb = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// 'OBJETIVO:      Despliega en pantalla datos principales del # solicitud suministrado por el
        /// '               usuario.
        /// 'REFERENCIAS:   fxDescribeBanco - (Devuelve la descripcion del Banco al que se giro la
        /// '               solicitud)
        /// 'OBSERVACIONES: Ninguna.
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesCopiaEsquemaModels> Tes_CopiaEsquema_Obtener(int CodEmpresa, int solicitud, int contabilidad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = $@"select C.codigo,C.Beneficiario,C.Monto,C.Fecha_Solicitud,C.Tipo,C.Id_Banco
                                   ,C.cod_unidad,C.cod_concepto,U.descripcion as UnidadDesc,X.descripcion as ConceptoDesc
                                   ,T.descripcion as TDocumento,B.descripcion as BancoDesc,
                                   C.CTA_AHORROS as cuentaIBAN , C.CTA_IBAN_ORIGEN as cuentaOrigen, 
                                   C.CORREO_NOTIFICA as correo , C.COD_DIVISA as divisa
                                   , C.TIPO_CED_ORIGEN as tipoId, C.detalle1, C.detalle2, C.detalle3, C.detalle4, C.detalle5
                                    from Tes_Transacciones C inner join CntX_unidades U on C.cod_unidad = U.cod_unidad and cod_Contabilidad = @contabilidad
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_conceptos X on C.cod_concepto = X.cod_concepto
                                    inner join Tes_Bancos B on C.id_banco = B.id_banco
                                    where C.nsolicitud = @solicitud ";

                var response = conn.Query<TesCopiaEsquemaModels>(
                     query,
                     new
                     {
                         solicitud = solicitud,
                         contabilidad = contabilidad
                     }).FirstOrDefault();

                if (response != null)
                {
                    response.detalle = string.Join(" ",
                                                response.detalle1 ?? "",
                                                response.detalle2 ?? "",
                                                response.detalle3 ?? "",
                                                response.detalle4 ?? "",
                                                response.detalle5 ?? ""
                                            ).Replace("null", "").Trim();
                }

                if (response != null)
                {
                    response.solicitud = solicitud;
                }

                return DbHelper.CreateOkResponse(response!);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<TesCopiaEsquemaModels>(ex.Message);
            }
        }
    
        /// <summary>
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Duplica una determinada solicitud ya ingresada a Tesoreria. Tambien duplica
        ///'               el detalle de la misma solicitud para la nueva.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               sbLimpiaDatos - (Limpia los objetos de entrada de datos)
        ///'               fxValidaSolicitud - (Valida que la Solicitud por duplicar contenga
        ///'               identificador de Banco y codigo)
        ///'               fxFechaServidor - (Devuelve la fecha del servidor)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto Tes_CopiarEsquema_Guardar(int CodEmpresa, TesCopiaEsquemaModels solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"exec spTES_Transaccion_Copia @TesoreriaId, @Notas,  @Usuario";
                var tesoleria = conn.Query<int>(query, new {
                    TesoreriaId = solicitud.solicitud,
                    Notas = solicitud.notas,
                    Usuario = solicitud.usuario
                }).FirstOrDefault();

                if (tesoleria > 0)
                {
                    BitacoraDb.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = solicitud.usuario.ToUpper(),
                        DetalleMovimiento = "Solicitud de Copia de Esquema, Solicitud: " + solicitud.solicitud + " A la Sol : " + tesoleria,
                        Movimiento = "Aplica",
                        Modulo = 9
                    });

                    return DbHelper.OkResponse(tesoleria.ToString());
                }
                else
                {
                    return DbHelper.ErrorResponse("No fue posible realizar la Copia de la Solicitud!");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método que obtiene una lista de solicitudes de copia de esquema de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesCopiaEsquemaLista> Tes_CopiaEsquemaLista_Obtener(int CodEmpresa, int contabilidad, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesCopiaEsquemaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesCopiaEsquemaLista
                {
                    total = 0,
                    lista = new List<TesCopiaEsquemaModels>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
                var fetch = filtros.paginacion <= 0 ? 50 : filtros.paginacion;

                // Mantengo tu lógica original: -1 => ASC, else => DESC
                var sortDir = (filtros.sortOrder == -1) ? "ASC" : "DESC";

                // Enviar el sortField como parámetro (no se usa como identificador, solo para CASE)
                var sortField = (filtros.sortField ?? "NSOLICITUD").Trim();

                const string sqlCount = @"
SELECT COUNT(1)
FROM Tes_Transacciones C
INNER JOIN CntX_unidades U ON C.cod_unidad = U.cod_unidad AND U.cod_Contabilidad = @contabilidad
INNER JOIN tes_tipos_doc T ON C.tipo = T.tipo
INNER JOIN tes_conceptos X ON C.cod_concepto = X.cod_concepto
INNER JOIN Tes_Bancos B ON C.id_banco = B.id_banco
WHERE
    (@filtro IS NULL)
 OR (CAST(C.nsolicitud AS NVARCHAR(50)) LIKE @like)
 OR (C.tipo LIKE @like)
 OR (C.beneficiario LIKE @like)
 OR (C.codigo LIKE @like);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    contabilidad,
                    filtro = hasFiltro ? texto : null,
                    like
                });

                const string sqlList = @"
SELECT
    NSOLICITUD, CODIGO, BENEFICIARIO, MONTO, Fecha_Solicitud, TIPO, Id_Banco, COD_UNIDAD, cod_concepto,
    UnidadDesc, ConceptoDesc, TDocumento, BancoDesc
FROM (
    SELECT
        C.nsolicitud       AS NSOLICITUD,
        C.codigo           AS CODIGO,
        C.beneficiario     AS BENEFICIARIO,
        C.monto            AS MONTO,
        C.fecha_solicitud  AS Fecha_Solicitud,
        C.tipo             AS TIPO,
        C.id_banco         AS Id_Banco,
        C.cod_unidad       AS COD_UNIDAD,
        C.cod_concepto     AS cod_concepto,
        U.descripcion      AS UnidadDesc,
        X.descripcion      AS ConceptoDesc,
        T.descripcion      AS TDocumento,
        B.descripcion      AS BancoDesc
    FROM Tes_Transacciones C
    INNER JOIN CntX_unidades U ON C.cod_unidad = U.cod_unidad AND U.cod_Contabilidad = @contabilidad
    INNER JOIN tes_tipos_doc T ON C.tipo = T.tipo
    INNER JOIN tes_conceptos X ON C.cod_concepto = X.cod_concepto
    INNER JOIN Tes_Bancos B ON C.id_banco = B.id_banco
) X
WHERE
    (@filtro IS NULL)
 OR (CAST(NSOLICITUD AS NVARCHAR(50)) LIKE @like)
 OR (TIPO LIKE @like)
 OR (BENEFICIARIO LIKE @like)
 OR (CODIGO LIKE @like)
ORDER BY
    -- ASC
    CASE WHEN @SortDir = 'ASC' THEN
        CASE @SortField
            WHEN 'NSOLICITUD'      THEN CONVERT(sql_variant, NSOLICITUD)
            WHEN 'CODIGO'          THEN CONVERT(sql_variant, CODIGO)
            WHEN 'BENEFICIARIO'    THEN CONVERT(sql_variant, BENEFICIARIO)
            WHEN 'MONTO'           THEN CONVERT(sql_variant, MONTO)
            WHEN 'Fecha_Solicitud' THEN CONVERT(sql_variant, Fecha_Solicitud)
            WHEN 'TIPO'            THEN CONVERT(sql_variant, TIPO)
            WHEN 'Id_Banco'        THEN CONVERT(sql_variant, Id_Banco)
            WHEN 'COD_UNIDAD'      THEN CONVERT(sql_variant, COD_UNIDAD)
            WHEN 'cod_concepto'    THEN CONVERT(sql_variant, cod_concepto)
            WHEN 'UnidadDesc'      THEN CONVERT(sql_variant, UnidadDesc)
            WHEN 'ConceptoDesc'    THEN CONVERT(sql_variant, ConceptoDesc)
            WHEN 'TDocumento'      THEN CONVERT(sql_variant, TDocumento)
            WHEN 'BancoDesc'       THEN CONVERT(sql_variant, BancoDesc)
            ELSE CONVERT(sql_variant, NSOLICITUD)
        END
    END ASC,

    -- DESC
    CASE WHEN @SortDir = 'DESC' THEN
        CASE @SortField
            WHEN 'NSOLICITUD'      THEN CONVERT(sql_variant, NSOLICITUD)
            WHEN 'CODIGO'          THEN CONVERT(sql_variant, CODIGO)
            WHEN 'BENEFICIARIO'    THEN CONVERT(sql_variant, BENEFICIARIO)
            WHEN 'MONTO'           THEN CONVERT(sql_variant, MONTO)
            WHEN 'Fecha_Solicitud' THEN CONVERT(sql_variant, Fecha_Solicitud)
            WHEN 'TIPO'            THEN CONVERT(sql_variant, TIPO)
            WHEN 'Id_Banco'        THEN CONVERT(sql_variant, Id_Banco)
            WHEN 'COD_UNIDAD'      THEN CONVERT(sql_variant, COD_UNIDAD)
            WHEN 'cod_concepto'    THEN CONVERT(sql_variant, cod_concepto)
            WHEN 'UnidadDesc'      THEN CONVERT(sql_variant, UnidadDesc)
            WHEN 'ConceptoDesc'    THEN CONVERT(sql_variant, ConceptoDesc)
            WHEN 'TDocumento'      THEN CONVERT(sql_variant, TDocumento)
            WHEN 'BancoDesc'       THEN CONVERT(sql_variant, BancoDesc)
            ELSE CONVERT(sql_variant, NSOLICITUD)
        END
    END DESC,

    -- desempate estable
    NSOLICITUD ASC
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = conn.Query<TesCopiaEsquemaModels>(sqlList, new
                {
                    contabilidad,
                    filtro = hasFiltro ? texto : null,
                    like,
                    SortField = sortField,
                    SortDir = sortDir,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesCopiaEsquemaModels>();
            }

            return result;
        }

    }
}
