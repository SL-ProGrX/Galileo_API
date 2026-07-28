using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.ReportingServices.Diagnostics.Internal;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesReclasificacionDB
    {

        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;

        public FrmTesReclasificacionDB(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para obtener los bancos activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReclasificacionBancos_Obtener(int CodEmpresa,string usuario,string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        /// <summary>
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Despliega en pantalla los datos pertinentes a la solicitud digitada por el
        ///'               usuario.
        ///'REFERENCIAS:   LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        ///'               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesReclasificacionDto> TES_Reclasificacion_Obtener(int CodEmpresa, int solicitud)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"Select T.*,B.Descripcion as 'BancoDesc',B.CtaConta as 'BancoCta', Td.descripcion as 'TipoDesc'
                                        from Tes_Transacciones T 
                                        inner join Tes_Bancos B on T.id_Banco = B.id_Banco
                                        inner join tes_tipos_doc Td on T.Tipo = Td.Tipo
                                        Where T.Nsolicitud= @solicitud ";

                return conn.Query<TesReclasificacionDto>(query, new
                {
                    solicitud = solicitud
                }).FirstOrDefault() ?? new TesReclasificacionDto();
            });
        }

        /// <summary>
        /// Método para obtener la cuenta contable del banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<string> TES_Reclasificacion_CuentaBanco(int CodEmpresa, int id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select ctaconta as Cuenta from Tes_Bancos where id_banco = @id_banco ";

                return conn.Query<string>(query,
                new
                {
                    id_banco = id_banco
                }).FirstOrDefault() ?? string.Empty;
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocsCargaCboAcceso_Obtener(int CodEmpresa, string usuario, int id_banco, string tipo)
        {
            return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, id_banco, tipo);
        }

        /// <summary>
        /// Método para cambiar el banco de la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto TES_Reclasificacion_CambiaBanco(int CodEmpresa, TesReclasificaBancoModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select estado_asiento from Tes_Transacciones where nsolicitud = @nsolicitud";
                var estado = conn.Query<string>(query,
                    new
                    {
                        nsolicitud = data.nsolicitud
                    }).FirstOrDefault();
                if (estado == "G")
                {
                    return DbHelper.ErrorResponse("El asiento de esta solicitud ya fue generado, no se puede reclasificar...");
                }

                data.bancoDestino = data.bancoDestino.Trim();

                query = $@"exec spTes_Reclasificacion @Nsolicitud, @bancoDestino, @tipo, @usuario,@nota ";

                conn.ExecuteAsync(query,
                    new
                    {
                        Nsolicitud = data.nsolicitud,
                        bancoDestino = data.bancoDestino,
                        tipo = data.tipo,
                        usuario = data.usuario,
                        nota = data.nota
                    });

                _Security_MainDB.Bitacora
                    (new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = data.usuario,
                        DetalleMovimiento = $"Solicitud {data.nsolicitud} reclasificada a Banco {data.bancoDestino}",
                        Movimiento = "RECLASIFICACION - WEB",
                        Modulo = vModulo
                    });

                return DbHelper.OkResponse("Cambio de Banco Realizado Satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Modifica la solicitud en cuanto al # de Documento.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        /// '               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto TES_Reclasificacion_CambiaDocumento(int CodEmpresa, TesReclasificaDocumentoModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select estado_asiento from Tes_Transacciones where nsolicitud = @nsolicitud";
                var estado = conn.Query<string>(query,
                    new
                    {
                        nsolicitud = data.nsolicitud
                    }).FirstOrDefault();
                if (estado == "G")
                {
                    return DbHelper.ErrorResponse("El asiento de esta solicitud ya fue generado, no se puede reclasificar...");
                }

                // Verifico si el # Documento anterior
                query = $@"select Ndocumento from Tes_Transacciones 
                               where nsolicitud = @nsolicitud And Tipo = @tipo and id_banco= @id_banco";
                var ndocumentoAnterior = conn.Query<string>(query,
                    new
                    {
                        nsolicitud = data.nsolicitud,
                        tipo = data.tipo,
                        id_banco = data.id_banco
                    }).FirstOrDefault();

                query = $@"Select Nsolicitud from Tes_Transacciones where id_banco= @id_banco
                                      And Tipo = @tipo  and Ndocumento = @ndocumento ";
                var solicitud = conn.Query<int>(query,
                    new
                    {
                        id_banco = data.id_banco,
                        tipo = data.tipo,
                        ndocumento = data.ndocumento
                    }).FirstOrDefault();

                if (solicitud != 0)
                {
                    return DbHelper.ErrorResponse("# Documento Ya Existe, No Se Puede Reclasificar");
                }

                query = $@"Update Tes_Transacciones Set Ndocumento = @ndocumento Where NSolicitud = @solicitud ";
                conn.ExecuteAsync(query,
                    new
                    {
                        ndocumento = data.ndocumento,
                        solicitud = data.nsolicitud
                    });

                string bitacora = $"Cambio N.Documento de {ndocumentoAnterior} a {data.ndocumento}";
                mTesoreria.sbTesBitacoraEspecial(CodEmpresa, data.nsolicitud, "09", bitacora, data.usuario);

                _Security_MainDB.Bitacora
                    (new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = data.usuario,
                        DetalleMovimiento = $"Solicitud {data.nsolicitud} reclasificada a Documento {data.ndocumento}",
                        Movimiento = "RECLASIFICACION - WEB",
                        Modulo = vModulo
                    });

                return DbHelper.OkResponse("El documento ha sido reclasificado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Reclasifica la solicitud en cuanto al Banco y Tipo de Documento. Ademas
        ///'               actualiza para el detalle de la solicitud el # Cuenta del Banco.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        ///'               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ErrorDto> TES_Reclasificacion_CambiaSolicitud(int CodEmpresa, TesReclasificaSolicitudModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string query = @"select estado_asiento, id_banco, tipo 
                               from Tes_Transacciones 
                               where nsolicitud = @nsolicitud";

                var estado = (await conn.QueryAsync<dynamic>(query, new
                {
                    nsolicitud = data.nsolicitud
                })).FirstOrDefault();

                if (estado == null)
                {
                    return DbHelper.ErrorResponse("No se encontró la solicitud indicada.");
                }

                if ((estado.estado_asiento?.ToString() ?? string.Empty) == "G")
                {
                    return DbHelper.ErrorResponse("El asiento de esta solicitud ya fue generado, no se puede reclasificar...");
                }

                if (!data.permiteReqId)
                {
                    data.tipoId = -1;
                }

                var parametros = new
                {
                    TesoreriaId = data.nsolicitud,
                    BancoId = data.id_banco,
                    Tipo = data.tipo,
                    Usuario = data.usuario,
                    Notas = data.nota,
                    tipoId = data.tipoId,
                    Cedula = data.cedula ?? string.Empty,
                    cedulaValida = data.cedulaValida ? 1 : 0,
                    CuentaIban = data.cuentaIban ?? string.Empty,
                    cuentaIbanValida = data.cuentaIbanValida ? 1 : 0,
                    Email = data.email ?? string.Empty,
                    emailValido = data.emailValido ? 1 : 0
                };

                var result = await conn.QueryAsync<dynamic>(
                    "spTES_W_Reclasificacion",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                var respuesta = result.FirstOrDefault();

                if (respuesta == null)
                {
                    return DbHelper.ErrorResponse("El procedimiento no retornó respuesta.");
                }

                var exitoso = Convert.ToBoolean(respuesta.exitoso ?? false);
                var mensaje = respuesta.mensaje?.ToString() ?? "No se recibió mensaje del procedimiento.";
                var paso = respuesta.paso?.ToString() ?? string.Empty;

                if (!exitoso)
                {
                    var detalleError = string.IsNullOrWhiteSpace(paso)
                        ? mensaje
                        : $"{mensaje} Paso: {paso}";

                    return DbHelper.ErrorResponse(detalleError);
                }

                var bitacora =
                    $"Solicitud {data.nsolicitud} reclasificada a Banco {data.id_banco}, Tipo {data.tipo} y Cod_ID {data.tipoId}";

                _Security_MainDB.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = data.usuario,
                        DetalleMovimiento = bitacora,
                        Movimiento = "RECLASIFICACION - WEB",
                        Modulo = vModulo
                    });

                return DbHelper.OkResponse(mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para obtener la lista de solicitudes de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<TesSolicitudesData>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = (filtros.pagina < 0) ? 0 : filtros.pagina;
                var fetch = (filtros.paginacion <= 0) ? 50 : filtros.paginacion;

                // Mantengo tu convención del ejemplo:
                // -1 => ASC, else => DESC
                var sortDir = (filtros.sortOrder == -1) ? "ASC" : "DESC";

                // Ojo: NO se usa como identificador directo; solo para CASE (evita S2077)
                var sortField = (filtros.sortField ?? "NSOLICITUD").Trim();

                const string sqlCount = @"
SELECT COUNT(1)
FROM Tes_Transacciones C
INNER JOIN Tes_Tipos_doc T ON C.tipo = T.tipo
WHERE
    (@filtro IS NULL)
 OR (CAST(C.NSOLICITUD AS NVARCHAR(50)) LIKE @like)
 OR (C.BENEFICIARIO LIKE @like)
 OR (T.descripcion LIKE @like)
 OR (C.CODIGO LIKE @like);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                // Nota: el subquery es para poder ordenar/filtrar por alias (tipo) sin repetir joins/expresiones.
                const string sqlList = @"
SELECT
    NSOLICITUD, tipo, CODIGO, BENEFICIARIO, monto, estado, COD_UNIDAD
FROM (
    SELECT
        C.NSOLICITUD                 AS NSOLICITUD,
        RTRIM(T.descripcion)         AS tipo,
        C.CODIGO                     AS CODIGO,
        C.BENEFICIARIO               AS BENEFICIARIO,
        C.monto                      AS monto,
        C.estado                     AS estado,
        C.COD_UNIDAD                 AS COD_UNIDAD
    FROM Tes_Transacciones C
    INNER JOIN Tes_Tipos_doc T ON C.tipo = T.tipo
) X
WHERE
    (@filtro IS NULL)
 OR (CAST(NSOLICITUD AS NVARCHAR(50)) LIKE @like)
 OR (BENEFICIARIO LIKE @like)
 OR (tipo LIKE @like)
 OR (CODIGO LIKE @like)
ORDER BY
    -- ASC
    CASE WHEN @SortDir = 'ASC' THEN
        CASE @SortField
            WHEN 'NSOLICITUD'   THEN CONVERT(sql_variant, NSOLICITUD)
            WHEN 'tipo'        THEN CONVERT(sql_variant, tipo)
            WHEN 'CODIGO'      THEN CONVERT(sql_variant, CODIGO)
            WHEN 'BENEFICIARIO'THEN CONVERT(sql_variant, BENEFICIARIO)
            WHEN 'monto'       THEN CONVERT(sql_variant, monto)
            WHEN 'estado'      THEN CONVERT(sql_variant, estado)
            WHEN 'COD_UNIDAD'  THEN CONVERT(sql_variant, COD_UNIDAD)
            ELSE CONVERT(sql_variant, NSOLICITUD)
        END
    END ASC,

    -- DESC
    CASE WHEN @SortDir = 'DESC' THEN
        CASE @SortField
            WHEN 'NSOLICITUD'   THEN CONVERT(sql_variant, NSOLICITUD)
            WHEN 'tipo'        THEN CONVERT(sql_variant, tipo)
            WHEN 'CODIGO'      THEN CONVERT(sql_variant, CODIGO)
            WHEN 'BENEFICIARIO'THEN CONVERT(sql_variant, BENEFICIARIO)
            WHEN 'monto'       THEN CONVERT(sql_variant, monto)
            WHEN 'estado'      THEN CONVERT(sql_variant, estado)
            WHEN 'COD_UNIDAD'  THEN CONVERT(sql_variant, COD_UNIDAD)
            ELSE CONVERT(sql_variant, NSOLICITUD)
        END
    END DESC,

    -- desempate estable
    NSOLICITUD ASC
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = conn.Query<TesSolicitudesData>(sqlList, new
                {
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
                result.Result.lista = new List<TesSolicitudesData>();
            }

            return result;
        }

        /// <summary>
        /// Método para obtener los tipos de identificación
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _AuxiliarDB.TiposIdentificacion_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Método para validar si el id de la cuenta se puede cambiar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<bool> Tes_ReclasificaId_Valida(int CodEmpresa, string? tipo)
        {
            if (tipo == null)
            {
                return DbHelper.CreateErrorResponse<bool>("Tipo no puede ser nulo", 0, false);
            }

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select ISNULL(INT_RECLASIFICA_ID, 0) from TES_TIPOS_DOC where TIPO = @Tipo ";

                return conn.Query<bool>(query, new
                {
                    Tipo = tipo
                }).FirstOrDefault() ;
            });
        }
    }
}
