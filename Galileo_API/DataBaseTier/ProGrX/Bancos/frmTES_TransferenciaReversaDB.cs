using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_TransferenciaReversaDB
    {
        private readonly IConfiguration _config;
        private readonly mTesoreria mTesoreria;
        private readonly mProGrX_AuxiliarDB _utils;
        private readonly int module = 9;
        private readonly mSecurityMainDb _mSecurity;

        public frmTES_TransferenciaReversaDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(_config);
            _utils = new mProGrX_AuxiliarDB(_config);
            _mSecurity = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene las solicitudes de transferencia reversa según los criterios especificados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<transferenciaSolicitudData>> TES_TransferenciaReversa_Obtener(int CodEmpresa, transferenciaSolicitudData solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<transferenciaSolicitudData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string whereClause = string.Empty;

                    if (solicitud.codigo != "")
                    {
                        whereClause += " and Codigo like @codigo ";
                    }
                    if (solicitud.ndocumento != "")
                    {
                        whereClause += " and Ndocumento like @ndocumento ";
                    }
                    if (solicitud.beneficiario != "")
                    {
                        whereClause += " and Beneficiario like @beneficiario ";
                    }
                    if (solicitud.cta_ahorros != "")
                    {
                        whereClause += " and Cta_Ahorros like @cta_ahorros ";
                    }

                    if (solicitud.cod_plan != "")
                    {
                        whereClause += " and isnull(cod_Plan,'-sp-') = @cod_plan ";
                    }

                    var query = $@"select nsolicitud,
                                                codigo,
                                                beneficiario,
                                                monto,
                                                fecha_emision,
                                                cta_ahorros,
                                                Ndocumento
                                   from Tes_Transacciones 
                                    where TRIM(documento_base) = @documento and id_banco = @id_banco {whereClause}";
                    response.Result = connection.Query<transferenciaSolicitudData>(query,
                        new
                        {
                            documento = solicitud.documento.Trim() ?? string.Empty,
                            id_banco = solicitud.id_banco,
                            cod_plan = solicitud.cod_plan ?? "-sp-",
                            codigo = $"'%{solicitud.codigo.Trim() ?? string.Empty}%'",
                            beneficiario = $"'%{solicitud.beneficiario.Trim() ?? string.Empty}%'",
                            cta_ahorros = $"'%{solicitud.cta_ahorros.Trim() ?? string.Empty}%'",
                            ndocumento = $"'%{solicitud.ndocumento.Trim() ?? string.Empty}%'",
                            
                           
                        }).ToList();
                    if (response.Result.Count == 0)
                    {
                        response.Code = -1;
                        response.Description = "No se encontraron datos para la solicitud especificada.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1; // Internal Server Error
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los planes de banco disponibles para la reversa de transferencias.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReversaPlanes_Obtener(int CodEmpresa, string id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Bp.COD_PLAN as 'item', Bp.COD_PLAN as 'descripcion'
                                    from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
                                    Where B.ID_BANCO = @id_banco And B.UTILIZA_PLAN = 1
                                    order by Bp.COD_PLAN  asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query,
                        new { id_banco }).ToList();

                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "-sp-",
                        descripcion = "Sin Plan"
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

        public ErrorDto<long> sbNTrasnferencia(int CodEmpresa, int id_banco, string tipo, string avance, string plan)
        {
            return mTesoreria.fxTesTipoDocConsec(CodEmpresa, id_banco, tipo, avance, plan);
        }

        /// <summary>
        /// Aplica una reversa a una transferencia existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="transferencia"></param>
        /// <returns></returns>
        public ErrorDto TES_TransferenciaReversa_Aplicar(int CodEmpresa, transferenciaReversaAplicaModel transferencia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select valor   from TES_PARAMETROS where COD_PARAMETRO = '11'";
                    var vDias = connection.QueryFirstOrDefault<int>(query);
                    if (vDias == 0)
                    {
                        vDias = 5; // Valor por defecto si no se encuentra en la base de datos
                    }

                    //calculo los días entre dos fechas
                    DateTime fecha1 = (DateTime)transferencia.lista.Last().fecha;
                    DateTime fecha2 = DateTime.Now;

                    double dias = (fecha2.Date - fecha1.Date).TotalDays;

                    if (dias > vDias)
                    {
                        response.Code = -2;
                        response.Description = $"Esta intentando reversar una transferencia con mas de {vDias}  días de emisión: {_utils.validaFechaGlobal(fecha1)}";
                        return response;
                    }

                    query = $@"Select * From Tes_Autorizaciones Where Clave= @clave and nombre = @usuario and estado = 'A'";
                    var autorizacion = connection.QueryFirstOrDefault<tesAutorizacionesDTO>(query,
                        new { clave = transferencia.clave, usuario = transferencia.usuario });

                    if (autorizacion == null)
                    {
                        response.Code = -1;
                        response.Description = "Contraseña Incorrecta, o no Existe Nivel de Autorización";
                        return response;
                    }

                        query = $@"select count(*) as Existe from tes_te_reversion where isnull(Tipo,'T') = 'T'
                                    and id_Banco = @id_banco and Documento = @documento ";
                    var existe = connection.QueryFirstOrDefault<int>(query,
                        new { id_banco = transferencia.id_banco, documento = transferencia.ndocumento.Trim() });

                    if (existe == 1)
                    {
                        response.Code = -2;
                        response.Description = $"La transferencia No.{transferencia.ndocumento}, ya fue reversada anteriormente!";
                        return response;
                    }

                    query = $@"exec spTES_TE_Reversion_Main @id_banco, @tipo, @documento, @observaciones, @usuario ";
                    var ReversionId = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            id_banco = transferencia.id_banco,
                            tipo = transferencia.tipo,
                            documento = transferencia.ndocumento.Trim(),
                            observaciones = transferencia.observaciones ?? string.Empty,
                            usuario = transferencia.usuario.ToUpper()
                        });

                    if (transferencia.lista.Count > 0)
                    {
                        foreach (var item in transferencia.lista)
                        {
                            query = $@"EXEC spTES_TE_Reversion_Transaccion @iConsecutivo, @item, @usuario ";
                            var result = connection.Execute(query,
                            new
                            {
                                iConsecutivo = ReversionId,
                                item = item.nsolicitud,
                                usuario = transferencia.usuario.ToUpper()
                            });

                        }
                    }

                    response.Description = ReversionId.ToString();

                    //bitacora
                    _mSecurity.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = transferencia.usuario,
                        Modulo = module, // Tesoreria
                        Movimiento = "Aplica",
                        DetalleMovimiento = "Reversion Transferencia = " + ReversionId + " Id.Cuenta:" + transferencia.id_banco + ", Tipo: " + transferencia.tipo,
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


        public ErrorDto<List<tesReversionData>> TES_TransferenciaConsulta_Obtener(
            int CodEmpresa, 
            int id_banco,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesReversionData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string fechaIni = fechaInicio.ToString("yyyy-MM-dd");
                    string fechaCorte = fechaFin.ToString("yyyy-MM-dd");

                    var query = $@"select * from tes_te_reversion where id_banco = @id_banco
                                   and fecha_genera between '{fechaIni} 00:00:00' and '{fechaCorte} 23:59:59'";

                    response.Result = connection.Query<tesReversionData>(query,
                        new
                        {
                            id_banco = id_banco
                        }).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1; // Internal Server Error
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// Obtiene los detalles de una reversa de transferencia específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_reversion"></param>
        /// <returns></returns>
        public ErrorDto<List<TransferenciaDetalleModel>> TES_TransferenciaReversa_Detalle(int CodEmpresa, string id_reversion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TransferenciaDetalleModel>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from vTes_TE_Reversion_Det where id_reversion = @id_reversion ";

                    response.Result = connection.Query<TransferenciaDetalleModel>(query,
                        new { id_reversion = id_reversion }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1; // Internal Server Error
                response.Description = ex.Message;
            }
            return response;

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
    }
}
