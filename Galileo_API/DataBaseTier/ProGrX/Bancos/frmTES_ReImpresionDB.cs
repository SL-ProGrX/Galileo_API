using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesReImpresionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;
        private readonly int module = 9;
        private readonly MSecurityMainDb mSecurity;
        private readonly MReportingServicesDB srvReportes;

        public FrmTesReImpresionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mTesoreria = new MTesoreria(config);
            mSecurity = new MSecurityMainDb(config);
            srvReportes = new MReportingServicesDB(config);
        }

        /// <summary>
        /// Método para obtener los datos de la solicitud de reimpresión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesReImpresionModels> TES_ReImpresion_Obtener(int CodEmpresa, int solicitud)
        {

            var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesReImpresionModels>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesReImpresionModels
                {
                    verifica = " - El Documento se puede ReImprimir...",
                    verificaTag = "S"
                }
            };
            try
            {
                var query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento,Y.comprobante
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_banco_docs Y on C.id_banco = Y.id_Banco and C.tipo = Y.tipo
                                    where C.nsolicitud = @solicitud ";

                response.Result = connection.Query<TesReImpresionModels>(query,
                    new
                    { solicitud = solicitud }).FirstOrDefault();

                if (response.Result != null)
                {
                    response.Result.verificaTag = "S";

                    if (response.Result.comprobante != "01")
                    {
                        response.Result.verifica = " - El Documento Actual no se puede ReImprimir, porque no es Cheque Continuo...";
                        response.Result.verificaTag = "N";
                    }

                    if (response.Result.estado != "I")
                    {
                        response.Result.verifica += " - El documento no se encuentra Impreso / No se puede ReImprimir...";
                        response.Result.verificaTag = "N";
                    }

                    if (response.Result.verificaTag == "S")
                    {
                        response.Result.verifica = " - El Documento se puede ReImprimir...";
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "Datos no encontrados";
                    response.Result = null;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Método para guardar la solicitud de reimpresión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<object> TES_ReImpresion_Guardar(int CodEmpresa, TesReImpresionModels solicitud)
        {
            var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0
            };
            try
            {
                var query = "";
                //'Verificar Usuarios y Claves de Autorización
                query = $@"select isnull(count(*),0) as Existe from tes_autorizaciones where nombre = @usuario 
                                    and estado = 'A' and clave = @clave ";

                var existe = connection.QueryFirstOrDefault<int>(query, new
                {
                    usuario = solicitud.usuario,
                    clave = solicitud.clave
                });

                if (existe == 0)
                {
                    response.Code = -1;
                    response.Description = "El usuario y clave de autorización no concuerda con ninguno de los registrados, verifique...";
                    return response;
                }

                solicitud.usuarioLogin = solicitud.usuario;

                var impresion = sbReImprime(CodEmpresa, solicitud);
                response = impresion;


                if (impresion.Code != -1)
                {
                    //detalle de anulacion debe ser de maximo 100 caracteres
                    solicitud.detalle_Anulacion = solicitud.detalle_Anulacion.Length > 100 ? solicitud.detalle_Anulacion.Substring(0, 100) : solicitud.detalle_Anulacion;

                    query = $@"insert tes_ReImpresiones(nsolicitud,fecha,usuario,autoriza,notas) 
                                    values(@solicitud, dbo.MyGetdate(),@usuarioLogin,@usuario,@notas)";

                    connection.Execute(query, new
                    {
                        solicitud = solicitud.nSolicitud,
                        usuarioLogin = solicitud.usuarioLogin,
                        usuario = solicitud.usuario,
                        notas = solicitud.detalle_Anulacion
                    });

                    //bitacora
                    mTesoreria.sbTesBitacoraEspecial(CodEmpresa, solicitud.nSolicitud, "17", solicitud.detalle_Anulacion, solicitud.usuario);

                    mSecurity.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = solicitud.usuario,
                        Modulo = module, // Tesoreria
                        Movimiento = "Aplica",
                        DetalleMovimiento = "ReImpresión de Solicitud :" + solicitud.nSolicitud,
                    });

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Reimprime el documento asociado a la solicitud indicada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="solicitud">Datos de la solicitud de reimpresión.</param>
        /// <returns>Resultado del proceso de reimpresión.</returns>
        private ErrorDto<object> sbReImprime(int CodEmpresa, TesReImpresionModels solicitud)
        {
            var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0
            };

            try
            {
                const string queryBanco = @"select firmas_desde,firmas_hasta,formato_transferencia,Lugar_Emision  
                                    from Tes_Bancos 
                                    where id_banco = @banco ";

                var banco = connection.QueryFirstOrDefault<TesReImpresionBancoData>(queryBanco, new
                {
                    banco = solicitud.id_banco
                });

                const string queryAutorizacion = @"select isnull(count(*),0) as Existe 
                                           from TES_BANCO_FIRMASAUT 
                                           where id_Banco = @banco
                                           and usuario = @usuario ";

                var existe = connection.QueryFirstOrDefault<int>(queryAutorizacion, new
                {
                    banco = solicitud.id_banco,
                    usuario = solicitud.usuarioLogin
                });

                const string queryTransaccion = @"select * 
                                          from Tes_Transacciones 
                                          where nsolicitud = @solicitud ";

                var transaccion = connection.QueryFirstOrDefault<TesTransaccionDto>(queryTransaccion, new
                {
                    solicitud = solicitud.nSolicitud
                });

                const string queryFormatos = @"SELECT ARCHIVO_ESPECIAL_CK, ARCHIVO_CHEQUES_FIRMAS, ARCHIVO_CHEQUES_SIN_FIRMAS  
                                       FROM Tes_Bancos
                                       WHERE ID_BANCO = @bancos";

                var docFormatos = connection.QueryFirstOrDefault<TesReImpresionDoc>(queryFormatos, new
                {
                    bancos = solicitud.id_banco
                });

                var data = new FrmReporteGlobal
                {
                    codEmpresa = CodEmpresa,
                    parametros = null,
                    nombreReporte = ObtenerNombreReporte(solicitud, banco, transaccion, docFormatos, existe > 0),
                    usuario = solicitud.usuarioLogin,
                    cod_reporte = "P",
                    folder = "Bancos"
                };

                data.parametros = ConstruirParametrosReporte(solicitud, banco, transaccion);

                var actionResult = srvReportes.ReporteRDLC_v2(data);
                MapearResultadoReporte(actionResult, response);
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al generar el reporte, verifique...";
            }

            return response;
        }

        /// <summary>
        /// Obtiene el nombre del reporte que se debe utilizar para la reimpresión.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud.</param>
        /// <param name="banco">Configuración del banco.</param>
        /// <param name="transaccion">Transacción a reimprimir.</param>
        /// <param name="docFormatos">Formatos configurados para el banco.</param>
        /// <param name="usuarioTieneFirma">Indica si el usuario tiene firma autorizada para el banco.</param>
        /// <returns>Nombre del reporte a ejecutar.</returns>
        private static string ObtenerNombreReporte(
            TesReImpresionModels solicitud,
            TesReImpresionBancoData banco,
            TesTransaccionDto transaccion,
            TesReImpresionDoc docFormatos,
            bool usuarioTieneFirma)
        {
            if (DebeUsarChequeConFirmas(banco, transaccion, usuarioTieneFirma))
            {
                return !string.IsNullOrEmpty(docFormatos.archivo_cheques_firmas)
                    ? ConstruirNombreReporteBanco(solicitud.id_banco, docFormatos.archivo_cheques_firmas)
                    : "Banking_DocFormat01";
            }

            return !string.IsNullOrEmpty(docFormatos.archivo_cheques_sin_firmas)
                ? ConstruirNombreReporteBanco(solicitud.id_banco, docFormatos.archivo_cheques_sin_firmas)
                : "Banking_DocFormat02";
        }

        /// <summary>
        /// Determina si la transacción debe imprimirse con formato de cheque con firmas.
        /// </summary>
        /// <param name="banco">Configuración del banco.</param>
        /// <param name="transaccion">Transacción evaluada.</param>
        /// <param name="usuarioTieneFirma">Indica si el usuario tiene firma autorizada.</param>
        /// <returns>True si aplica formato con firmas; de lo contrario false.</returns>
        private static bool DebeUsarChequeConFirmas(
            TesReImpresionBancoData banco,
            TesTransaccionDto transaccion,
            bool usuarioTieneFirma)
        {
            return usuarioTieneFirma
                && transaccion.monto >= banco.firmas_desde
                && transaccion.monto <= banco.firmas_hasta;
        }

        /// <summary>
        /// Construye el nombre final del reporte del banco removiendo extensiones conocidas.
        /// </summary>
        /// <param name="idBanco">Identificador del banco.</param>
        /// <param name="archivoReporte">Archivo configurado para el reporte.</param>
        /// <returns>Nombre del reporte a ejecutar.</returns>
        private static string ConstruirNombreReporteBanco(int idBanco, string archivoReporte)
        {
            return idBanco + "_" + archivoReporte
                .Replace(".rdl", "")
                .Replace(".rdlc", "")
                .Replace(".RDL", "")
                .Replace(".RDLC", "");
        }

        /// <summary>
        /// Construye el JSON de parámetros requerido por el servicio de reportes.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud.</param>
        /// <param name="banco">Configuración del banco.</param>
        /// <param name="transaccion">Transacción a reimprimir.</param>
        /// <returns>Parámetros serializados en JSON.</returns>
        private static string ConstruirParametrosReporte(
            TesReImpresionModels solicitud,
            TesReImpresionBancoData banco,
            TesTransaccionDto transaccion)
        {
            var lugarEmision = banco.Lugar_Emision ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(lugarEmision))
            {
                lugarEmision += ",";
            }

            if (!transaccion.fecha_emision.HasValue)
            {
                throw new System.ArgumentException("La transacción no tiene fecha de emisión.", nameof(transaccion));
            }

            var fechaEmision = transaccion.fecha_emision.Value;
            decimal vMonto = Convert.ToDecimal(transaccion.monto);
            string vMesLetras = MTesoreria.fxTesMesDescripcion(fechaEmision.Month);

            var parametrosJson = new
            {
                filtros = $@" WHERE 1=1 AND CHEQUES.NSOLICITUD = {solicitud.nSolicitud} ",
                Fecha = $@" {lugarEmision} DE {vMesLetras} DE {fechaEmision.Year} ",
                Año = fechaEmision.Year.ToString(),
                Letras = MProGrXAuxiliarDB.NumeroALetras(vMonto).Result,
            };

            return System.Text.Json.JsonSerializer.Serialize(parametrosJson);
        }

        /// <summary>
        /// Mapea la respuesta del servicio de reportes al objeto estándar del proceso.
        /// </summary>
        /// <param name="actionResult">Resultado devuelto por el servicio de reportes.</param>
        /// <param name="response">Respuesta estándar a completar.</param>
        private static void MapearResultadoReporte(IActionResult actionResult, ErrorDto<object> response)
        {
            var objectResult = actionResult as ObjectResult;

            if (objectResult == null)
            {
                response.Result = actionResult;
                return;
            }

            var res = objectResult.Value;
            var json = System.Text.Json.JsonSerializer.Serialize(res);
            var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(json);

            response.Code = err.Code;
            response.Description = err.Description;
        }
    }
}
