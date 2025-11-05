using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxP_EventosDB
    {
        private readonly IConfiguration _config;

        public frmCxP_EventosDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<CxPEventos> Eventos_Obtener(int CodCliente, string cod_evento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<CxPEventos>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //var query = $@"select * from CXP_EVENTOS where cod_evento = '{cod_evento}'";
                    var query = $@"SELECT E.*, 
                                   ISNULL(Cta.Descripcion, '') AS 'cod_comision_cuenta', 
                                   ISNULL(Cta.Cod_Cuenta_Mask, '') AS 'comision_cuenta', 
                                   ISNULL(Crd.Codigo, '') AS 'cod_linea_crd', 
                                   ISNULL(Crd.Descripcion, '') AS 'descripcion_linea_crd'
                            FROM cxp_Eventos E
                            LEFT JOIN CntX_Cuentas Cta 
                                ON E.Comision_Cuenta = Cta.cod_Cuenta 
                                AND Cta.cod_contabilidad = 1 -- 
                            LEFT JOIN Catalogo Crd 
                                ON E.cod_Linea_Crd = Crd.Codigo
                            WHERE E.cod_Evento = '{cod_evento}'";
                    response.Result = connection.Query<CxPEventos>(query).FirstOrDefault();
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

        public ErrorDTO top1EventoObtener(int CodCliente, int Scroll, string cod_evento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Top 1 cod_evento from CXP_EVENTOS";

                    if (!string.IsNullOrEmpty(cod_evento))
                    {
                        if (Scroll == 1)
                        {
                            query += $" where cod_evento > '{cod_evento}' order by cod_evento asc";
                        }
                        else
                        {
                            query += $" where cod_evento < '{cod_evento}' order by cod_evento desc";
                        }
                    }

                    info.Description = connection.QueryFirstOrDefault<string>(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDTO Evento_Guardar(int CodCliente, CxPEventos evento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;


            var activo = 0;
            if (evento.activo == true)
            {
                activo = 1;
            }
            else
            {
                activo = 2;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {


                    var query = $@"exec spCxP_Eventos_Add '{evento.cod_evento}', '{evento.descripcion}', {activo},
                    '{evento.fecha_inicio}', '{evento.fecha_finaliza}', '{evento.lugar_venta}', '{evento.notas}', {evento.comision_porc}, '{evento.comision_cuenta}',
                        '{evento.cod_linea_crd}', '{evento.registro_usuario}'";

                    connection.ExecuteAsync(query);
                    info.Description = evento.cod_evento.ToString();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;

            }
            return info;
        }

        public ErrorDTO Evento_Eliminar(int CodCliente, string cod_evento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete CXP_EVENTOS where cod_evento = '{cod_evento}'";
                    connection.Execute(query);
                }
            }
            catch (Exception)
            {
                info.Code = -1;
                info.Description = "No se puede eliminar el evento, ya que tiene registros asociados";
            }
            return info;
        }

        public ErrorDTO<List<CxPEventosProveedor>> ObtenerProveedoresEvento(int CodEmpresa, string? cod_evento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CxPEventosProveedor>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"exec [spCxP_Eventos_Proveedores_List] '{cod_evento}' ";
                    response.Result = connection.Query<CxPEventosProveedor>(query).ToList();
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

        public ErrorDTO AsignaEventoProveedor(int CodCliente, int proveedor, string evento, int activa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec [spCxP_Proveedores_Eventos_Asigna] {proveedor}, {evento}, {activa}, '{usuario}'";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;

            }
            return info;

        }

        public ErrorDTO<List<CxPEventosBusqueda>> EventosLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CxPEventosBusqueda>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select cod_evento, descripcion from CXP_EVENTOS";
                    response.Result = connection.Query<CxPEventosBusqueda>(query).ToList();
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

        public ErrorDTO<List<CxPEventosLineas>> EventosLineas_Obtener(int CodEmpresa, string cod_evento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CxPEventosLineas>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT CODIGO AS CrdCod, DESCRIPCION AS  CrdDesc from Catalogo";
                    
                    response.Result = connection.Query<CxPEventosLineas>(query).ToList();
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