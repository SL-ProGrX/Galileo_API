using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Sinpe_TFT;
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
                solicitud ??= new TransferenciaSolicitudData();

                // Normalizo textos
                var documento = solicitud.documento?.Trim();
                var codigoTxt = solicitud.codigo?.Trim();
                var ndocumentoTxt = solicitud.ndocumento?.Trim();
                var beneficiarioTxt = solicitud.beneficiario?.Trim();
                var ctaAhorrosTxt = solicitud.cta_ahorros?.Trim();
                var codPlanTxt = solicitud.cod_plan?.Trim();

                var hasDocumento = !string.IsNullOrWhiteSpace(documento);

                // Si documento es obligatorio para este endpoint, mejor fallar rápido
                if (!hasDocumento)
                    return DbHelper.CreateErrorResponse<List<TransferenciaSolicitudData>>("Debe indicar el documento base.");

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
                            AND REFERENCIA_SINPE NOT IN (
    	                            SELECT COD_REFERENCIA FROM SINPE_MOV_TRANSITO
                                );";

                var parameters = new
                {
                    documento,
                    id_banco = solicitud.id_banco,

                    Codigo = string.IsNullOrWhiteSpace(codigoTxt) ? null : codigoTxt,
                    CodigoLike = string.IsNullOrWhiteSpace(codigoTxt) ? null : $"%{codigoTxt}%",

                    Ndocumento = string.IsNullOrWhiteSpace(ndocumentoTxt) ? null : ndocumentoTxt,
                    NdocumentoLike = string.IsNullOrWhiteSpace(ndocumentoTxt) ? null : $"%{ndocumentoTxt}%",

                    Beneficiario = string.IsNullOrWhiteSpace(beneficiarioTxt) ? null : beneficiarioTxt,
                    BeneficiarioLike = string.IsNullOrWhiteSpace(beneficiarioTxt) ? null : $"%{beneficiarioTxt}%",

                    CtaAhorros = string.IsNullOrWhiteSpace(ctaAhorrosTxt) ? null : ctaAhorrosTxt,
                    CtaAhorrosLike = string.IsNullOrWhiteSpace(ctaAhorrosTxt) ? null : $"%{ctaAhorrosTxt}%",

                    CodPlan = string.IsNullOrWhiteSpace(codPlanTxt) ? "-sp-" : codPlanTxt
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

                // Normalizo textos
                var documento = reversa.documento?.Trim();
              
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
    	                            SELECT COD_REFERENCIA FROM SINPE_MOV_TRANSITO
                                ) AND ESRADO = 4";

                var parameters = new
                {
                    id_banco = reversa.id_banco,

                    Ndocumento = string.IsNullOrWhiteSpace(documento) ? null : documento,
                    NdocumentoLike = string.IsNullOrWhiteSpace(documento) ? null : $"%{documento}%",

                    NSolicitud = string.IsNullOrWhiteSpace(documento) ? null : documento,
                    NSolicitudLike = string.IsNullOrWhiteSpace(documento) ? null : $"%{documento}%",
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
                //calculo los días entre dos fechas
                if (reversa.lista == null || reversa.lista.Count == 0)
                {
                    return DbHelper.ErrorResponse("No existen registros para reversar.");
                }

                var query = $@"Select * From Tes_Autorizaciones Where Clave= @clave and nombre = @usuario and estado = 'A'";
                var autorizacion = conn.QueryFirstOrDefault<TesAutorizacionesDto>(query,
                    new { reversa.clave, reversa.usuario });

                if (autorizacion == null)
                {
                    return DbHelper.ErrorResponse("Contraseña Incorrecta, o no Existe Nivel de Autorización", -1);
                }

                query = $@"select count(*) as Existe from TES_SINPE_REVERSA where
                                     id_Banco = @id_banco and NDOCUMENTO = @documento ";
                var existe = conn.QueryFirstOrDefault<int>(query,
                    new { reversa.id_banco, documento = reversa.documento!.Trim() });

                if (existe == 1)
                {
                    return DbHelper.ErrorResponse($"La transferencia No.{reversa.documento}, ya fue reversada anteriormente!", -2);
                }


                query = $@"exec spTES_TE_Reversion_Main @id_banco, @tipo, @documento, @observaciones, @usuario ";
                var ReversionId = conn.QueryFirstOrDefault<int>(query,
                    new
                    {
                        reversa.id_banco,
                        tipo = "TS",
                        documento = reversa.documento.Trim(),
                        observaciones = reversa.observaciones ?? string.Empty,
                        usuario = reversa.usuario!.ToUpper()
                    });

                if (reversa.lista.Count > 0)
                {
                    foreach (var item in reversa.lista)
                    {
                        query = $@"select SUBSTRING(@CuentaIBAN,9,2)";
                        var tipoCuenta = conn.QueryFirstOrDefault<int>(query);

                       

                        query = $@"SELECT M.RECHAZO_CODIGO , M.COD_REFERENCIA, 0 as 'COMPTOBANTE_CGP' ,M.COMPROBANTE_INTERNO , M.RECHAZO_DESC, FROM Tes_Transacciones T INNER JOIN SINPE_MOV_TRANSITO M
                                    ON T.REFERENCIA_SINPE = M.COD_REFERENCIA
                                    WHERE M.COD_REFERENCIA = @Cod_Referencia";
                        var infoTrans = conn.QueryFirstOrDefault<dynamic>(query,
                            new {
                                Cod_Referencia = item.nsolicitud
                            });

                        var parametros = new
                        {
                            CODIGO_RECHAZO_SINPE = infoTrans!.RECHAZO_CODIGO,
                            CODIGO_REFERENCIA = infoTrans.COD_REFERENCIA,
                            COMPTOBANTE_CGP = infoTrans.COMPTOBANTE_CGP,
                            COMPROBANTE_INTERNO = infoTrans.COMPROBANTE_INTERNO,
                            DESCRIPCION_RECHAZO = infoTrans.RECHAZO_DESC
                        };

                        var procedimiento = tipoCuenta == 1
                            ? "sp_Sinpe_ReversaDebitos"
                            : "sp_Sinpe_ReversaCreditos";

                        conn.Execute(
                            procedimiento,
                            parametros,
                            commandType: CommandType.StoredProcedure
                        );

                    }
                }

                //bitacora
                _mSecurity.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = reversa.codEmpresa,
                    Usuario = reversa.usuario,
                    Modulo = module, // Tesoreria
                    Movimiento = "Aplica",
                    DetalleMovimiento = "Reversion Transferencia = " + ReversionId + " Id.Cuenta:" + reversa.id_banco + ", Tipo: TS",
                });

                return DbHelper.OkResponse(ReversionId.ToString());
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        #endregion

    }
}
