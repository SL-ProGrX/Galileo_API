using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.TES;
using System.Reflection;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ReImpresionDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;
        private readonly int module = 9;
        private readonly mSecurityMainDb mSecurity;
        private readonly mReportingServicesDB srvReportes;
        private readonly mProGrX_AuxiliarDB mProGrX_Auxiliar;

        public frmTES_ReImpresionDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
            mSecurity = new mSecurityMainDb(config);
            srvReportes = new mReportingServicesDB(config);
            mProGrX_Auxiliar = new mProGrX_AuxiliarDB(config);
        }

        /// <summary>
        /// Metodo para obtener los datos de la solicitud de reimpresión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<tesReImpresionModels> TES_ReImpresion_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<tesReImpresionModels>
            {
                Code = 0,
                Description = "Ok",
                Result = new tesReImpresionModels
                {
                    verifica = " - El Documento se puede ReImprimir...",
                    verificaTag = "S"
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento,Y.comprobante
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_banco_docs Y on C.id_banco = Y.id_Banco and C.tipo = Y.tipo
                                    where C.nsolicitud = @solicitud ";

                    response.Result = connection.Query<tesReImpresionModels>(query,
                        new
                        {solicitud = solicitud }).FirstOrDefault();

                    if(response.Result != null)
                    {
                        response.Result.verificaTag = "S";

                        if (response.Result != null)
                        {
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
                            response.Result = new tesReImpresionModels();
                        }
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "Datos no encontrados";
                        response.Result = null;
                    }

                    
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
        /// metodo para guardar la solicitud de reimpresión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<object> TES_ReImpresion_Guardar(int CodEmpresa, tesReImpresionModels solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
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

                        var result = connection.Execute(query, new
                        {
                            solicitud = solicitud.nSolicitud,
                            usuarioLogin = solicitud.usuarioLogin,
                            usuario = solicitud.usuario,
                            notas = solicitud.detalle_Anulacion
                        });

                        //bitacora
                        mTesoreria.sbTesBitacoraEspecial(CodEmpresa, solicitud.nSolicitud, "17", solicitud.detalle_Anulacion, solicitud.usuario);

                        mSecurity.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = solicitud.usuario,
                            Modulo = module, // Tesoreria
                            Movimiento = "Aplica",
                            DetalleMovimiento = "ReImpresión de Solicitud :" + solicitud.nSolicitud,
                        });

                    }

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
        /// Metodo para reimprimir el documento de la solicitud de reimpresión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        private ErrorDto<object> sbReImprime(int CodEmpresa, tesReImpresionModels solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0
            };
            try
            {
               var archivoEspecial = mTesoreria.sbCargaArchivosEspeciales(CodEmpresa, solicitud.id_banco);
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select firmas_desde,firmas_hasta,formato_transferencia,Lugar_Emision  from Tes_Bancos where id_banco = @banco ";
                    var banco = connection.QueryFirstOrDefault<tesReImpresionBancoData>(query, new { banco = solicitud.id_banco });

                    query = $@"select isnull(count(*),0) as Existe from TES_BANCO_FIRMASAUT where id_Banco = @banco
                                      and usuario = @usuario ";

                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        banco = solicitud.id_banco,
                        usuario = solicitud.usuarioLogin
                    });

                    bool vFirmas = existe > 0 ? true : false;

                    query = $@"select * from Tes_Transacciones where nsolicitud = @solicitud ";
                    var transaccion = connection.QueryFirstOrDefault<TES_TransaccionDTO>(query, new { solicitud = solicitud.nSolicitud });

                    //Imprime reporte
                    frmReporteGlobal data = new frmReporteGlobal();
                    data.codEmpresa = CodEmpresa;
                    data.parametros = null;
                    data.nombreReporte = "";
                    data.usuario = solicitud.usuarioLogin;
                    data.cod_reporte = "P";
                    data.folder = "Bancos";

                    if (banco.Lugar_Emision != "")
                    {
                        banco.Lugar_Emision += ",";
                    }

                    //Busco Archivo de Banco:
                    query = $@"SELECT ARCHIVO_ESPECIAL_CK, ARCHIVO_CHEQUES_FIRMAS  ,ARCHIVO_CHEQUES_SIN_FIRMAS  FROM Tes_Bancos
                                  WHERE ID_BANCO = @bancos";

                    var docFormatos = connection.QueryFirstOrDefault<tesReImpresionDoc>(query, new { bancos = solicitud.id_banco });

                    if(existe > 0) 
                    {
                        if(transaccion.monto >= banco.firmas_desde && transaccion.monto <= banco.firmas_hasta)
                        {
                            //Cheque Con Firmas
                            if(!string.IsNullOrEmpty(docFormatos.archivo_cheques_firmas))
                            {
                                data.nombreReporte = solicitud.id_banco + "_" + docFormatos.archivo_cheques_firmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", "");
                            }
                            else
                            {
                                data.nombreReporte = "Banking_DocFormat01";
                             } 
                        }
                        else
                        {
                            //Cehque Sin firmas
                            if (!string.IsNullOrEmpty(docFormatos.archivo_cheques_sin_firmas))
                            {
                                data.nombreReporte = solicitud.id_banco + "_" + docFormatos.archivo_cheques_sin_firmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", "");
                            }
                            else
                            {
                                data.nombreReporte = "Banking_DocFormat02";
                            }
                        }
                    }
                    else
                    {
                        //Cehque Sin firmas
                        if (!string.IsNullOrEmpty(docFormatos.archivo_cheques_sin_firmas))
                        {
                            data.nombreReporte = solicitud.id_banco + "_" + docFormatos.archivo_cheques_sin_firmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", "");
                        }
                        else
                        {
                            data.nombreReporte = "Banking_DocFormat02";
                        }
                    }

                    decimal vMonto = Convert.ToDecimal(transaccion.monto);
                    string vMesLetras = mTesoreria.fxTesMesDescripcion(transaccion.fecha_emision.Value.Month);

                    var parametrosJson = new
                    {
                        filtros = $@" WHERE 1=1 AND CHEQUES.NSOLICITUD = {solicitud.nSolicitud} ",
                            Fecha = $@" {banco.Lugar_Emision} DE {vMesLetras} DE {transaccion.fecha_emision.Value.Year.ToString()} ",
                            Año = transaccion.fecha_emision.Value.Year.ToString(),
                            Letras = mProGrX_Auxiliar.NumeroALetras(vMonto).Result,

                        };

                    data.parametros = System.Text.Json.JsonSerializer.Serialize(parametrosJson);

                    var actionResult = srvReportes.ReporteRDLC_v2(data);

                    // Asegúrate de castear a ObjectResult, no a ResImpresion
                    var objectResult = actionResult as ObjectResult;
                    if (objectResult == null)
                    {
                        //response.Code = -1;
                        //response.Description = "Error al generar el reporte, verifique...";
                        //return response;
                        response.Result = actionResult;
                    }
                    else
                    {
                        // Si tu action hace: return StatusCode(200, resImpresion);
                        var res = objectResult.Value;
                        //converto res a JSON
                        var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                        // convierto JSON a ErrorDto
                        var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                        response.Code = err.Code;
                        response.Description = err.Description;
                    }
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al generar el reporte, verifique...";
            }
            return response;
        }

    }
}
