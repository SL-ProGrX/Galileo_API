using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransferenciaReversaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;
        private readonly int module = 9;
        private readonly MSecurityMainDb _mSecurity;

        public FrmTesTransferenciaReversaDB(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            _mSecurity = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

     
        public ErrorDto<long> sbNTrasnferencia(int CodEmpresa, int id_banco, string tipo, string avance, string plan)
        {
            return mTesoreria.fxTesTipoDocConsec(CodEmpresa, id_banco, tipo, avance, plan);
        }

        
       
        /// <summary>
        /// Carga el combo de acceso a la gestión de transferencias bancarias.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="gestion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        #region Reversa Transferencia
        /// <summary>
        /// Obtiene las solicitudes de transferencia reversa según los criterios especificados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaReversa_Obtener(
    int CodEmpresa,
    TransferenciaSolicitudData solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var request = solicitud ?? new TransferenciaSolicitudData();
                var filtros = TES_TransferenciaReversa_NormalizarFiltrosObtener(request);

                if (string.IsNullOrWhiteSpace(filtros.Documento))
                {
                    return DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>(
                        "Debe indicar el documento base.");
                }

                const string query = @"
                        SELECT
                            nsolicitud,
                            codigo,
                            beneficiario,
                            monto,
                            fecha_emision,
                            cta_ahorros,
                            Ndocumento
                        FROM Tes_Transacciones
                        WHERE
                            TRIM(documento_base) = @documento
                            AND id_banco = @id_banco

                            AND (@Codigo IS NULL OR Codigo LIKE @CodigoLike)
                            AND (@Ndocumento IS NULL OR Ndocumento LIKE @NdocumentoLike)
                            AND (@Beneficiario IS NULL OR Beneficiario LIKE @BeneficiarioLike)
                            AND (@CtaAhorros IS NULL OR Cta_Ahorros LIKE @CtaAhorrosLike)

                            -- Mantengo tu lógica: si no viene cod_plan, usar '-sp-' y comparar con ISNULL(cod_Plan,'-sp-')
                            AND (ISNULL(cod_Plan,'-sp-') = @CodPlan)
                            AND REFERENCIA_SINPE IS NULL;";

                var parameters = TES_TransferenciaReversa_CrearParametrosObtener(request, filtros);

                var response = conn.Query<TransferenciaSolicitudData>(query, parameters).ToList();

                return response.Count == 0
                    ? DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>(
                        "No se encontraron datos para la solicitud especificada.")
                    : DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>(ex.Message);
            }
        }

        private static object TES_TransferenciaReversa_CrearParametrosObtener(
    TransferenciaSolicitudData solicitud,
    TesTransferenciaReversaFiltrosObtener filtros)
        {
            return new
            {
                documento = filtros.Documento,
                id_banco = solicitud.id_banco,

                Codigo = filtros.Codigo,
                CodigoLike = TES_TransferenciaReversa_CrearLike(filtros.Codigo),

                Ndocumento = filtros.Ndocumento,
                NdocumentoLike = TES_TransferenciaReversa_CrearLike(filtros.Ndocumento),

                Beneficiario = filtros.Beneficiario,
                BeneficiarioLike = TES_TransferenciaReversa_CrearLike(filtros.Beneficiario),

                CtaAhorros = filtros.CtaAhorros,
                CtaAhorrosLike = TES_TransferenciaReversa_CrearLike(filtros.CtaAhorros),

                CodPlan = filtros.CodPlan
            };
        }

        private static TesTransferenciaReversaFiltrosObtener TES_TransferenciaReversa_NormalizarFiltrosObtener(
            TransferenciaSolicitudData solicitud)
        {
            return new TesTransferenciaReversaFiltrosObtener
            {
                Documento = solicitud.documento!.Trim(),
                Codigo = TES_TransferenciaReversa_NormalizarTexto(solicitud.codigo!),
                Ndocumento = TES_TransferenciaReversa_NormalizarTexto(solicitud.ndocumento!),
                Beneficiario = TES_TransferenciaReversa_NormalizarTexto(solicitud.beneficiario!),
                CtaAhorros = TES_TransferenciaReversa_NormalizarTexto(solicitud.cta_ahorros!),
                CodPlan = TES_TransferenciaReversa_NormalizarTexto(solicitud.cod_plan!) ?? "-sp-"
            };
        }

        private static string TES_TransferenciaReversa_NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }

        private static string TES_TransferenciaReversa_CrearLike(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : $"%{valor}%";
        }

       

        /// <summary>
        /// Obtiene los planes de banco disponibles para la reversa de transferencias.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReversaPlanes_Obtener(int CodEmpresa, string id_banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select Bp.COD_PLAN as 'item', Bp.COD_PLAN as 'descripcion'
                                    from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
                                    Where B.ID_BANCO = @id_banco And B.UTILIZA_PLAN = 1
                                    order by Bp.COD_PLAN  asc";
                var result = conn.Query<DropDownListaGenericaModel>(query,
                    new { id_banco }).ToList();

                result.Add(new DropDownListaGenericaModel
                {
                    item = "-sp-",
                    descripcion = "Sin Plan"
                });

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Aplica una reversa a una transferencia existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="transferencia"></param>
        /// <returns></returns>
        public ErrorDto TES_TransferenciaReversa_Aplicar(int CodEmpresa, TransferenciaReversaAplicaModel transferencia)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = "select valor   from TES_PARAMETROS where COD_PARAMETRO = '11'";
                var vDias = conn.QueryFirstOrDefault<int>(query);
                if (vDias == 0)
                {
                    vDias = 5; // Valor por defecto si no se encuentra en la base de datos
                }

                //calculo los días entre dos fechas
                if (transferencia.lista == null || transferencia.lista.Count == 0)
                {
                    return DbHelper.ErrorResponse("No existen registros para calcular la fecha.");
                }

                var ultimo = transferencia.lista[^1]; // <- S6608 solucionada

                if (ultimo.fecha == null)
                {
                    return DbHelper.ErrorResponse("La fecha del último registro es inválida.");
                }

                DateTime fecha1 = ultimo.fecha.Value;
                DateTime fecha2 = DateTime.Now;

                double dias = (fecha2.Date - fecha1.Date).TotalDays;

                if (dias > vDias)
                {
                    return DbHelper.ErrorResponse($"Esta intentando reversar una transferencia con mas de {vDias}  días de emisión: {MProGrXAuxiliarDB.validaFechaGlobal(fecha1, "yyyy-MM-dd HH:mm:ss")}", -2);
                }

                query = $@"Select * From Tes_Autorizaciones Where Clave= @clave and nombre = @usuario and estado = 'A'";
                var autorizacion = conn.QueryFirstOrDefault<TesAutorizacionesDto>(query,
                    new { transferencia.clave, transferencia.usuario });

                if (autorizacion == null)
                {
                    return DbHelper.ErrorResponse("Contraseña Incorrecta, o no Existe Nivel de Autorización", -1);
                }

                query = $@"select count(*) as Existe from tes_te_reversion where isnull(Tipo,'T') = 'T'
                                    and id_Banco = @id_banco and Documento = @documento ";
                var existe = conn.QueryFirstOrDefault<int>(query,
                    new { transferencia.id_banco, documento = transferencia.ndocumento!.Trim() });

                if (existe == 1)
                {
                    return DbHelper.ErrorResponse($"La transferencia No.{transferencia.ndocumento}, ya fue reversada anteriormente!", -2);
                }

                query = $@"exec spTES_TE_Reversion_Main @id_banco, @tipo, @documento, @observaciones, @usuario ";
                var ReversionId = conn.QueryFirstOrDefault<int>(query,
                    new
                    {
                        transferencia.id_banco,
                        transferencia.tipo,
                        documento = transferencia.ndocumento.Trim(),
                        observaciones = transferencia.observaciones ?? string.Empty,
                        usuario = transferencia.usuario!.ToUpper()
                    });

                if (transferencia.lista.Count > 0)
                {
                    foreach (var item in transferencia.lista)
                    {
                        query = $@"EXEC spTES_TE_Reversion_Transaccion @iConsecutivo, @item, @usuario ";
                        conn.Execute(query,
                        new
                        {
                            iConsecutivo = ReversionId,
                            item = item.nsolicitud,
                            usuario = transferencia.usuario.ToUpper()
                        });

                    }
                }

                //bitacora
                _mSecurity.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = transferencia.usuario,
                    Modulo = module, // Tesoreria
                    Movimiento = "Aplica",
                    DetalleMovimiento = "Reversion Transferencia = " + ReversionId + " Id.Cuenta:" + transferencia.id_banco + ", Tipo: " + transferencia.tipo,
                });

                return DbHelper.OkResponse(ReversionId.ToString());
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Consulta Reversa Transferencia
        /// <summary>
        /// Metodo para obtener las reversas de transferencias según los criterios especificados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public ErrorDto<List<TesReversionData>> TES_TransferenciaConsulta_Obtener(
    int CodEmpresa,
    int id_banco,
    DateTime fechaInicio,
    DateTime fechaFin)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                // Rango inclusivo: [00:00:00, 23:59:59.9999999]
                var ini = fechaInicio.Date;
                var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

                const string query = @"
                            SELECT *
                            FROM tes_te_reversion
                            WHERE id_banco = @id_banco
                              AND fecha_genera BETWEEN @ini AND @fin;";

                var result = conn.Query<TesReversionData>(query, new
                {
                    id_banco,
                    ini,
                    fin
                }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesReversionData>>(ex.Message);
            }
        }


        /// <summary>
        /// Obtiene los detalles de una reversa de transferencia específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_reversion"></param>
        /// <returns></returns>
        public ErrorDto<List<TransferenciaDetalleModel>> TES_TransferenciaReversa_Detalle(int CodEmpresa, string id_reversion)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select * from vTes_TE_Reversion_Det where id_reversion = @id_reversion ";

                return conn.Query<TransferenciaDetalleModel>(query, new { id_reversion }).ToList();
            });
        }

        #endregion

        #region Transferencia SINPE
        /// <summary>
        /// Metodo para obtener las transferencias SINPE para reversa según los criterios especificados.
        /// </summary>
        /// <param name="reversa"></param>
        /// <returns></returns>
        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaRevSinpe_Obtener(TesReversaSinpeRequest reversa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, reversa.codEmpresa);
            try
            {
                var solicitud = string.Empty;
                var nDocumento = string.Empty;
                // Normalizo textos
                if (reversa.tipoDocumento == "S")
                {
                    solicitud = (reversa.documento == " 0") ? null : reversa.documento?.Trim();
                }
                else
                {
                    nDocumento = (reversa.documento == " 0") ? null : reversa.documento?.Trim();
                }

                    

              
                const string query = @"
                        SELECT
                                nsolicitud,
                                codigo,
                                beneficiario,
                                monto,
                                fecha_emision,
                                cta_ahorros,
                                Ndocumento
                            FROM Tes_Transacciones
                            WHERE
                                id_banco = @id_banco
                                AND (@Ndocumento IS NULL OR Ndocumento LIKE @NdocumentoLike)
                                AND (@NSolicitud IS NULL OR NSOLICITUD LIKE @NSolicitudLike)
                                AND REFERENCIA_SINPE IN (
    	                            SELECT COD_REFERENCIA FROM SINPE_MOV_TRANSITO  where ESTADO = 4
                                )";

                var parameters = new
                {
                    id_banco = reversa.id_banco,

                    Ndocumento = string.IsNullOrWhiteSpace(nDocumento) ? null : nDocumento,
                    NdocumentoLike = string.IsNullOrWhiteSpace(nDocumento) ? null : $"%{nDocumento}%",

                    NSolicitud = string.IsNullOrWhiteSpace(solicitud) ? null : solicitud,
                    NSolicitudLike = string.IsNullOrWhiteSpace(solicitud) ? null : $"%{solicitud}%",
                };

                var response = conn.Query<TransferenciaSolicitudData>(query, parameters).ToList();

                if (response.Count == 0)
                    return DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>(
                        "No se encontraron datos para la solicitud especificada.");

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>(ex.Message);
            }
        }

        /// <summary>
        /// Metodo para aplicar la reversa de una transferencia SINPE.
        /// </summary>
        /// <param name="reversa"></param>
        /// <returns></returns>
        public ErrorDto TES_TransferenciaRevSinpe_Aplicar(TesReversaSinpeModel reversa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, reversa.codEmpresa);

            try
            {
                var validacion = TES_TransferenciaRevSinpe_ValidarEntrada(reversa);
                if (validacion.Code != 0)
                    return validacion;

                var autorizacion = TES_TransferenciaRevSinpe_ValidarAutorizacion(conn, reversa);
                if (autorizacion.Code != 0)
                    return autorizacion;

                var existeReversion = TES_TransferenciaRevSinpe_ValidaReversionExistente(conn, reversa);
                if (existeReversion.Code != 0)
                    return existeReversion;

                var reversionId = TES_TransferenciaRevSinpe_CrearReversion(conn, reversa);
                if (reversionId.Code != 0)
                    return reversionId;

                var procesa = TES_TransferenciaRevSinpe_ProcesarSolicitudes(conn, reversa);
                if (procesa.Code != 0)
                    return procesa;

                TES_TransferenciaRevSinpe_RegistrarBitacora(reversa, reversionId.Result);

                return DbHelper.OkResponse(reversionId.Result.ToString());
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida la información mínima requerida para aplicar la reversa SINPE.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_ValidarEntrada(TesReversaSinpeModel reversa)
        {
            if (reversa.lista == null || reversa.lista.Count == 0)
                return DbHelper.ErrorResponse("No existen registros para reversar.");

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Valida la clave y el nivel de autorización del usuario para reversar transferencias SINPE.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_ValidarAutorizacion(
            SqlConnection conn,
            TesReversaSinpeModel reversa)
        {
            const string query = @"
Select *
From Tes_Autorizaciones
Where Clave = @clave
  and nombre = @usuario
  and estado = 'A'";

            var autorizacion = conn.QueryFirstOrDefault<TesAutorizacionesDto>(
                query,
                new { reversa.clave, reversa.usuario });

            if (autorizacion == null)
            {
                return DbHelper.ErrorResponse(
                    "Contraseña Incorrecta, o no Existe Nivel de Autorización",
                    -1);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Valida si la transferencia ya fue reversada anteriormente.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_ValidaReversionExistente(
            SqlConnection conn,
            TesReversaSinpeModel reversa)
        {
            const string query = @"
select count(*) as Existe
from TES_SINPE_REVERSA
where id_Banco = @id_banco
  and NDOCUMENTO = @documento";

            var existe = conn.QueryFirstOrDefault<int>(
                query,
                new
                {
                    reversa.id_banco,
                    documento = reversa.documento!.Trim()
                });

            if (existe == 1)
            {
                return DbHelper.ErrorResponse(
                    $"La transferencia No.{reversa.documento}, ya fue reversada anteriormente!",
                    -2);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Registra el encabezado principal de la reversa SINPE.
        /// </summary>
        private ErrorDto<int> TES_TransferenciaRevSinpe_CrearReversion(
            SqlConnection conn,
            TesReversaSinpeModel reversa)
        {
            const string query = @"
exec spTES_TE_Reversion_Main
    @id_banco,
    @tipo,
    @documento,
    @observaciones,
    @usuario";

            var reversionId = conn.QueryFirstOrDefault<int>(
                query,
                new
                {
                    reversa.id_banco,
                    tipo = "TS",
                    documento = reversa.documento!.Trim(),
                    observaciones = reversa.observaciones ?? string.Empty,
                    usuario = reversa.usuario!.ToUpper()
                });

            return DbHelper.CreateOkResponse(reversionId);
        }

        /// <summary>
        /// Procesa cada solicitud incluida en la reversa SINPE.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_ProcesarSolicitudes(
            SqlConnection conn,
            TesReversaSinpeModel reversa)
        {
            foreach (var item in reversa.lista!)
            {
                var procesaItem = TES_TransferenciaRevSinpe_ProcesarSolicitud(conn, item);
                if (procesaItem.Code != 0)
                    return procesaItem;
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Procesa la reversa SINPE de una solicitud individual.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_ProcesarSolicitud(
            SqlConnection conn,
            TesReversaSinpeListaModel item)
        {
            var tipoCuenta = TES_TransferenciaRevSinpe_ObtenerTipoCuenta(conn, item.cta_ahorros);
            var infoTrans = TES_TransferenciaRevSinpe_ObtenerInfoTransaccion(conn, item.nsolicitud);

            if (infoTrans == null)
                return DbHelper.ErrorResponse($"No se encontró información SINPE para la solicitud {item.nsolicitud}.");

            var parametros = new
            {
                CODIGO_RECHAZO_SINPE = infoTrans.RECHAZO_CODIGO,
                CODIGO_REFERENCIA = infoTrans.COD_REFERENCIA,
                COMPTOBANTE_CGP = infoTrans.COMPTOBANTE_CGP,
                COMPROBANTE_INTERNO = infoTrans.COD_TRANSITO,
                DESCRIPCION_RECHAZO = infoTrans.RECHAZO_DESC
            };

            var procedimiento = tipoCuenta == 1
                ? "sp_Sinpe_ReversaDebitos"
                : "sp_Sinpe_ReversaCreditos";

            return TES_TransferenciaRevSinpe_EjecutarProcedimiento(conn, procedimiento, parametros);
        }

        /// <summary>
        /// Obtiene el tipo de cuenta a partir del IBAN.
        /// </summary>
        private int TES_TransferenciaRevSinpe_ObtenerTipoCuenta(SqlConnection conn, string? cuentaIban)
        {
            const string query = @"select SUBSTRING(@CuentaIBAN, 9, 2)";

            return conn.QueryFirstOrDefault<int>(
                query,
                new { CuentaIBAN = cuentaIban ?? string.Empty });
        }

        // <summary>
        /// Obtiene la información de tránsito SINPE asociada a una solicitud.
        /// </summary>
        private dynamic? TES_TransferenciaRevSinpe_ObtenerInfoTransaccion(
            SqlConnection conn,
            int nSolicitud)
        {
            const string query = @"
SELECT
    M.RECHAZO_CODIGO,
    M.COD_REFERENCIA,
    0 as COMPTOBANTE_CGP,
    M.COMPROBANTE_INTERNO,
    M.RECHAZO_DESC,
    M.COD_TRANSITO
FROM Tes_Transacciones T
INNER JOIN SINPE_MOV_TRANSITO M
    ON T.REFERENCIA_SINPE = M.COD_REFERENCIA
WHERE T.NSOLICITUD = @Cod_Referencia";

            return conn.QueryFirstOrDefault<dynamic>(
                query,
                new { Cod_Referencia = nSolicitud });
        }

        /// <summary>
        /// Ejecuta el procedimiento de reversa SINPE y valida su respuesta.
        /// </summary>
        private ErrorDto TES_TransferenciaRevSinpe_EjecutarProcedimiento(
            SqlConnection conn,
            string procedimiento,
            object parametros)
        {
            try
            {
                var resultado = conn.QueryFirstOrDefault<SinpeReversaResultado>(
                    procedimiento,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (resultado == null)
                    return DbHelper.ErrorResponse("El procedimiento no devolvió respuesta.");

                if (resultado.Resultado != 0)
                {
                    return DbHelper.ErrorResponse(
                        resultado.DescripcionRechazo ?? "Ocurrió un error al reversar la transferencia.",
                        resultado.Resultado);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Registra la bitácora de la reversa SINPE aplicada.
        /// </summary>
        private void TES_TransferenciaRevSinpe_RegistrarBitacora(
            TesReversaSinpeModel reversa,
            int reversionId)
        {
            _mSecurity.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = reversa.codEmpresa,
                Usuario = reversa.usuario,
                Modulo = module,
                Movimiento = "Aplica",
                DetalleMovimiento = "Reversion Transferencia = " + reversionId +
                                    " Id.Cuenta:" + reversa.id_banco + ", Tipo: TS",
            });
        }

        #endregion

    }
}
