using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier;
using PgxAPI.Models.ProGrX_Nucleo;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifConsultaDocumentosDB
    {

        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmSifConsultaDocumentosDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        ///  Consulta la última apertura de caja para la caja indicada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCajas"></param>
        /// <returns></returns>
        public ErrorDto<int> SifConsultaDocumentos_CajaUltimaApertura_Consultar(int CodEmpresa, string pCajas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0,
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"select dbo.fxSIFDocsCajaUltimaApertura (@pCajas) as Resultado";
                result.Result = connection.Query<int>(query, new { pCajas }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = -1;
            }

            return result;
        }


        /// <summary>
        /// Actualiza la transacción de un documento 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="actDocumento"></param>
        /// <param name="antDocumento"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        public ErrorDto SifConsultaDocumentos_Transaccion_Actualizar(int CodEmpresa, string usuario, string actDocumento, string antDocumento, string tipoDocumento, string codTransaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"update sif_Transacciones 
                                set Documento = @actDocumento                                    
                                    WHERE tipo_documento = @tipoDocumento  and cod_transaccion =@codTransaccion ";
                    connection.Execute(query, new
                    {
                        actDocumento,
                        tipoDocumento,
                        codTransaccion
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"TDoc.: {tipoDocumento} - NDoc.:{codTransaccion} - Act.Doc.: {actDocumento}  - Ant.Doc.:  {antDocumento} ",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Envio de recibo digital
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="formato"></param>
        /// <returns></returns>
        public ErrorDto SifConsultaDocumentos_ReciboDigitar_Enviar(int CodEmpresa, string codigo, string tipoDocumento, string formato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"exec spCajasReciboDigital @codigo, @tipoDocumento,@formato";
                connection.Execute(query, new
                {
                    codigo,
                    tipoDocumento,
                    formato
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }


        /// <summary>
        /// Consulta las formas de pago de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocsFormasDePagoData>> SifConsultaDocumentos_FormasDePago_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocsFormasDePagoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocsFormasDePagoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@" select F.DESCRIPCION,
                                    P.Monto, P.COD_DIVISA, P.TIPO_CAMBIO, P.monto / dbo.fxSys_Tipo_Cambio_Apl(P.TIPO_CAMBIO)  AS importe_real
                                    , case when F.TIPO = 'C' then 'CK.: ' + P.CHEQUE_NUMERO + ' - Emisor.: ' + P.CHEQUE_EMISOR
                                     when F.TIPO = 'D' then 'DOC.: ' + P.NUM_REFERENCIA
                                     when F.TIPO = 'T' then 'TARJ.: ' + P.TARJETA_NUMERO + ' AUT.:' + P.TARJETA_AUTORIZACION + '  TIPO..:' + P.COD_TARJETA
                                     when F.TIPO = 'S' then  S.DOC_TIPO + ' - ' + S.DOC_NUMERO + '     (Id.: ' + CONVERT(VARCHAR(20), S.LINEA) + ') '
                                     else P.NUM_REFERENCIA end AS referencia
                                     ,ISNULL(P.OBSERVACIONES,'') AS 'NOTAS'
                                     from SIF_TRANSACCIONES_PAGO P inner join SIF_FORMAS_PAGO F on P.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                                     left join CAJAS_SALDO_FAVOR S on P.SALDO_FAVOR_ID = S.Linea
                                    where P.tipo_documento = @tipoDocumento and P.cod_transaccion = @codTransaccion order by P.cod_linea";

                result.Result = connection.Query<SifConsultaDocsFormasDePagoData>(query, new { tipoDocumento, codTransaccion }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta el seguimiento de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        public ErrorDto<SifConsultaDocSeguimientoData> SifConsultaDocumentos_Seguimiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifConsultaDocSeguimientoData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifConsultaDocSeguimientoData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"Select registro_fecha,registro_usuario,traspaso_fecha,traspaso_usuario,anulacion_fecha,anulacion_usuario
                                    from sif_transacciones
                                    where tipo_documento = @tipoDocumento and cod_transaccion = @codTransaccion";

                    result.Result = connection.Query<SifConsultaDocSeguimientoData>(query, new { tipoDocumento, codTransaccion }).FirstOrDefault();
                    if (result.Result != null)
                    {

                        result.Result.registro_fechast =
                            result.Result.registro_fecha == DateTime.MinValue
                            ? ""
                            : result.Result.registro_fecha.ToString("dd/MM/yyyy hh:mm:ss tt");

                        result.Result.anulacion_fechast =
                            result.Result.anulacion_fecha == DateTime.MinValue
                            ? ""
                            : result.Result.anulacion_fecha.ToString("dd/MM/yyyy hh:mm:ss tt");

                        result.Result.traspaso_fechast =
                            result.Result.traspaso_fecha == DateTime.MinValue
                            ? ""
                            : result.Result.traspaso_fecha.ToString("dd/MM/yyyy hh:mm:ss tt");

                    }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        ///  Consulta la informacion generalde un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        public ErrorDto<SifConsultaDocCargaDocumentoData> SifConsultaDocumentos_CargaDocumento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifConsultaDocCargaDocumentoData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifConsultaDocCargaDocumentoData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@" select T.tipo_documento,T.cod_transaccion, Docs.Descripcion as 'DocumentoDesc' 
                                   , isnull(T.cliente_identificacion,'') as identificacion, isnull(T.cliente_nombre,'') as nombre,T.monto,T.registro_fecha
                                   , T.cod_Concepto,T.registro_usuario, C.descripcion as concepto,Documento,O.Descripcion as oficina,Ca.Descripcion as 'Caja'
                                   , case when T.Estado = 'P' then 'Pendiente' when T.Estado = 'I' then 'Impreso' when T.Estado = 'A' then 'Anulado' end as 'Estado'
                                   , Linea1,Linea2,Linea3,Linea4,Linea5,Linea6,Linea7,Linea8,Linea9,Linea10,Linea11,Detalle,isnull(T.TRASLADO_BLOQUEO,0) as 'Bloqueo'
                                   , dbo.fxCajas_Recibo_Digital_Doc_Aplica(T.Tipo_Documento, T.Cod_Transaccion) as 'Recibo_Digital'
                                  from sif_transacciones T
                                  inner join sif_conceptos C on T.cod_concepto = C.cod_concepto
                                  inner join sif_documentos Docs on T.tipo_documento = Docs.Tipo_Documento
                                  inner join sif_oficinas O on T.cod_Oficina = O.cod_oficina
                                  left join cajas_definicion Ca on T.cod_caja = Ca.cod_caja
                                  where T.tipo_documento = @tipoDocumento and T.cod_transaccion = @codTransaccion";

                result.Result = connection.Query<SifConsultaDocCargaDocumentoData>(query, new { tipoDocumento, codTransaccion }).FirstOrDefault();

                if (result.Result != null)
                {
                    result.Result.detalle = BuildDetalle(result.Result);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Construye el campo detalle concatenando las líneas y el detalle original.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static  string BuildDetalle(SifConsultaDocCargaDocumentoData data)
        {
            var lines = new List<string>
            {
                data.linea1 ?? "",
                data.linea2 ?? "",
                data.linea3 ?? "",
                data.linea4 ?? "",
                data.linea5 ?? "",
                data.linea6 ?? "",
                data.linea7 ?? "",
                data.linea8 ?? "",
                data.linea9 ?? "",
                data.linea10 ?? "",
                data.linea11 ?? "",
                data.detalle ?? ""
            };

            var nonEmptyLines = lines.Where(line => !string.IsNullOrEmpty(line)).ToList();
            return string.Join("\n", nonEmptyLines);
        }


        /// <summary>
        /// Consulta los asientos contables de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocCargaAsientoData>> SifConsultaDocumentos_CargaAsiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocCargaAsientoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocCargaAsientoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select isnull(C.Cod_Cuenta_Mask, D.cod_cuenta) as 'COD_CUENTA' , isnull(C.descripcion,'--Cuenta No Existe--') as 'Descripcion',D.Cod_divisa,D.tipo_movimiento,D.monto,D.cod_unidad
                                 ,U.descripcion as UnidadX,D.cod_centro_costo,X.descripcion as CCX,D.Tipo_Cambio
                                 ,D.Referencia_01,D.Referencia_02,D.Referencia_03, D.Monto / dbo.fxSys_Tipo_Cambio_Apl(D.Tipo_Cambio) as 'IMPORTE_REAL', D.Tipo_Documento, D.Cod_Transaccion
                                  from Sif_transacciones_asiento D left join CntX_Cuentas C on D.cod_cuenta = C.cod_cuenta and D.cod_contabilidad = C.cod_contabilidad
                                  left join cntx_unidades U on D.cod_unidad = U.cod_unidad and D.cod_contabilidad = U.cod_contabilidad
                                  left join cntx_centro_costos X on D.cod_centro_costo = X.cod_centro_costo and D.cod_contabilidad = X.cod_contabilidad
                                  where D.cod_transaccion=  @codTransaccion and tipo_Documento = @tipoDocumento
                                  order by D.Numero_linea";

                    result.Result = connection.Query<SifConsultaDocCargaAsientoData>(query, new { tipoDocumento, codTransaccion }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta el nombre de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <returns></returns>
        public ErrorDto<string> SifConsultaDocumentos_NombreDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select Descripcion from SIF_Documentos where Activo = 1  and Tipo_Documento like @tipoDocumento";
                    result.Result = connection.Query<string>(query, new { tipoDocumento }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        ///  Ejecuta la reversión o actualización de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="documento"></param>
        /// <param name="tipoDocumento"></param>
        /// <returns></returns>
        public ErrorDto SifConsultaDocumentos_Reversar_Actualizar(int CodEmpresa, string usuario, string documento, string tipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                    var query = $@"exec spSIFDocsReversaMain @tipoDocumento,@documento,@usuario ";
                    connection.Execute(query, new
                    {
                        tipoDocumento,
                        documento,
                        usuario
                    });


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }


        /// <summary>
        /// Consulta las cajas activas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Cajas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                    var query = $@"select rtrim(cod_caja) as 'item',rtrim(descripcion) as 'descripcion' FROM cajas_definicion where activa = 1";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta listado de formas de pago
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_FormasPago_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                    var query = $@"select rtrim(cod_forma_pago) as 'item',rtrim(descripcion) as 'descripcion' FROM sif_Formas_Pago  where activa = 1";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta listado de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Bancos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                    var query = $@"select id_banco as 'item',descripcion  as 'descripcion' FROM Tes_Bancos where estado = 'A'";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta el nombre de un usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<string> SifConsultaDocumentos_NombreUsuario_Consultar(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select descripcion from usuarios where nombre =@usuario";
                    result.Result = connection.Query<string>(query, new { usuario }).FirstOrDefault();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        ///  Consulta las cuentas por cobrar de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocCuentasPorCobrarData>> SifConsultaDocumentos_CuentasPorCobrar_Obtener(int CodEmpresa, string documento, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocCuentasPorCobrarData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocCuentasPorCobrarData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select * from vSIF_CtrlDoc_CxC_Detalle Where TCon= @documento And NCon = @codigo";

                    result.Result = connection.Query<SifConsultaDocCuentasPorCobrarData>(query, new { documento, codigo }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta los patrimonios de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocPatrimoniosData>> SifConsultaDocumentos_Patrimonios_Obtener(int CodEmpresa, string documento, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocPatrimoniosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocPatrimoniosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select * from vSIF_CtrlDoc_Pat_Detalle Where TCon= @documento And NCon = @codigo";

                    result.Result = connection.Query<SifConsultaDocPatrimoniosData>(query, new { documento, codigo }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta los fondos de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocFondosData>> SifConsultaDocumentos_Fondos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocFondosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocFondosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select * from vSIF_CtrlDoc_Fnd_Detalle Where TCon= @documento And NCon = @codigo";

                    result.Result = connection.Query<SifConsultaDocFondosData>(query, new { documento, codigo }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta los créditos de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConsultaDocCreditosData>> SifConsultaDocumentos_Creditos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConsultaDocCreditosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConsultaDocCreditosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select * from vSIF_CtrlDoc_Crd_Detalle Where TCon= @documento And NCon = @codigo";

                    result.Result = connection.Query<SifConsultaDocCreditosData>(query, new { documento, codigo }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        ///  Consulta el último documento de un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <returns></returns>
        public ErrorDto<string> SifConsultaDocumentos_UltDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select  Top 1 rtrim(COD_TRANSACCION)  as 'Transaccion'
                                    from Sif_Transacciones
                                     where Tipo_Documento = @tipoDocumento
                                      order by Registro_Fecha desc, Cod_Transaccion";
                    result.Result = connection.Query<string>(query, new { tipoDocumento }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Consulta la siguiente transacción de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="transaccion"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<string> SifConsultaDocumentos_SiguienteTransaccion_Consultar(int CodEmpresa, string tipoDocumento, string transaccion, int orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select Top 1 cod_transaccion from sif_transacciones";
                    if (orden == 1)
                    {
                        query += $@" where tipo_documento =@tipoDocumento  and cod_transaccion > @transaccion order by cod_transaccion asc";
                    }
                    else
                    {
                        query += $@" where tipo_documento =@tipoDocumento  and cod_transaccion < @transaccion order by cod_transaccion desc";
                    }
                    result.Result = connection.Query<string>(query, new { tipoDocumento, transaccion }).FirstOrDefault();
                    if (result.Result == null)
                    {
                        result.Result = "0";
                    }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// consulta el listado de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Documentos_Obtener(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select tipo_documento as 'item',descripcion as 'descripcion'  
                                    from sif_documentos
                                       where Activo = 1 and descripcion like '%'+ @filtro +'%'
                                       order by descripcion";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query, new { filtro }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        ///  Consulta el listado de tipos de conceptos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_TipoConceptos_Obtener(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select cod_concepto as 'item',descripcion as 'descripcion'  
                                    from sif_conceptos
                                        Where descripcion like '%'+ @filtro +'%'
                                       order by descripcion";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query, new { filtro }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        ///  Consulta usuario  asignadas a una caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="caja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_UsuariosCajas_Obtener(int CodEmpresa, string caja)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select USUARIO as 'item',USUARIO as 'descripcion'  
                                    from CAJAS_USUARIOS
                                       where COD_CAJA = @caja";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query, new { caja }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta el listado de documentos según los filtros indicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifConsultaDocTrasaccionesDataLista> SifConsultaDocumentos_Buscar(int CodEmpresa, bool esExportar, SifConsultaDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<SifConsultaDocTrasaccionesDataLista>
            {
                Code = 0,
                Description = "OK",
                Result = new SifConsultaDocTrasaccionesDataLista
                {
                    totales = new SifConsultaDocTrasaccionesTotales()
                    {
                        total = 0,
                        montototal = 0
                    },
                    lista = new List<SifConsultaDocTrasaccionesData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
                {
                    response.Code = -1;
                    response.Description = "Las fechas de inicio y corte son requeridas.";
                    response.Result = null;
                    return response;
                }

                string query = BuildDocumentosBuscarQuery(filtros, CodEmpresa, out var parametrosDynamic);

                // Cast dynamic to object to avoid Dapper extension method issue
                object parametros = parametrosDynamic;

                var totales = connection.Query<SifConsultaDocTrasaccionesData>(query, parametros).ToList();

                response.Result.totales.total = totales.Count;
                response.Result.totales.montototal = totales.Sum(x => x.monto);

                query = AppendOrderAndPaging(query, filtros, esExportar);

                response.Result.lista = connection.Query<SifConsultaDocTrasaccionesData>(query, parametros).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al buscar documentos: {ex.Message}";
                response.Result = null;
            }
            return response;
        }

        private string BuildDocumentosBuscarQuery(SifConsultaDocFiltros filtros, int CodEmpresa, out dynamic parametros)
        {
            int pSesion = 0;
            string[] conceptos = Array.Empty<string>();
            string[] documentos = Array.Empty<string>();

            if (string.IsNullOrEmpty(filtros.sortField))
            {
                filtros.sortField = "T.Registro_fecha desc, Tipo_Documento, Cod_Transaccion ";
            }
            if (!string.IsNullOrEmpty(filtros.filtro))
            {
                filtros.filtro = "where  ( Cod_transaccion LIKE '%" + filtros.filtro + "%' " +
                    " OR documento LIKE '%" + filtros.filtro + "%' " +
                    " OR Tipo_documento LIKE '%" + filtros.filtro + "%' ) ";
            }

            var query = $@"select Cod_transaccion,isnull(documento,0) as documento, Tipo_documento, monto, case when estado = 'I' then 'Emitido'
                            when estado = 'P' then 'Pendiente' when  estado = 'A' then 'Anulado'  end as Estado
                            ,isnull(Registro_fecha,'') as Fecha_registro,isnull(Registro_Usuario,'') as 'Usuario', cod_caja, cod_apertura, Id_Sesion
                            , cod_oficina, Cliente_Identificacion, Cliente_Nombre
                            ,isnull(Detalle,'') as detalle 
                            from ( 
                                    select Cod_transaccion,isnull(documento,0) as documento, Tipo_documento, monto, case when estado = 'I' then 'Emitido'
                                    when estado = 'P' then 'Pendiente' when  estado = 'A' then 'Anulado'  end as Estado
                                    ,isnull(Registro_fecha,'') as Registro_fecha,isnull(Registro_Usuario,'') as Registro_Usuario, cod_caja, cod_apertura, Id_Sesion
                                    , cod_oficina, Cliente_Identificacion, Cliente_Nombre
                                    ,isnull(Detalle,'') as detalle from Sif_Transacciones ";

            query += BuildWhereClause(filtros, ref documentos, ref conceptos, ref pSesion, CodEmpresa);

            query += $@") AS T {filtros.filtro} ";

            if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
            {
                throw new ArgumentException("Las fechas de inicio y corte son requeridas.");
            }
            DateTime fecha_inicio = filtros.fecha_inicio.Value.Date.AddHours(00).AddMinutes(00).AddSeconds(00);
            DateTime fecha_inicioF = new DateTime(fecha_inicio.Year, fecha_inicio.Month, fecha_inicio.Day, 0, 0, 0, DateTimeKind.Local);
            DateTime fecha_corte = filtros.fecha_corte.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            DateTime fecha_corteF = new DateTime(fecha_corte.Year, fecha_corte.Month, fecha_corte.Day, 23, 59, 59, DateTimeKind.Local);

            parametros = new
            {
                filtros.valor_filtro,
                Listadocumentos = documentos,
                Listaconceptos = conceptos,
                fecha_inicio = fecha_inicioF,
                fecha_corte = fecha_corteF,
                filtros.chk_documentos_bloqueados,
                filtros.usuario_registra,
                filtros.no_transaccion,
                filtros.no_documento,
                filtros.referencia_01,
                filtros.referencia_02,
                filtros.referencia_03,
                filtros.caja,
                filtros.caja_apertura,
                filtros.usuarios,
                filtros.cuenta,
                filtros.forma_pago,
                filtros.forma_pago_no_ref,
                pSesion
            };

            return query;
        }

        private string BuildWhereClause(
            SifConsultaDocFiltros filtros,
            ref string[] documentos,
            ref string[] conceptos,
            ref int pSesion,
            int CodEmpresa)
        {
            var where = "";

            if (filtros.tipo_filtro == 1)
            {
                where += $@" where cliente_Identificacion like '%'+@valor_filtro+'%'";
            }
            else if (filtros.tipo_filtro == 2)
            {
                where += $@" where Cliente_Nombre like  '%'+@valor_filtro+'%'";
            }

            if (filtros.lista_documentos != null)
            {
                documentos = filtros.lista_documentos
                   .Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => s.Trim())
                   .Distinct()
                   .ToArray();

                where += $@" and Tipo_Documento IN @Listadocumentos";
            }

            if (filtros.lista_conceptos != null)
            {
                conceptos = filtros.lista_conceptos
               .Split(',', StringSplitOptions.RemoveEmptyEntries)
               .Select(s => s.Trim())
               .Distinct()
               .ToArray();
                where += $@" and Cod_Concepto in @Listaconceptos";
            }

            where += BuildDateAndStateClause(filtros);

            where += $@" and Traslado_Bloqueo = @chk_documentos_bloqueados";

            where += BuildOptionalFiltersClause(filtros);

            if (int.TryParse(filtros.valor_sesion, out _))
            {
                pSesion = Convert.ToInt32(filtros.valor_sesion);

                if (filtros.sesion == "ROE")
                {
                    int resultado = cboSesionConsultar(CodEmpresa, pSesion);
                    if (resultado > 0)
                    {
                        pSesion = resultado;
                    }
                }
                where += $@" and ID_SESION = @pSesion ";
            }

            return where;
        }

        private static string BuildDateAndStateClause(SifConsultaDocFiltros filtros)
        {
            var clause = "";

            if (filtros.tipo_fecha == "Registro")
            {
                clause += $@" and Registro_fecha between @fecha_inicio  and  @fecha_corte ";
            }
            else if (filtros.tipo_fecha == "Anulación")
            {
                clause += $@" and anulacion_fecha between @fecha_inicio  and  @fecha_corte ";
            }
            else if (filtros.tipo_fecha == "Traslado")
            {
                clause += $@" and traspaso_fecha between @fecha_inicio  and  @fecha_corte ";
            }

            if (filtros.tipo_estado == "Impreso")
            {
                clause += $@" and estado in('I','E')";
            }
            else if (filtros.tipo_estado == "Pendiente")
            {
                clause += $@"  and estado = 'P' ";
            }

            return clause;
        }

        private static string BuildOptionalFiltersClause(SifConsultaDocFiltros filtros)
        {
            var clause = "";

            clause += BuildUsuarioRegistraClause(filtros);
            clause += BuildNoTransaccionClause(filtros);
            clause += BuildNoDocumentoClause(filtros);
            clause += BuildReferenciaClause(filtros);
            clause += BuildCajaClause(filtros);
            clause += BuildUsuariosClause(filtros);
            clause += BuildCuentaClause(filtros);
            clause += BuildFormaPagoClause(filtros);
            clause += BuildAsientosDesbalanceadosClause(filtros);

            return clause;
        }

        private static string BuildUsuarioRegistraClause(SifConsultaDocFiltros filtros)
        {
            return !string.IsNullOrWhiteSpace(filtros.usuario_registra)
                ? $@" and Registro_Usuario like '%'+@usuario_registra+'%'"
                : "";
        }

        private static string BuildNoTransaccionClause(SifConsultaDocFiltros filtros)
        {
            return !string.IsNullOrWhiteSpace(filtros.no_transaccion)
                ? $@" and Cod_Transaccion like '%'+@no_transaccion+'%' "
                : "";
        }

        private static string BuildNoDocumentoClause(SifConsultaDocFiltros filtros)
        {
            return !string.IsNullOrWhiteSpace(filtros.no_documento)
                ? $@" and Cod_Transaccion like '%'+@no_documento+'%'"
                : "";
        }

        private static string BuildReferenciaClause(SifConsultaDocFiltros filtros)
        {
            var clause = "";
            if (!string.IsNullOrWhiteSpace(filtros.referencia_01))
            {
                clause += $@" and Referencia_01 like '%'+@referencia_01+'%'";
            }
            if (!string.IsNullOrWhiteSpace(filtros.referencia_02))
            {
                clause += $@" and Referencia_02 like '%'+@referencia_02+'%'";
            }
            if (!string.IsNullOrWhiteSpace(filtros.referencia_03))
            {
                clause += $@" and Referencia_03 like '%'+@referencia_03+'%'";
            }
            return clause;
        }

        private static string BuildCajaClause(SifConsultaDocFiltros filtros)
        {
            var clause = "";
            if (filtros.caja != null && filtros.caja.Trim() != "TODOS")
            {
                clause += $@" and cod_caja= @caja";
                if (filtros.caja_apertura > 0)
                {
                    clause += $@" and cod_Apertura= @caja_apertura";
                }
            }
            return clause;
        }

        private static string BuildUsuariosClause(SifConsultaDocFiltros filtros)
        {
            return (filtros.usuarios != null && filtros.usuarios.Trim() != "" && filtros.usuarios.Trim() != "TODOS")
                ? $@" and Registro_Usuario= @usuarios"
                : "";
        }

        private static string BuildCuentaClause(SifConsultaDocFiltros filtros)
        {
            return (filtros.cuenta != null && filtros.cuenta.Trim() != "")
                ? $@" and  dbo.fxSIFDocsCuentaExiste(Tipo_Documento,Cod_Transaccion, @cuenta)  = 1"
                : "";
        }

        private static string BuildFormaPagoClause(SifConsultaDocFiltros filtros)
        {
            return (filtros.forma_pago != null && filtros.forma_pago.Trim() != "" && filtros.forma_pago.Trim() != "TODOS")
                ? $@" and dbo.fxSIFDocsFormaPagoExiste(Tipo_Documento,Cod_Transaccion,@forma_pago,@forma_pago_no_ref) = 1"
                : "";
        }

        private static string BuildAsientosDesbalanceadosClause(SifConsultaDocFiltros filtros)
        {
            return (filtros.chk_asientos_desbalanceados ?? false)
                ? $@" and dbo.fxSIFDocsAsientoBalanceado(Tipo_Documento,Cod_Transaccion) = 0"
                : "";
        }

        private static string AppendOrderAndPaging(string query, SifConsultaDocFiltros filtros, bool esExportar)
        {
            var order = $@"  order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}";
            if (!esExportar)
            {
                order += $@"
                                 OFFSET {filtros.pagina} ROWS 
                                 FETCH NEXT {filtros.paginacion} ROWS ONLY";
            }
            return query + order;
        }

        /// <summary>
        /// Consulta la sesion de una caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int cboSesionConsultar(int CodEmpresa, int valor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            int result = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"select ISNULL(ID_SESION,0) AS 'ID_SESION' from CAJAS_ROE WHERE ID_ROE =@valor";

                    result = connection.Query<int>(query, new { valor }).FirstOrDefault();

            }
            catch (Exception)
            {
                result = 0;
            }

            return result;

        }


        /// <summary>
        /// * Ejecuta la generación del reporte de un documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public ErrorDto<object> SifConsultaDocumentos_Reporte(int CodEmpresa, string usuario, string tipoDocumento, string transaccion)
        {
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {

                response = new MRecibos(_config).sbImprimeRecibo(CodEmpresa, transaccion, tipoDocumento, usuario);
                if (response.Code != -1)
                {
                    response.Description = "Proceso Aplicado Satisfactoriamente...";
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

    }
}