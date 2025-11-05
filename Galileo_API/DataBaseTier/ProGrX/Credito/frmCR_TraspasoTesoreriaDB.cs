using Dapper;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Utilities;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.ProGrX.Credito;

namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_TraspasoTesoreriaDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos
        private readonly mTesoreria _mtes;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmCR_TraspasoTesoreriaDB(IConfiguration? config)
        {
            _config = config;
            _mtes = new mTesoreria(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        #region remesas
        #endregion

        #region cargar
        #endregion

        #region trasladar

        /// <summary>
        /// Metodo para obtener las remesas en estado 'C' (Cerradas) para el traspaso a tesoreria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_remesa as 'item', CONCAT(cod_remesa,' - ',FECHA_INICIO,' - ', FECHA_CORTE, ' - ', USUARIO ) as 'descripcion' from CRD_REMESAS_TES where estado = 'C' order by fecha desc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// METODO: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// METODO: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }

        public ErrorDTO<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TraspasoModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TraspasoModel>()
            };
            try
            {
                string vFechaInicio = "";
                string vFechaCorte = "";
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select fecha_inicio,fecha_corte from CRD_REMESAS_TES where cod_remesa = @cod_remesa";
                    var result = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)>(query, new { cod_remesa });
                    if (result != default)
                    {
                        vFechaInicio = result.fecha_inicio.ToString("yyyy-MM-dd");
                        vFechaCorte = result.fecha_corte.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "No se encontró la remesa.";
                        return response;
                    }

                    query = $@"select R.id_solicitud,R.codigo,S.cedula,S.nombre,R.montoapr,R.monto_girado
                               , isnull(D.Numero,0) as 'Desembolsos_Numero', isnull(D.Monto,0) as 'Desembolsos'
                                from reg_creditos R inner join Socios S on R.cedula = S.cedula
                                inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
                                 left join vCrdOperacion_DesembolsosGiro D on R.id_Solicitud = D.id_Solicitud
                                where R.estadosol='F' 
                                and R.fechaforp between '{vFechaInicio} 00:00:00' and '{vFechaCorte} 23:59:59'
                                and R.estado in('A','C') 
                                and  R.id_solicitud in(select id_solicitud from CRD_REMESAS_TES_DETALLE
                                where cod_remesa = @id_remesa)
                                order by R.id_solicitud";
                    response.Result = connection.Query<TraspasoModel>(query, new { id_remesa = cod_remesa }).ToList();

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

        public ErrorDTO CrTraspasoTes_Traslado_Generar(int CodEmpresa, int cod_remesa, string usuario ,string? token)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string vFecha = DateTime.Now.ToString("yyyy-MM-dd");
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if(token == null || token.Trim() == "")
                    {
                        query = "select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
                        var vToken = connection.QueryFirstOrDefault<string?>(query);
                        if(vToken != null && vToken.Trim() != "")
                        {
                            token = vToken;
                        }
                        else
                        {
                            _mtes.spTes_Token_New(CodEmpresa, usuario);
                            token = connection.QueryFirstOrDefault<string?>(query);
                        }

                        query = $@"select id_solicitud,codigo
                                   from reg_creditos
                                   where estado in('A','C') and estadosol = 'F' and tesoreria is null
                                   and id_solicitud in(select id_solicitud from CRD_REMESAS_TES_DETALLE where cod_remesa = @cod_remesa)";
                        var lista = connection.Query<(int id_solicitud, string codigo)>(query, new { cod_remesa }).ToList();

                        foreach (var item in lista)
                        {
                            //'Nuevo Proceso (Integrado)
                            query = "exec spCrdCreditoEnviaTesoreria_Todo @Operacion, @Token, @Remesa, @RemesaTipo ";
                            connection.Execute(query, new { Operacion = item.id_solicitud, Token = token, Remesa = cod_remesa, RemesaTipo = "CRD" });

                            //Bitacora
                            _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = usuario,
                                DetalleMovimiento = $"Traspaso a Tesoreria de la Operacion y Desembol OP: {item.id_solicitud}",
                                Movimiento = "Registra - WEB",
                                Modulo = vModulo
                            });


                            //Tags de Seguimiento
                            _mtes.sbCrdOperacionTags(CodEmpresa ,item.id_solicitud, item.codigo, "S04", usuario, "" , $"Remesa de Traslado No..: {cod_remesa}");
                        }

                        //'Actualiza y Carga Remesa
                        query = $@"update CRD_REMESAS_TES SET Estado = 'T' Where cod_remesa = @cod_remesa";
                        connection.Execute(query, new { cod_remesa });

                        response.Description = $"Operaciones Enviadas a Tesoreria Satisfactoriamente...";
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

        #endregion

        #region informes
        #endregion

        #region reactivaciones
        #endregion

        #region cambio
        #endregion

        #region consultas
        #endregion

        #region aux.giro
        #endregion
    }
}
