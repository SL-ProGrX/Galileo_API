using Dapper; 
using PgxAPI.Models; 
using PgxAPI.Models.ERROR; 
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_Tipo_DocumentoDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSIF_Tipo_DocumentoDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta un tipo de documento segun el order (ultimo o primero)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<string> SIF_tipoDocumento_Consultar(int CodEmpresa, string tipoDocumento, int orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {


                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";

                    if (orden == 1)
                    {
                        query = $@"select Top 1 tipo_documento from sif_documentos  where tipo_documento > @tipoDocumento order by tipo_documento asc";
                    }
                    else
                    {
                        query = $@"select Top 1 tipo_documento from sif_documentos  where tipo_documento < @tipoDocumento order by tipo_documento desc";
                    }

                    result.Result = connection.QueryFirstOrDefault<string>(query, new { tipoDocumento });

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
        /// Consulta el detalle de un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// <returns></returns>
        public ErrorDto<SifTipoDocumentoData> SIF_tipoDocumentoData_Consultar(int CodEmpresa, string tipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifTipoDocumentoData>
            {
                Code = 0,
                Description = "Ok",
                Result = new SifTipoDocumentoData()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from vSys_Documentos_Tipos where Tipo_documento =  @tipoDocumento ";
                    result.Result = connection.QueryFirstOrDefault<SifTipoDocumentoData>(query, new { tipoDocumento });

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
        /// Consulta todos los documentos existentes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SIF_tipoDocumento_Obtener(int CodEmpresa)
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
                {
                    var query = $@"select Tipo_documento as 'item',descripcion as 'descripcion' from sif_documentos ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Actualiza los datos de un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDoc"></param>
        /// <returns></returns>
        public ErrorDto SIF_tipoDocumento_Actualiza(int CodEmpresa, string usuario, SifTipoDocumentoData tipoDoc)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE sif_documentos
                                    SET   descripcion = @descripcion
                                        , tipo_movimiento = @tipo_movimiento
                                        , activo = @activo
                                        , Tipo_asiento = @tipo_asiento
                                        , cod_cuenta = @cod_cuenta
                                        , asiento_transaccion =@asiento_transaccion
                                        , asiento_mascara = @asiento_mascara
                                        ,asiento_modulo =  @asiento_modulo
                                        , formato_salida = @formato_salida_id
                                        , impuesto_registra =@impuesto_registra
                                        , Impuesto_porcentaje = @impuesto_porcentaje
                                        , Impuesto_cod_cuenta =@impuesto_cod_cuenta
                                        , tipo_comprobante = @tipo_comprobante
                                        , archivo_per =@archivo_per
                                        ,Permite_Reversion = @permite_reversion
                                        ,APLICA_CIERRE_ESPECIAL = @aplica_cierre_especial
                                        ,REVERSION_DIAS_AUTORIZADOS = @reversion_dias_autorizados
                                         where Tipo_documento =@tipo_documento";

                    connection.Execute(query, new
                    {
                        descripcion = tipoDoc.descripcion.Trim(),
                        tipo_movimiento = tipoDoc.tipo_movimiento,
                        activo = tipoDoc.activob ? 1 : 0,
                        tipo_asiento = tipoDoc.tipo_asiento,
                        cod_cuenta = tipoDoc.cod_cuenta,
                        asiento_transaccion = tipoDoc.asiento_transaccionb ? 1 : 0,
                        asiento_mascara = tipoDoc.asiento_mascara,
                        asiento_modulo = tipoDoc.asiento_modulob ? 1 : 0,
                        formato_salida_id = tipoDoc.formato_salida,
                        impuesto_registra = tipoDoc.impuesto_registrab ? 1 : 0,
                        impuesto_porcentaje = tipoDoc.impuesto_porcentaje,
                        impuesto_cod_cuenta = tipoDoc.impuesto_cod_cuenta,
                        tipo_comprobante = tipoDoc.tipo_comprobante,
                        archivo_per = tipoDoc.archivo_per,
                        permite_reversion = tipoDoc.permite_reversionb ? 1 : 0,
                        aplica_cierre_especial = tipoDoc.aplica_cierre_especialb ? 1 : 0,
                        reversion_dias_autorizados = tipoDoc.reversion_dias_autorizados,
                        tipo_documento = tipoDoc.tipo_documento


                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Documento : {tipoDoc.tipo_documento}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Inserta un nuevo tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDoc"></param>
        /// <returns></returns>
        public ErrorDto SIF_tipoDocumento_Insertar(int CodEmpresa, string usuario, SifTipoDocumentoData tipoDoc)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    //verifico si existe tipo
                    var queryexist = $@"select isnull(count(*),0) as Existe from SIF_DOCUMENTOS   where Tipo_documento =@tipoDoc ";
                    var existe = connection.QueryFirstOrDefault<int>(queryexist, new { tipoDoc = tipoDoc.tipo_documento.Trim() });

                    if (existe > 0)
                    {
                        info.Code = -2;
                        return info;
                    }
                    
                    var query = $@"INSERT INTO SIF_DOCUMENTOS (
                                     TIPO_DOCUMENTO,descripcion,consecutivo,tipo_comprobante,tipo_movimiento,tipo_asiento
                                    ,cod_cuenta,activo,asiento_transaccion,asiento_mascara,asiento_modulo,formato_salida,impuesto_registra,Impuesto_porcentaje
                                    ,impuesto_cod_cuenta,registro_fecha,registro_usuario,archivo_per,Permite_Reversion,APLICA_CIERRE_ESPECIAL,REVERSION_DIAS_AUTORIZADOS)
                                    VALUES  (@tipo_documento, @descripcion,@consecutivo,@tipo_comprobante, @tipo_movimiento,@tipo_asiento,
                                    @cod_cuenta,@activo,@asiento_transaccion,@asiento_mascara,@asiento_modulo,@formato_salida_id,@impuesto_registra,@impuesto_porcentaje,
                                    @impuesto_cod_cuenta,dbo.MyGetDate(), @usuario,@archivo_per,@permite_reversion,@aplica_cierre_especial,@reversion_dias_autorizados  ) ";

                    connection.Execute(query, new
                    {
                        tipo_documento = tipoDoc.tipo_documento,
                        descripcion = tipoDoc.descripcion.Trim(),
                        consecutivo = tipoDoc.consecutivo,
                        tipo_comprobante = tipoDoc.tipo_comprobante,
                        tipo_movimiento = tipoDoc.tipo_movimiento,
                        tipo_asiento = tipoDoc.tipo_asiento,
                        cod_cuenta = tipoDoc.cod_cuenta,
                        activo = tipoDoc.activo,    
                        asiento_transaccion = tipoDoc.asiento_transaccion,
                        asiento_mascara = tipoDoc.asiento_mascara,
                        asiento_modulo = tipoDoc.asiento_modulo,
                        formato_salida_id = tipoDoc.formato_salida,
                        impuesto_registra = tipoDoc.impuesto_registra,
                        impuesto_porcentaje = tipoDoc.impuesto_porcentaje,
                        impuesto_cod_cuenta = tipoDoc.impuesto_cod_cuenta,                        
                        archivo_per = tipoDoc.archivo_per,
                        permite_reversion = tipoDoc.permite_reversion,
                        aplica_cierre_especial = tipoDoc.aplica_cierre_especial,
                        reversion_dias_autorizados = tipoDoc.reversion_dias_autorizados,
                        usuario= usuario

                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Documento : {tipoDoc.tipo_documento}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de beneficio ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }

        /// <summary>
        /// Consulta los conceptos relacionados a un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDoc"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTipoDocConceptoData>> SIF_TipoDocumentosConceptosRelacionados_Obtener(int CodEmpresa, string tipoDoc)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifTipoDocConceptoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifTipoDocConceptoData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"Select C.cod_concepto,C.descripcion,X.cod_Concepto as Asignado 
                                    from sif_conceptos C left join sif_conceptos_documento X
                                    on C.cod_concepto = X.cod_concepto and X.Tipo_documento =@tipoDoc 
                                    order by X.cod_Concepto desc,C.cod_concepto asc";

                    result.Result = connection.Query<SifTipoDocConceptoData>(query, new { tipoDoc }).ToList();
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
        /// Inserta o elimina un concepto de un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_concepto"></param>
        /// <param name="tipoDoc"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto SIF_TipoDocumentosConceptosRelacionados_Guardar(int CodEmpresa, string usuario, string cod_concepto, string tipoDoc, string accion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);        
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";

                    if (accion=="D")
                    {
                         query = $@"Delete sif_conceptos_documento where cod_concepto = @cod_concepto and tipo_documento = @tipo_documento";
                    }
                    else
                    {
                         query = $@"insert into sif_conceptos_documento(cod_concepto,tipo_documento,registro_fecha,registro_usuario)
                                    values(@cod_concepto,@tipo_documento,dbo.MyGetdate(),@usuario)";
                    }
                  
                    connection.Execute(query,
                         new
                         {
                             cod_concepto = cod_concepto.Trim(),
                             tipo_documento= tipoDoc.Trim(),
                             usuario = usuario,
                            
                         });

              
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
