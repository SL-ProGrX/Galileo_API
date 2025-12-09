using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransaccionesDb
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria mTesoreria;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly FrmCntXConsultaCuentasDb _ConsultaCuentasDB;

        private readonly VerificadorCoreFactory _factory;


        public FrmTesTransaccionesDb(IConfiguration config)
        {
            _config = config;
            mTesoreria = new MTesoreria(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _ConsultaCuentasDB = new FrmCntXConsultaCuentasDb(config);

            _factory = new VerificadorCoreFactory(config);
        }

        /// <summary>
        /// Método para obtener los tipos de documentos desde modulo principal
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocumentos_Obtener(int CodEmpresa, string Usuario, int id_banco, string? tipo = "S")
        {
            return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, Usuario, id_banco, tipo);
        }

        /// <summary>
        /// Método para obtener las unidades disponibles para el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Unidades_Obtener(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            return mTesoreria.sbTesUnidadesCargaCbo(CodEmpresa, usuario, banco, contabilidad);
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
        /// Método para obtener los conceptos de carga desde modulo principal
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return mTesoreria.sbTesConceptosCargaCbo(CodEmpresa, usuario, banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosCarga_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        /// <summary>
        /// Método para obtener Afectaciones de la transacción 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreria"></param>
        /// <returns></returns>
        public ErrorDto<List<TesAfectacionDto>> TES_Afectaciones_Obtener(int CodEmpresa, int tesoreria)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesAfectacionDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Consulta_Afectacion_Modulos @Solicitud";
                    response.Result = connection.Query<TesAfectacionDto>(query, new { Solicitud = tesoreria }).ToList();
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
        /// Método para obtener bitácora de transacciones por numero de solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreria"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBitacoraDto>> TES_Bitacora_Obtener(int CodEmpresa, int tesoreria)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesBitacoraDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select H.ID, H.FECHA, H.USUARIO,ISNULL(M.DESCRIPCION,'No identificado') AS MOVIMIENTO,H.DETALLE
                    from TES_HISTORIAL H left join TES_TIPOS_MOVIMIENTOS M on H.COD_MOVIMIENTO = M.COD_MOVIMIENTO
                    WHERE H.NSOLICITUD = @Solicitud";
                    response.Result = connection.Query<TesBitacoraDto>(query, new { Solicitud = tesoreria }).ToList();
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
        /// Método para obtener el siguiente o anterior registro de la tabla Tes_Transacciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codigo"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<int> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string codigo, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 nsolicitud from Tes_Transacciones C
                                  inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad";

                    switch (scrollCode)
                    {
                        case 0:
                            if (codigo == "")
                            {
                                codigo = "0";
                            }
                            query += $@" where C.nsolicitud > {codigo} AND U.cod_contabilidad = {contabilidad}  order by C.nsolicitud asc";

                            break;
                        case 1:
                            if (codigo == "0")
                            {
                                codigo = "999999999";
                            }

                            query += $@" where C.nsolicitud < {codigo} AND U.cod_contabilidad = {contabilidad} order by C.nsolicitud desc";

                            break;
                        default:
                            break;
                    }

                    response.Result = connection.Query<int>(query).FirstOrDefault();
                    if (response.Result == 0)
                    {
                        TES_Transaccion_Scroll(CodEmpresa, scrollCode, response.Result.ToString(), contabilidad);
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }

            return response;
        }

        /// <summary>
        /// Método para extraer los datos de la solicitud.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreria"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<TesTransaccionDto> TES_Transaccion_Obtener(int CodEmpresa, int tesoreria, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesTransaccionDto>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Transaccion_Consulta @Solicitud ";
                    response.Result = connection.Query<TesTransaccionDto>(query, new { Solicitud = tesoreria }).FirstOrDefault();

                    if (response.Result != null)
                    {
                        response.Result.detalle = string.Join(" ",
                                                    response.Result.detalle1 ?? "",
                                                    response.Result.detalle2 ?? "",
                                                    response.Result.detalle3 ?? "",
                                                    response.Result.detalle4 ?? "",
                                                    response.Result.detalle5 ?? ""
                                                ).Replace("null", "").Trim();
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
        /// Método para obtener el detalle del asiento de la transacción seleccionada. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<TesTransAsientoDto>> TES_TransaccionAsiento_Obtener(
            TesConsultaAsientos vSolicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(vSolicitud.CodEmpresa);
            var response = new ErrorDto<List<TesTransAsientoDto>>
            {
                Code = 0
            };
            try
            {
                var query = "";
                var vEstado = "P";
                using var connection = new SqlConnection(stringConn);
                {

                    if (vSolicitud.solicitud > 0)
                    {
                        query = $@"select C.cod_cuenta_Mask as 'Cod_Cuenta',C.descripcion,D.debehaber,D.monto,D.cod_unidad,Ch.Estado
                          ,U.descripcion as UnidadX,D.cod_cc,X.descripcion as CCX,Ch.id_Banco,D.tipo_cambio,D.cod_divisa
                           from Tes_Trans_Asiento D inner join Tes_Transacciones Ch on D.nsolicitud = Ch.Nsolicitud
                           inner join CntX_Cuentas C on D.cuenta_contable = C.cod_cuenta and C.cod_contabilidad = @contabilidad
                           left join CntX_unidades U on D.cod_unidad = U.cod_unidad and U.cod_contabilidad =  @contabilidad
                           left join CNTX_CENTRO_COSTOS X on D.cod_cc = X.COD_CENTRO_COSTO and X.cod_contabilidad =  @contabilidad
                           where D.nsolicitud = @solicitud 
                           order by D.linea";

                        response.Result = connection.Query<TesTransAsientoDto>(query,
                        new
                        {
                            contabilidad = vSolicitud.contabilidad,
                            solicitud = vSolicitud.solicitud
                        }).ToList();
                        int count = 0;
                        foreach (var item in response.Result)
                        {
                            if (count == 0)
                            {
                                if (vSolicitud.id_banco != item.id_banco)
                                {
                                    query = $@"select C.cod_cuenta_Mask as 'Cod_Cuenta',C.descripcion, C.cod_divisa
                                            from CntX_Cuentas C inner join Tes_Bancos B on C.cod_Cuenta = B.CtaConta
                                            Where B.id_banco = @banco
                                            and C.cod_contabilidad = @contabilidad";
                                    TesTransAsientoDto linea = connection.QueryFirstOrDefault<TesTransAsientoDto>(
                                        query, new
                                        {
                                            banco = vSolicitud.id_banco,
                                            contabilidad = vSolicitud.contabilidad
                                        });

                                    item.cod_cuenta = linea.cod_cuenta;
                                    item.cod_divisa = linea.cod_divisa;
                                    item.descripcion = linea.descripcion;
                                }
                            }
                            count++;
                        }
                    }
                    else
                    {
                        query = $@"select TOP 1 C.cod_cuenta_Mask as 'Cod_Cuenta',C.descripcion, 'D' as debehaber,  {vSolicitud.monto} as monto , 
                                    '{vSolicitud.cod_unidad}' as cod_uniadd, '{vSolicitud.estado}' as estado ,
                                    ( select descripcion from CntX_Unidades U where U.cod_unidad = '{vSolicitud.cod_unidad}' and U.cod_contabilidad = C.cod_contabilidad) as UnidadX,
                                    '' as cod_cc, '' as CCX, B.id_banco, 0 as tipo_cambio,
                                    C.cod_divisa
                                                from CntX_Cuentas C inner join Tes_Bancos B on C.cod_Cuenta = B.CtaConta
                                                Where B.id_banco = {vSolicitud.id_banco}
                                                and C.cod_contabilidad = @contabilidad
                                     UNION 
                                  select TOP 1 C.cod_cuenta_Mask as 'Cod_Cuenta',C.descripcion, 'H' as debehaber, {vSolicitud.monto} as monto , 
                                     '{vSolicitud.cod_unidad}' as cod_uniadd, '{vSolicitud.estado}' as estado ,
                                    ( select descripcion from CntX_Unidades U where U.cod_unidad = '{vSolicitud.cod_unidad}' and U.cod_contabilidad = C.cod_contabilidad) as UnidadX,
                                    '' as cod_cc, '' as CCX, {vSolicitud.id_banco} as id_banco , 0 as tipo_cambio,
                                     C.cod_divisa
                                                from CntX_Cuentas C inner join Tes_Conceptos B on C.cod_Cuenta = B.cod_cuenta
                                                Where B.cod_concepto = '{vSolicitud.cod_concepto}'
                                                and C.cod_contabilidad = @contabilidad ";


                        response.Result = connection.Query<TesTransAsientoDto>(query,
                        new
                        {
                            contabilidad = vSolicitud.contabilidad,
                        }).ToList();

                        int count = 0;
                        foreach (var item in response.Result)
                        {
                            if (count == 0)
                            {
                                if (vSolicitud.id_banco != item.id_banco)
                                {
                                    query = $@"select C.cod_cuenta_Mask as 'Cod_Cuenta',C.descripcion, C.cod_divisa
                                            from CntX_Cuentas C inner join Tes_Bancos B on C.cod_Cuenta = B.CtaConta
                                            Where B.id_banco = @banco
                                            and C.cod_contabilidad = @contabilidad";
                                    TesTransAsientoDto linea = connection.QueryFirstOrDefault<TesTransAsientoDto>(
                                        query, new
                                        {
                                            banco = vSolicitud.id_banco,
                                            contabilidad = vSolicitud.contabilidad
                                        });

                                    item.cod_cuenta = linea.cod_cuenta;
                                    item.cod_divisa = linea.cod_divisa;
                                    item.descripcion = linea.descripcion;
                                }
                            }
                            count++;
                        }

                        foreach (var item in response.Result)
                        {
                            item.cod_unidad = vSolicitud.cod_unidad;

                            if (item.cod_divisa == "DOL")
                            {
                                item.tipo_cambio = Convert.ToDecimal(vSolicitud.tipoCambio);
                            }
                            else
                            {
                                item.tipo_cambio = 1;
                            }
                            item.monto = item.monto * Convert.ToDecimal(vSolicitud.tipoCambio);
                        }

                    }

                    if (mTesoreria.fxTesTiposDocAsiento(vSolicitud.CodEmpresa, vSolicitud.tipo) == "A")
                    {
                        response.Result[0].debehaber = "H";
                        if (response.Result.Count > 1)
                        {
                            for (int i = 0; i < response.Result.Count; i++)
                            {
                                if (i >= 1)
                                {
                                    response.Result[i].debehaber = "D";
                                }
                            }
                        }
                    }
                    else
                    {
                        response.Result[0].debehaber = "D";
                        if (response.Result.Count > 1)
                        {
                            for (int i = 0; i < response.Result.Count; i++)
                            {
                                if (i >= 1)
                                {
                                    response.Result[i].debehaber = "H";
                                }
                            }
                        }
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
        /// Método para obtener la localización de la remesa por solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<TesLocalizacionDto>> TES_Localizacion_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesLocalizacionDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.fecha_rec,D.cod_remesa,U.descripcion,D.usuario_rec,D.observacion
                                    from Tes_Ubi_RemDet D inner join Tes_ubi_Remesa R on D.cod_Remesa = R.cod_remesa
                                    inner join tes_Ubicaciones U on R.cod_ubicacion_destino = U.cod_ubicacion
                                    Where D.nsolicitud = @solicitud And D.estado = 1
                                    Order by D.fecha_rec desc";
                    response.Result = connection.Query<TesLocalizacionDto>(query,
                        new
                        {
                            solicitud = solicitud
                        }).ToList();
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
        /// Método para obtener la bitácora de reimpresiones por solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<TesReimpresionesDto>> TES_ReImpresiones_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesReimpresionesDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Fecha,Usuario,Autoriza,Notas from Tes_reImpresiones where nsolicitud = @solicitud
                                      order by fecha desc";
                    response.Result = connection.Query<TesReimpresionesDto>(query,
                        new
                        {
                            solicitud = solicitud
                        }).ToList();
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
        /// Método para obtener la bitácora de cambios de fechas por solicitud  
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<TesCambioFechasDto>> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesCambioFechasDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Id as Idx,Fecha,Usuario,Detalle from tes_historial where nsolicitud = @solicitud
                                    and cod_movimiento = '08' order by fecha desc ";

                    response.Result = connection.Query<TesCambioFechasDto>(query,
                        new
                        {
                            solicitud = solicitud
                        }).ToList();
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
        /// Método para obtener la lista de solicitudes de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, int contabilidad, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco Total
                    var query = $@"select count(NSOLICITUD) from Tes_Transacciones C
                                  inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad WHERE U.cod_contabilidad = @Contabilidad";
                    response.Result.total = connection.Query<int>(query, new { Contabilidad = contabilidad }).FirstOrDefault();

                    if (filtro.filtro != null && filtro.filtro != "")
                    {
                        filtro.filtro = $@"WHERE ( 
                                                 NSOLICITUD like '%{filtro.filtro}%' 
                                              OR TIPO like '%{filtro.filtro}%'
                                              OR BENEFICIARIO like '%{filtro.filtro}%'
                                              OR CODIGO like '%{filtro.filtro}%' 
                                          )";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "NSOLICITUD";
                    }

                    if (filtro.sortOrder == 0)
                    {
                        filtro.sortOrder = 1; //Por defecto orden ascendente
                    }

                    if (filtro.pagina != null)
                    {
                        query = $@"select NSOLICITUD, TIPO, CODIGO, BENEFICIARIO, MONTO, ESTADO, COD_UNIDAD FROM (
                                      select C.NSOLICITUD, rtrim(T.descripcion) as TIPO, C.CODIGO , C.BENEFICIARIO , C.monto, C.estado, C.COD_UNIDAD  
                                  from Tes_Transacciones C 
                                       inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad  
                                       inner join Tes_Tipos_doc T on C.tipo = T.tipo
                                  WHERE U.cod_contabilidad = {contabilidad}
                                  ) X {filtro.filtro} ORDER BY {filtro.sortField} {(filtro.sortOrder == -1 ? "ASC" : "DESC")}  
                                      OFFSET {filtro.pagina} ROWS
                                      FETCH NEXT {filtro.paginacion} ROWS ONLY ";

                        response.Result.lista = connection.Query<Galileo.Models.ProGrX.Bancos.TesSolicitudesData>(query).ToList();
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
        /// Método para obtener solicitud por documento por scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<int> TES_TransaccionDocumento_Scroll(int CodEmpresa, int scrollCode, TesSolicitudDocParametro parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 ndocumento from Tes_Transacciones 
                                     where id_banco = @idBanco and Tipo = @Tipo";

                    switch (scrollCode)
                    {
                        case 0:
                            if (parametros.documento == "")
                            {
                                parametros.documento = "0";
                            }

                            query += $@" and TRY_CAST(ndocumento AS INT) > '{parametros.documento}' 
                                         order by TRY_CAST(ndocumento AS INT) asc";

                            break;
                        case 1:
                            if (parametros.documento == "")
                            {
                                parametros.documento = "999999999";
                            }

                            query += $@" and TRY_CAST(ndocumento AS INT) < '{parametros.documento}' 
                                           order by TRY_CAST(ndocumento AS INT) desc";

                            break;
                        default:
                            break;
                    }

                    var documento = connection.Query<string>(query, new { idBanco = parametros.id_banco, Tipo = parametros.tipo }).FirstOrDefault();

                    response = TES_TransaccionDoc_Obtener(
                        CodEmpresa,
                        documento,
                        parametros.id_banco,
                        parametros.tipo,
                        parametros.contabilidad);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }

            return response;
        }

        /// <summary>
        /// Método para obtener numero de solicitud por documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <param name="banco"></param>
        /// <param name="tipo"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<int> TES_TransaccionDoc_Obtener(
            int CodEmpresa,
            string documento,
            int banco,
            string tipo,
            int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Nsolicitud from Tes_Transacciones where 
                                    ndocumento = @Documento  and id_banco = @idBanco and Tipo = @Tipo ";
                    response.Result = connection.Query<int>(query, new {Documento = documento, idBanco = banco, Tipo = tipo }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }

            return response;
        }


        /// <summary>
        /// Método para aplicar el cambio de cuenta bancaria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="solicitud"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto TES_CambioCuentaBancaria_Aplicar(int CodEmpresa, string usuario, int solicitud, string cuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"exec spTes_Cuenta_Bancaria_Cambio @Solicitud, @Cuenta, @Usuario";

                response.Code = connection.Execute(query, new { Solicitud = solicitud, Cuenta = cuenta, Usuario = usuario });
                if (response.Code == 0)
                {
                    response.Description = "No se pudo realizar el cambio de cuenta bancaria";
                }
                else
                {
                    response.Description = "Cambio de Cuenta Bancaria realizado satisfactoriamente!";
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
        /// Método para guardar la transacción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public ErrorDto TES_Transaccion_Guardar(int CodEmpresa, string usuario, int contabilidad, TesTransaccionDto transaccion)
        {
            // TES_TransaccionDto transacción = JsonConvert.DeserializeObject<TES_TransaccionDto>(jTransaccion);
            var res = new ErrorDto();
            if (transaccion.user_solicita == null)
            {
                transaccion.user_solicita = usuario;
            }

            //valida si el asiento es nulo para crearlo
            if (transaccion.asientoDetalle == null)
            {
                var solicitud = new TesConsultaAsientos();
                solicitud.CodEmpresa = CodEmpresa;
                solicitud.solicitud = transaccion.nsolicitud;
                solicitud.contabilidad = contabilidad;
                solicitud.tipoCambio = float.Parse(transaccion.tipo_cambio.ToString());
                solicitud.divisa = transaccion.cod_divisa;
                solicitud.estado = transaccion.estado;
                solicitud.monto = transaccion.monto.Value;
                solicitud.id_banco = transaccion.id_banco;
                solicitud.cod_unidad = transaccion.cod_unidad;
                solicitud.cod_concepto = transaccion.cod_concepto;
                solicitud.tipo = transaccion.tipo;

                transaccion.asientoDetalle = TES_TransaccionAsiento_Obtener(solicitud).Result;

            }

            string[] resultado = DividirEnCincoPartes(transaccion.detalle);

            transaccion.detalle1 = resultado[0];
            transaccion.detalle2 = resultado[1];
            transaccion.detalle3 = resultado[2];
            transaccion.detalle4 = resultado[3];
            transaccion.detalle5 = resultado[4];

            transaccion.detalle = null;



            var valida = fxValida(CodEmpresa, usuario, transaccion);
            if (valida.Code == -1)
            {
                res.Code = valida.Code;
                res.Description = valida.Description;
                return res;
            }

            var idOrigen = transaccion.tipo_beneficiario - 1;
            // Fix for CS0266: Explicitly cast 'int?' to 'int' to resolve the type mismatch.
            transaccion.tipo_ced_origen = idOrigen.HasValue ? idOrigen.Value : default;

            var banco = mTesoreria.fxTesBancoDocsValor(CodEmpresa, transaccion.id_banco, transaccion.tipo, "REG_AUTORIZACION");
            if (banco.Code != -1)
            {
                transaccion.entregado = "N";
                if (banco.Result == "1")
                {
                    transaccion.autoriza = "N";
                    transaccion.fecha_autorizacion = null;
                    transaccion.user_autoriza = null;
                }
                else
                {
                    transaccion.autoriza = "S";
                    transaccion.fecha_autorizacion = DateTime.Now;
                    transaccion.user_autoriza = usuario;
                }
            }



            if (transaccion.nsolicitud == 0)
            {
                res = TES_Transaccion_Insertar(CodEmpresa, usuario, transaccion);
            }
            else
            {
                res = TES_Transaccion_Actualizar(CodEmpresa, usuario, transaccion);
            }

            if (res.Code == 1 || res.Code == 0)
            {
                var emitir = mTesoreria.fxTesBancoDocsValor(CodEmpresa, transaccion.id_banco, transaccion.tipo, "REG_EMISION").Result;
                if (emitir == "0")
                {
                    if (transaccion.nsolicitud == 0)
                    {
                        transaccion.nsolicitud = Convert.ToInt32(res.Description);
                    }
                    //Emite documento rdlc
                    res = mTesoreria.sbTesEmitirDocumento(CodEmpresa, usuario, vModulo, transaccion.nsolicitud, transaccion.ndocumento = "", null);

                    if (res.Code == -1)
                    {
                        res.Code = -3;
                        res.Description = transaccion.nsolicitud + "|" + res.Description;
                    }
                    else
                    {
                        res.Code = 0;
                        res.Description = transaccion.nsolicitud.ToString();
                    }

                    //if (!mTesoreria.fxTesTipoAccesoValida(CodEmpresa, transaccion.id_banco.ToString(), usuario, transaccion.tipo, "G").Result)
                    //{
                    //    if (res.Code == 1 || res.Code == 0)
                    //    {
                    //        res.Code = -2;
                    //    }
                    //}
                }

            }


            return res;
        }

        /// <summary>
        /// Método para insertar la transacción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        private ErrorDto TES_Transaccion_Insertar(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                string fechaAutoriza = _AuxiliarDB.validaFechaGlobal(transaccion.fecha_autorizacion);
                if (transaccion.user_autoriza == null)
                {
                    fechaAutoriza = null;
                }


                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"
                            INSERT INTO Tes_Transacciones (
                              ID_BANCO, TIPO, COD_CONCEPTO, COD_UNIDAD, CODIGO, BENEFICIARIO, MONTO, ESTADO, FECHA_SOLICITUD, USER_SOLICITA,
                              ESTADOI, MODULO, SUBMODULO, CTA_AHORROS, GENERA, ACTUALIZA, DETALLE1, DETALLE2, DETALLE3, DETALLE4, DETALLE5, REFERENCIA,
                              OP, ESTADO_ASIENTO, ENTREGADO, AUTORIZA, FECHA_AUTORIZACION, USER_AUTORIZA, NDOCUMENTO, TIPO_CAMBIO, COD_DIVISA, TIPO_BENEFICIARIO, COD_APP,
                               CORREO_NOTIFICA, TIPO_CED_ORIGEN, CTA_IBAN_ORIGEN, CEDULA_ORIGEN, tipo_ced_destino
                            )
                           OUTPUT  INSERTED.NSOLICITUD
                                VALUES (
                                    @id_banco,
                                    @tipo,
                                    @cod_concepto,
                                    @cod_unidad,
                                    @codigo,
                                    @beneficiario,
                                    @monto,
                                    'P',
                                    GETDATE(),
                                    @usuario,
                                    'P',
                                    @vModulo,
                                    'T',
                                    @cta_ahorros,
                                    'S',
                                    'N',
                                    @detalle1,
                                    @detalle2,
                                    @detalle3,
                                    @detalle4,
                                    @detalle5,
                                    @referencia,
                                    @op,
                                    'N',
                                    @entregado,
                                    @autoriza,
                                    @fecha_autorizacion ,
                                    @user_autoriza,
                                    @ndocumento,
                                    @tipo_cambio,
                                    @cod_divisa,
                                    @tipo_beneficiario,
                                    'ProGrx',
                                    @correo_notifica,
                                    @tipo_ced_origen,
                                    @cta_iban_origen,
                                    @cedula_origen,
                                    @tipo_ced_destino
                                );
                                ";


                    //VALUES  (  
                    //  '{transaccion.cod_divisa}', {transaccion.tipo_beneficiario}, 'ProGrx', 
                    //  '{transaccion.correo_notifica}', {transaccion.tipo_ced_origen}, '{transaccion.cta_iban_origen }', '{transaccion.cedula_origen}', {transaccion.tipo_ced_destino}
                    //)";

                    string vReferencia = (transaccion.referencia != null) ? transaccion.referencia.ToString() : "NULL";
                    string vOp = (transaccion.op != null) ? transaccion.op.ToString() : "NULL";

                    response.Code = connection.QuerySingle<int>(query, new
                    {
                        id_banco = transaccion.id_banco,
                        tipo = transaccion.tipo,
                        cod_concepto = transaccion.cod_concepto,
                        cod_unidad = transaccion.cod_unidad,
                        codigo = transaccion.codigo,
                        beneficiario = transaccion.beneficiario,
                        monto = transaccion.monto,
                        usuario = usuario,
                        vModulo = vModulo,
                        cta_ahorros = transaccion.cta_ahorros,
                        detalle1 = transaccion.detalle1,
                        detalle2 = transaccion.detalle2,
                        detalle3 = transaccion.detalle3,
                        detalle4 = transaccion.detalle4,
                        detalle5 = transaccion.detalle5,
                        referencia = transaccion.referencia,
                        op = transaccion.op,
                        entregado = transaccion.entregado,
                        autoriza = transaccion.autoriza,
                        fecha_autorizacion = fechaAutoriza,
                        user_autoriza = transaccion.user_autoriza,
                        ndocumento = transaccion.ndocumento,
                        tipo_cambio = transaccion.tipo_cambio,
                        cod_divisa = transaccion.cod_divisa,
                        tipo_beneficiario = transaccion.tipo_beneficiario,
                        correo_notifica = transaccion.correo_notifica,
                        tipo_ced_origen = transaccion.tipo_ced_origen,
                        cta_iban_origen = transaccion.cta_iban_origen,
                        cedula_origen = transaccion.cedula_origen,
                        tipo_ced_destino = transaccion.tipo_ced_destino
                    });



                    if (response.Code == 0)
                    {
                        response.Description = "No se pudo realizar la transacción";
                    }
                    else
                    {
                        int solicitud = (int)response.Code;
                        response.Description = solicitud.ToString();
                        response.Code = 0;

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Solicitud : {solicitud.ToString()}",
                            Movimiento = "Registra - WEB",
                            Modulo = vModulo
                        });

                        TES_TransaccionDetalleActualizar(CodEmpresa, solicitud, transaccion.asientoDetalle);
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
        /// Método para actualizar la transacción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        private ErrorDto TES_Transaccion_Actualizar(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"update Tes_Transacciones set 
                                    id_banco = @id_banco,
                                    tipo = @tipo,
                                    cod_concepto = @cod_concepto,
                                    cod_unidad = @cod_unidad,
                                    codigo = @codigo,
                                    Beneficiario = @Beneficiario,
                                    monto = @monto,
                                    cta_ahorros = @cta_ahorros,
                                    detalle1 = @detalle1,
                                    detalle2 = @detalle2,
                                    detalle3 = @detalle3,
                                    detalle4 = @detalle4,
                                    detalle5 = @detalle5, 
                                    tipo_beneficiario = @tipo_beneficiario,  
                                    tipo_cambio = @tipo_cambio,
                                    cod_divisa =  @cod_divisa,
                                    referencia = @referencia,
                                    Autoriza = @Autoriza, 
                                    fecha_autorizacion = @fecha_autorizacion,
                                    CORREO_NOTIFICA = @correo_notifica,
                                    user_autoriza = @user_autoriza,
                                    TIPO_CED_ORIGEN = @tipo_ced_origen, 
                                    CTA_IBAN_ORIGEN= @cta_iban_origen,
                                    CEDULA_ORIGEN= @cedula_origen, 
                                    tipo_ced_destino = @tipo_ced_destino
                                     where nsolicitud = @nsolicitud ";

                    response.Code = connection.Execute(query, new
                    {
                        id_banco = transaccion.id_banco,
                        tipo = transaccion.tipo,
                        cod_concepto = transaccion.cod_concepto,
                        cod_unidad = transaccion.cod_unidad,
                        codigo = transaccion.codigo,
                        beneficiario = transaccion.beneficiario,
                        monto = transaccion.monto,
                        cta_ahorros = transaccion.cta_ahorros,
                        detalle1 = transaccion.detalle1,
                        detalle2 = transaccion.detalle2,
                        detalle3 = transaccion.detalle3,
                        detalle4 = transaccion.detalle4,
                        detalle5 = transaccion.detalle5,
                        tipo_beneficiario = transaccion.tipo_beneficiario,
                        tipo_cambio = transaccion.tipo_cambio,
                        cod_divisa = transaccion.cod_divisa,
                        referencia = transaccion.referencia,
                        autoriza = transaccion.autoriza,
                        fecha_autorizacion = transaccion.fecha_autorizacion,
                        correo_notifica = transaccion.correo_notifica,
                        user_autoriza = transaccion.user_autoriza,
                        nsolicitud = transaccion.nsolicitud,
                        tipo_ced_origen = transaccion.tipo_ced_origen,
                        cta_iban_origen = transaccion.cta_iban_origen,
                        cedula_origen = transaccion.cedula_origen,
                        tipo_ced_destino = transaccion.tipo_ced_destino

                    });

                    if (response.Code == 0)
                    {
                        response.Description = "No se pudo realizar la transacción";
                    }
                    else
                    {
                        response.Description = transaccion.nsolicitud.ToString();
                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Solicitud : {transaccion.nsolicitud.ToString()}",
                            Movimiento = "Modifica - WEB",
                            Modulo = vModulo
                        });
                        TES_TransaccionDetalleActualizar(CodEmpresa, transaccion.nsolicitud, transaccion.asientoDetalle);
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
        /// Método para actualizar el detalle del asiento de la transacción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="detalle"></param>
        /// <returns></returns>
        public ErrorDto TES_TransaccionDetalleActualizar(int CodEmpresa, int solicitud, List<TesTransAsientoDto> detalle)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete Tes_Trans_Asiento where nsolicitud = @solicitud ";
                    connection.Execute(query, new
                    {
                        solicitud = solicitud
                    });
                    if (response.Code == -1)
                    {
                        response.Description = "No se pudo realizar la transacción";
                    }
                    else
                    {
                        int linea = 0;
                        foreach (var item in detalle)
                        {
                            linea++;
                            query = $@"insert Tes_Trans_Asiento(
                                        nSolicitud,
                                        Linea,
                                        Cuenta_Contable,
                                        cod_unidad,cod_cc,cod_divisa,tipo_cambio,DebeHaber,Monto) values(
                                        @nSolicitud,
                                        @linea ,
                                        @Cuenta_Contable,
                                        @cod_unidad,
                                        @cod_cc,
                                        @cod_divisa,
                                        @tipo_cambio,
                                        @DebeHaber,
                                        @Monto)";

                            response.Code = connection.Execute(query, new
                            {
                                nSolicitud = solicitud,
                                linea = linea,
                                Cuenta_Contable = item.cod_cuenta.Replace("-", ""),
                                cod_unidad = item.cod_unidad,
                                cod_cc = item.cod_cc,
                                cod_divisa = item.cod_divisa,
                                tipo_cambio = item.tipo_cambio,
                                DebeHaber = item.debehaber,
                                Monto = item.monto
                            });
                        }
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
        /// Método para emitir el documento de la transacción
        /// </summary>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto TES_TransaccionesDoc_Emite(int CodEmpresa, int id_banco, string tipo)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var banco = mTesoreria.fxTesBancoDocsValor(CodEmpresa, id_banco, tipo, "REG_EMISION");
                if (banco.Result == "0")
                {
                    //LLamo despues a reporte
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
        /// Método para eliminar la transacción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto TES_Transacciones_Eliminar(int CodEmpresa, int solicitud, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete Tes_Trans_Asiento where nsolicitud = @solicitud";
                    response.Code = connection.Execute(query, new
                    {
                        solicitud = solicitud
                    });

                    query = $@"delete Tes_Transacciones where nsolicitud = @solicitud";
                    response.Code = connection.Execute(query, new
                    {
                        solicitud = solicitud
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Solicitud : {solicitud.ToString()}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
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
        /// Método para obtener la lista de beneficiarios por tipo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_transaccionesBeneficiario_Obtener(
            int CodEmpresa, string tipo, string filtro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            response.Result.total = 0;
            response.Result.lista = new List<object>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    string where = "";

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "item";
                    }

                    switch (tipo)
                    {
                        case "1": //'Personas
                            if (filtros.filtro != "")
                            {
                                where = $@" where (Cedula like '%{filtros.filtro}%' OR nombre like '%{filtros.filtro}%')";
                            }

                            query = $@"select count(cedula) from socios ";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion, correo FROM (
                                         select cedula as item,nombre as descripcion, af_email as 'correo' from socios {where}
                                       ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                            break;
                        case "2": //'Bancos
                            if (filtros.filtro != "")
                            {
                                where = $@" where (ID_BANCO  like '%{filtros.filtro}%' OR descripcion like '%{filtros.filtro}%') AND estado = 'A'";
                            }
                            else
                            {
                                where = $@" where estado = 'A'";
                            }

                            query = $@"select count(id_banco) from tes_bancos where estado = 'A'";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM (
                                         select id_banco as item,descripcion from tes_bancos {where} 
                                       ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                            break;
                        case "3": //'Proveedores
                            if (filtros.filtro != "")
                            {
                                where = $@" where (CEDJUR like '%{filtros.filtro}%' OR descripcion like '%{filtros.filtro}%') and estado = 'A'";
                            }
                            else
                            {
                                where = $@" where estado = 'A'";
                            }

                            query = $@"select count(cod_proveedor) from cxp_proveedores ";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion, correo FROM (
                                         select CEDJUR as item,descripcion, email as 'correo' from cxp_proveedores {where} 
                                            ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                            break;
                        case "4"://'Acreedores
                            if (filtros.filtro != "")
                            {
                                where = $@" where (cod_acreedor like '%{filtros.filtro}%' OR descripcion like '%{filtros.filtro}%') and estado = 'A'";
                            }
                            else
                            {
                                where = $@" where estado = 'A'";
                            }

                            query = $@"select count(cod_acreedor) from crd_apa_acreedores where estado = 'A'";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM (
                                         select cod_acreedor as item,descripcion from crd_apa_acreedores {where} 
                                            ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                            break;
                        case "5"://'Cuentas por Cobrar
                            if (filtros.filtro != "")
                            {
                                where = $@" where (cedula like '%{filtros.filtro}%' OR nombre like '%{filtros.filtro}%')";
                            }

                            query = $@"select count(cedula) from CXC_PERSONAS";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM ( 
                                        select cedula  as item ,nombre as descripcion from CXC_PERSONAS {where} 
                                          ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                            break;
                        case "6"://'Empleados
                            if (filtros.filtro != "")
                            {
                                where = $@" where (IDENTIFICACION  like '%{filtros.filtro}%' OR Nombre_Completo like '%{filtros.filtro}%')";
                            }

                            query = $@"select count(Identificacion) from RH_PERSONAS";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM ( 
                                          select Identificacion  as item ,Nombre_Completo as descripcion  from RH_PERSONAS {where} 
                                            ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                            break;
                        case "7"://'Directos
                            if (filtros.filtro != "")
                            {
                                where = $@" where ( CODIGO like '%{filtros.filtro}%' OR Nombre_Completo like '%{filtros.filtro}%')";
                            }
                            query = $@"select count(Codigo) from vTes_Beneficiarios";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM ( 
                                          select Codigo as item,Beneficiario as descripcion from vTes_Beneficiarios {where} 
                                          ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                            break;
                        case "8"://'Desembolsos
                            if (filtros.filtro != "")
                            {
                                where = $@" where ( cedula like '%{filtros.filtro}%' OR nombre like '%{filtros.filtro}%')";
                            }
                            query = $@"select count(cedula) from vCxC_Cuentas_Desembolsos_Pendientes";
                            response.Result.total = connection.Query<int>(query).FirstOrDefault();

                            query = $@"select item, descripcion FROM ( 
                                         select cedula as item,nombre as descripcion from vCxC_Cuentas_Desembolsos_Pendientes {where} 
                                      ) t
                                                ORDER BY {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                                OFFSET {filtros.pagina} ROWS
                                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                            break;
                        default:
                            break;
                    }

                    response.Result.lista = connection.Query<object>(query).ToList();
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
        /// Método para obtener la divisa y tipo de cambio de una cuenta contable
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<TesDivisaAsiento> TES_TransaccionesDivisa_Obtener(int CodEmpresa, string cuenta, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesDivisaAsiento>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesDivisaAsiento()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_DIVISA FROM CntX_Cuentas
                                    WHERE COD_CUENTA_MASK = @cuenta AND COD_CONTABILIDAD = @contabilidad ";
                    response.Result.cod_divisa = connection.QueryFirstOrDefault<string>(query, new
                    {
                        cuenta = cuenta,
                        contabilidad = contabilidad
                    });

                    query = $@"SELECT Top 1 D.tc_compra from CNTX_DIVISAS_TIPO_CAMBIO D inner join  
                                        CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA where  D.COD_CONTABILIDAD = @cod_contabilidad
                                        and D.cod_divisa = @cod_divisa  order by corte desc";

                    response.Result.tipo_cambio = connection.QueryFirstOrDefault<float>(query, new
                    {
                        cod_contabilidad = contabilidad,
                        cod_divisa = response.Result.cod_divisa
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
        /// Método para obtener el tipo de cambio de una divisa en una contabilidad específica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_divisa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<float> TES_TransaccionesTC_Obtener(int CodEmpresa, string cod_divisa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<float>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT Top 1 D.tc_compra from CNTX_DIVISAS_TIPO_CAMBIO D inner join  
                                        CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA where  D.COD_CONTABILIDAD = @cod_contabilidad
                                        and D.cod_divisa = @cod_divisa  order by corte desc";

                    response.Result = connection.QueryFirstOrDefault<float>(query, new
                    {
                        cod_contabilidad = contabilidad,
                        cod_divisa = cod_divisa
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
        /// Método para validar una transferencia SINPE
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_TransferenciasSinpe_Valida(int CodEmpresa, int solicitud, string usuario)
        {
            //aseccs original
            //return _srvWCF.fxValidacionSinpe(CodEmpresa, solicitud.ToString(), usuario);
            var request = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                request = _factory.CrearServicio(CodEmpresa, usuario).fxValidacionSinpe(CodEmpresa, solicitud.ToString(), usuario);
            }
            catch (Exception ex)
            {
                request.Code = -1;
                request.Description = "Error al validar el servicio." + ex;
            }
            return request;
        }

        /// <summary>
        /// Método para obtener las unidades activas de una contabilidad específica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesUnidades_Obtener(int CodEmpresa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select cod_unidad as 'item',descripcion from CntX_unidades where Activa = 1 and cod_contabilidad = @contabilidad";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Método para obtener los centros de costo de una unidad específica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesCC_Obtener(int CodEmpresa, string unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    query = $@"select distinct C.COD_CENTRO_COSTO as 'item',C.descripcion
                                   from CNTX_CENTRO_COSTOS C inner join CNTX_UNIDADES_CC A on C.COD_CENTRO_COSTO = A.COD_CENTRO_COSTO
                                 and C.cod_contabilidad = A.cod_Contabilidad
                                 and A.cod_unidad = @unidad";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { unidad = unidad }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Método para obtener el control de divisas de un banco específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<TesControlDivisas> TesControlDivisas_Obtener(int CodEmpresa, int id_banco, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<TesControlDivisas>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesControlDivisas()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"SELECT isnull(D.TC_COMPRA,1) as TC_COMPRA , isnull(D.VARIACION,0) as VARIACION, B.COD_DIVISA, Di.DIVISA_LOCAL
                                , dbo.fxSys_Cadena_Capitaliza(di.DESCRIPCION) as 'DIVISA_DESC', isnull(Di.CURRENCY_SIM,B.COD_DIVISA) as 'CURRENCY_SIM'
                                 FROM   TES_BANCOS B left JOIN CNTX_DIVISAS_TIPO_CAMBIO D ON B.COD_DIVISA = D.COD_DIVISA
                                 AND D.COD_CONTABILIDAD = @contabilidad and dbo.MyGetdate() between inicio and corte 
                                 inner join CNTX_DIVISAS Di on B.COD_DIVISA = Di.COD_DIVISA
                                 where B.ID_BANCO = @banco";
                    var ctrDivisa = connection.Query<TesControlDivisasData>(query, new
                    {
                        banco = id_banco,
                        contabilidad = contabilidad
                    }).FirstOrDefault();

                    if (ctrDivisa != null)
                    {
                        resp.Result.gTipoCambio = ctrDivisa.tc_compra;
                        resp.Result.gVariacion = ctrDivisa.variacion;
                        resp.Result.gDivisaDesc = ctrDivisa.divisa_desc;
                        resp.Result.gDivisa = ctrDivisa.cod_divisa;
                        resp.Result.gDivisaCurrency = ctrDivisa.currency_sim;

                        resp.Result.pDivisaLocal = ctrDivisa.divisa_local;

                        if (resp.Result.pDivisaLocal == 0 && resp.Result.gTipoCambio == 1)
                        {
                            query = $@" SELECT Top 1 D.TC_COMPRA, D.VARIACION,X.Descripcion  from CNTX_DIVISAS_TIPO_CAMBIO D inner join  
                                        CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA where  D.COD_CONTABILIDAD = @contabilidad
                                        and D.cod_divisa = @cod_divisa order by corte desc";
                            var tcDivisa = connection.Query<TesControlDivisasData>(query, new
                            {
                                cod_divisa = resp.Result.gDivisa,
                                contabilidad = contabilidad
                            }).FirstOrDefault();

                            if (tcDivisa != null)
                            {
                                resp.Result.gTipoCambio = tcDivisa.tc_compra;
                                resp.Result.gVariacion = tcDivisa.variacion;
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Método para validar si una empresa está habilitada para realizar transacciones SINPE
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<bool> TesEmpresaSinpe_Valida(int CodEmpresa)
        {
            return mTesoreria.fxValidaEmpresaSinpe(CodEmpresa);
        }

        public ErrorDto<List<TesBitacoraTransaccion>> Tes_BitacoraTransaccion(int CodEmpresa, string solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<TesBitacoraTransaccion>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesBitacoraTransaccion>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //query = $@"select * FROM US_Bitacora where cod_empresa = @codEmpresa and modulo = @modulo 
                    //        AND detalle LIKE @detallePattern 
                    //        and app_nombre = 'ProGrX_WEB'";

                    query = $@"SELECT fecha_solicitud AS fecha_hora, tt.NSOLICITUD as cod_bitacora, UPPER( tt.USER_SOLICITA)  AS usuario, 'Solicitud' AS detalle
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                                    UNION ALL
                                    SELECT fecha_emision AS fecha_hora, tt.NSOLICITUD, UPPER( tt.USER_GENERA) , 'Emisión'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FECHA_ANULA AS fecha_hora, tt.NSOLICITUD,UPPER(  tt.USER_ANULA) , 'Anula'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FECHA_AUTORIZACION AS fecha_hora,tt.NSOLICITUD ,UPPER(  tt.USER_AUTORIZA) , 'Autoriza'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FECHA_ASIENTO  AS fecha_hora,tt.NSOLICITUD , UPPER( tt.USER_ASIENTO_EMISION)  , 'Asiento Emision'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FECHA_HOLD AS fecha_hora,tt.NSOLICITUD , UPPER( tt.USER_HOLD)   , 'Bloqueo'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FECHA_ENTREGA AS fecha_hora,tt.NSOLICITUD , UPPER( tt.USER_ENTREGA)   , 'Entrega'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.FIRMAS_AUTORIZA_FECHA AS fecha_hora,tt.NSOLICITUD , UPPER( tt.FIRMAS_AUTORIZA_USUARIO)   , 'Firma Autoriza'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.CONCILIA_FECHA AS fecha_hora,tt.NSOLICITUD , UPPER( tt.CONCILIA_USUARIO)   , 'Concilia'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    UNION ALL
                                    SELECT tt.REPOSICION_FECHA AS fecha_hora,tt.NSOLICITUD , UPPER( tt.REPOSICION_USUARIO)   , 'Reposición'
                                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD =  @solicitud
                                    ORDER BY fecha_hora desc";

                    resp.Result = connection.Query<TesBitacoraTransaccion>(query, new
                    {
                        solicitud = solicitud
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Método para convertir un número a letras
        /// </summary>
        /// <param name="numero"></param>
        /// <returns></returns>
        public ErrorDto<string> NumeroALetras(decimal numero)
        {
            return MProGrXAuxiliarDB.NumeroALetras(numero);
        }

        private ErrorDto<bool> fxValida(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            var result = new ErrorDto<bool>();
            string vMensaje = "";
            int i = 0;
            decimal curMonto = 0;
            string vTextTemp = "";
            decimal curTipoCambio = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool fxValida = true;
            try
            {
                if (mTesoreria.fxTesParametro(CodEmpresa, "12") == "S")
                {
                    //reviso si el código de id es igual al código del usuario
                    using var conn = new SqlConnection(stringConn);
                    {
                        var qUser = $@"SELECT CEDULA FROM USUARIOS WHERE UPPER(NOMBRE) like '%{usuario.ToUpper()}%'";
                        var existe = conn.QueryFirstOrDefault<string>(qUser);
                        if (existe != null && existe.Length > 0)
                        {
                            if (existe == transaccion.codigo.ToString().Trim())
                            {
                                vMensaje += "La identificación de destino no puede ser del usuario logeado.\n";
                            }
                        }
                    }
                }

                if (transaccion.monto == 0)
                {
                    vMensaje += "El monto del documento no es válido";
                }



                //Validar en los documento que se auto emiten que la fecha no corresponda a un periodo cerrado.
                if (!mTesoreria.fxTesBancoValida(CodEmpresa, transaccion.id_banco, usuario).Result)
                {
                    vMensaje += "- El Usuario Actual no esta Autorizado a utilizar este Banco...\n";
                }

                if (!mTesoreria.fxTesTipoAccesoValida(CodEmpresa, transaccion.id_banco.ToString(), usuario, transaccion.tipo, "S").Result)
                {
                    vMensaje += "- El Usuario Actual no esta Autorizado a utilizar este Tipo de Transacción...\n";
                }

                if (!mTesoreria.fxTesConceptoValida(CodEmpresa, transaccion.id_banco, usuario, transaccion.cod_concepto).Result)
                {
                    vMensaje += " - El Usuario Actual no esta Autorizado a utilizar este Concepto...\n";
                }

                if (!mTesoreria.fxTesUnidadValida(CodEmpresa, transaccion.id_banco, usuario, transaccion.cod_unidad).Result)
                {
                    vMensaje += "- El Usuario Actual no esta Autorizado a utilizar esta unidad...\n";
                }

                //Si el documento Se AutoEmite / Revisar si Tiene AutoConsecutivo / Si no es Asi validad el # de Documento
                if (mTesoreria.fxTesBancoDocsValor(CodEmpresa, transaccion.id_banco, transaccion.tipo, "REG_EMISION").Result == "0")
                {
                    if (mTesoreria.fxTesBancoDocsValor(CodEmpresa, transaccion.id_banco, transaccion.tipo, "DOC_AUTO").Result == "0")
                    {
                        string vDocumento = (transaccion.ndocumento == null) ? "" : transaccion.ndocumento;
                        if (vDocumento.Length == 0)
                        {
                            vMensaje += " - Esta Solicitud se AutoEmite / Digite el #Documento para su Emisión...\n";
                        }
                        else
                        {
                            if (!mTesoreria.fxTesDocumentoVerifica(CodEmpresa, transaccion.id_banco, transaccion.tipo, transaccion.ndocumento).Result)
                            {
                                vMensaje += " - Esta Solicitud se AutoEmite / El #Documento para su Emisión ya se encuentra registrado...\n";
                            }
                        }
                    }
                }

                if (transaccion.codigo.Length == 0)
                {
                    vMensaje += " - Código del Beneficiario no es válido ...\n";
                }

                if (transaccion.beneficiario == null || transaccion.beneficiario.Length == 0)
                {
                    vMensaje += " - Beneficiario no es válido ...\n";
                }

                if (transaccion.tipo_ced_origen == null)
                {
                    vMensaje += " - Tipo Beneficiario no es válido ...\n";
                }

                if (mTesoreria.fxTesCuentaObligatoriaVerifica(CodEmpresa, transaccion.id_banco).Result)
                {
                    if (transaccion.cta_ahorros == null || transaccion.cta_ahorros.Trim() == "")
                    {
                        vMensaje += " - La cuenta destino es requerida para este banco...\n";
                    }
                }

                transaccion.detalle = transaccion.detalle1 + transaccion.detalle2 + transaccion.detalle3 + transaccion.detalle4 + transaccion.detalle5;
                if (transaccion.detalle.Length == 0)
                {
                    vMensaje += " - El Detalle no es válido ...\n";
                }

                if (transaccion.estado != "P")
                {
                    vMensaje += "- No se puede modificar este Documento porque se encuentra Emitido o Anulado ...\n";
                }

                //verifico el valance
                decimal totalAsiento = 0;
                if (transaccion.asientoDetalle.Count == 0)
                {
                    vMensaje += " - El Asiento Contable no es válido ...\n";
                }
                else
                {
                    curMonto = 0;
                    curTipoCambio = 0;
                    foreach (var item in transaccion.asientoDetalle)
                    {
                        if (item.debehaber == "D")
                        {
                            curMonto += item.monto;
                            totalAsiento += item.monto;
                            curTipoCambio = item.tipo_cambio;
                        }
                        else
                        {
                            curMonto -= item.monto;
                            curTipoCambio = item.tipo_cambio;
                        }
                    }
                    if (curMonto != 0)
                    {
                        vMensaje += " - El Asiento Contable debe estar Balanceado ...\n";
                    }

                }

                //Valida que la Primer linea del Asiento sea igual al monto del documento
                if (transaccion.asientoDetalle[0].monto != transaccion.monto * transaccion.tipo_cambio)
                {
                    vMensaje += " -El Monto Linea 1 del Asiento no corresponde al original...\n";
                }

                if (totalAsiento != transaccion.monto * transaccion.tipo_cambio)
                {
                    vMensaje += " -El Monto del Asiento no corresponde al original...\n";
                }

                using var connection = new SqlConnection(stringConn);
                {

                    var cuentaValida = "";
                    //Valida Asiento: Cuentas, Unidad, Centros, Divisas y Tipo de Cambios
                    foreach (var item in transaccion.asientoDetalle)
                    {
                        decimal pDebito = (item.debehaber == "D") ? item.monto : 0;
                        decimal pCredito = (item.debehaber == "H") ? item.monto : 0;
                        string gEnlace = "1";
                        cuentaValida = $@"exec spCntX_Cuentas_Valida_Load {gEnlace},'{transaccion.user_solicita}','TES', '{item.cod_cuenta.Replace("-", "")}','{item.cod_divisa}','{item.cod_unidad}','{item.cod_cc}',{item.tipo_cambio},{pDebito},{pCredito},{i}";

                        var cuenta = connection.QueryFirstOrDefault(cuentaValida);
                        i++;
                    }

                    var validaresultado = $@"exec spCntX_Cuentas_Valida_Resultado '{transaccion.user_solicita}', 0";
                    var resultado = connection.QueryFirstOrDefault(validaresultado);
                    //vMensaje += resultado.RESULTADO;
                }


                if (vMensaje.Length > 0)
                {
                    result.Code = -1;
                    result.Result = false;
                    result.Description = vMensaje;
                }
                else
                {
                    result.Code = 0;
                    result.Description = "";
                    result.Result = true;
                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Description = "Error al validar la transacción";
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int banco, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "0"
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select dbo.fxTes_DocumentoAutoEmite(@banco,@tipo) as 'AutoEmite'";
                    resp.Result = connection.QueryFirstOrDefault<string>(query, new
                    {
                        banco = banco,
                        tipo = tipo
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Método para obtener las unidades activas de una contabilidad específica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<TesCuentasBancarias>> TES_TransaccionesCuentasBancarias_Obtener(int CodEmpresa, string identificacion, string banco, string? tipoOrigen = "1")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<TesCuentasBancarias>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesCuentasBancarias>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //reviso si el banco permite filtros por grupo
                    query = $@"SELECT COD_GRUPO, CTA, COD_DIVISA, INT_GRUPOS_ASOCIADOS FROM TES_BANCOS WHERE ID_BANCO = @banco";
                    var bancoInfo = connection.QueryFirstOrDefault<BancoValidaCuenta>(query, new { banco = banco });

                    switch (tipoOrigen)
                    {
                        case "1":
                            if (!bancoInfo.int_grupos_asociados)
                            {
                                query = $@"SELECT C.CUENTA_INTERNA,  rtrim(C.cod_Banco) + ' - ' + C.CUENTA_INTERNA as 'cuenta_desc' , C.CUENTA_INTERNA as 'itmx', '{bancoInfo.cta}' as 'idx'
                                          FROM SYS_CUENTAS_BANCARIAS C 
                                          INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                                          WHERE C.Identificacion = @cedula
                                          AND B.COD_GRUPO = @grupo 
                                          AND  C.COD_DIVISA = @divisa 
                                          AND  C.ACTIVA = 1";
                            }
                            else
                            {
                                query = $@"SELECT C.CUENTA_INTERNA,  rtrim(C.cod_Banco) + ' - ' + C.CUENTA_INTERNA as 'cuenta_desc'  , C.CUENTA_INTERNA as 'itmx', '{bancoInfo.cta}' as 'idx'
                                          FROM SYS_CUENTAS_BANCARIAS C 
                                          INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                                          WHERE C.Identificacion = @cedula
                                          AND B.COD_GRUPO IN (
                                          SELECT COD_GRUPO FROM TES_BANCOS_GRUPOS_ASG tbga WHERE ID_BANCO = {banco}
                                          )
                                          AND  C.COD_DIVISA = @divisa 
                                          AND  C.ACTIVA = 1 ";
                            }
                            break;
                    }



                    resp.Result = connection.Query<TesCuentasBancarias>(query, new
                    {
                        cedula = identificacion,
                        grupo = bancoInfo.cod_grupo,
                        divisa = bancoInfo.cod_divisa
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Método para obtener las cuentas contables de una empresa y un modelo de cuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            var result = new ErrorDto<CtnxCuentasDto>
            {
                Code = 1,
                Description = "Ok",
                Result = new CtnxCuentasDto()
            };

            var lista = _ConsultaCuentasDB.ObtenerCuentas(CodEmpresa, cuenta);

            result.Result = lista.Find(x => x.cod_cuenta == cuenta.Cuenta);

            return result;
        }

        public ErrorDto<TesCuentasBancarias> TES_TransaccionesCtaInterna_Obtener(int CodEmpresa, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<TesCuentasBancarias>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"SELECT B.descripcion as 'cuenta_desc', B.cta as 'cuenta_interna', ( SELECT se.CEDULA_JURIDICA  FROM SIF_EMPRESA se where portal_id = @empresa ) as 'itmx'
                                  FROM TES_BANCOS B WHERE B.ID_BANCO = @banco";
                    resp.Result = connection.QueryFirstOrDefault<TesCuentasBancarias>(query, new
                    {
                        banco = id_banco,
                        empresa = CodEmpresa
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public string[] DividirEnCincoPartes(string texto)
        {
            var partes = new string[5];
            if (texto != null)
            {
                int longitudMax = 100;

                for (int i = 0; i < 5; i++)
                {
                    int inicio = i * longitudMax;

                    if (inicio >= texto.Length)
                    {
                        partes[i] = ""; // Si ya no hay más texto, agregar string vacío
                    }
                    else
                    {
                        int longitud = Math.Min(longitudMax, texto.Length - inicio);
                        partes[i] = texto.Substring(inicio, longitud);
                    }
                }
            }


            return partes;
        }


        private decimal fxDivisaTipoCambio(int CodEmpresa, string pDivisa, string pTipo = "C")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            decimal fxDivisaTipoCambio = 0;


            return fxDivisaTipoCambio;
        }
    }
}
