using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_CA_RemesasDB
    {
        private readonly IConfiguration _config;
        mProGrx_Main mProGrx_Main;

        public frmCC_CA_RemesasDB(IConfiguration config)
        {
            _config = config;
            mProGrx_Main = new mProGrx_Main(_config);
        }

        public List<CcCaGenericData> CC_CA_LineasObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CcCaGenericData> resp = new List<CcCaGenericData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_LINEA as idx, rtrim(COD_LINEA) + ' - ' + descripcion as 'ItmX' from PRM_CA_LINEAS where activo = 1";
                    resp = connection.Query<CcCaGenericData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CcCaGenericData> CC_CA_EntidadesObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CcCaGenericData> resp = new List<CcCaGenericData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_Entidad as idx, rtrim(cod_Entidad) + ' - ' + descripcion as 'ItmX' from PRM_CA_ENTIDAD where activo = 1";
                    resp = connection.Query<CcCaGenericData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CcCaGenericData> CC_CA_ProcesosObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CcCaGenericData> resp = new List<CcCaGenericData>();
            decimal proceso = mProGrx_Main.glngFechaCR(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    for (int i = 0; i < 7; i++)
                    {
                        var data = new CcCaGenericData
                        {
                            itmx = proceso.ToString()
                        };
                        resp.Add(data);

                        var fxSIFPrmProcesoSig = @$"select dbo.fxSIFPrmProcesoSig({proceso}) as 'Result'";
                        proceso = connection.Query<int>(fxSIFPrmProcesoSig).FirstOrDefault();

                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<PrmCaRemesaDt> CC_CA_Remesa_DTObtener(int CodEmpresa, int Cod_Remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<PrmCaRemesaDt> resp = new List<PrmCaRemesaDt>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Remesa_Consulta {Cod_Remesa}";
                    resp = connection.Query<PrmCaRemesaDt>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<PrmCaRemesa> CC_CA_Remesas_Lista(int CodEmpresa, string Estado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<PrmCaRemesa> resp = new List<PrmCaRemesa>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Remesa_Lista {Estado}";
                    resp = connection.Query<PrmCaRemesa>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CcCaCasosData> CC_CA_BuscarCasos_SP(int CodEmpresa, string Filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltrosBuscarCasos request = JsonConvert.DeserializeObject<FiltrosBuscarCasos>(Filtros);
            List<CcCaCasosData> resp = new List<CcCaCasosData>();
            int tarjetas = 0;
            if (request.soloTarjetasValidas == true)
            {
                tarjetas = 1;
            }
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Busca_Casos @Proceso, @Linea, @Tarjetas, @FechaCorte, @NCuotas";

                    var parameters = new DynamicParameters();
                    parameters.Add("Proceso", request.proceso, DbType.Int32);
                    parameters.Add("Linea", request.linea, DbType.String);
                    parameters.Add("Tarjetas", tarjetas, DbType.Int32);
                    parameters.Add("FechaCorte", request.fechaCorte, DbType.Date);
                    parameters.Add("NCuotas", request.nCuotas, DbType.Int32);

                    resp = connection.Query<CcCaCasosData>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_CargarRemesa_SP(int CodEmpresa, RemesaInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            string procesoStr = request.proceso.ToString();
            string year = procesoStr.Substring(0, 4);
            string month = procesoStr.Substring(4, 2);
            string fechainicio = $"{year}/{month}/01";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spPrm_CA_Remesa @Proceso, @Linea, @Entidad, @Usuario, @FechaInicio, @FechaCorte, @NCuotas";

                    var parameters = new DynamicParameters();
                    parameters.Add("Proceso", request.proceso, DbType.Int32);
                    parameters.Add("Linea", request.linea, DbType.String);
                    parameters.Add("Entidad", request.entidad, DbType.String);
                    parameters.Add("Usuario", request.usuario, DbType.String);
                    parameters.Add("FechaInicio", fechainicio, DbType.Date);
                    parameters.Add("FechaCorte", request.fechacorte, DbType.Date);
                    parameters.Add("NCuotas", request.ncuotas, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Remesa agregada exitosamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_CargarRemesaDetalle_SP(int CodEmpresa, RemesaDetalleInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spPrm_CA_Remesa_Detalle @Remesa, @Cedula, @Nombre, @Compromiso, @Tarjeta, @TarjetaVence";

                    var parameters = new DynamicParameters();
                    parameters.Add("Remesa", request.remesa, DbType.Int32);
                    parameters.Add("Cedula", request.cedula, DbType.String);
                    parameters.Add("Nombre", request.nombre, DbType.String);
                    parameters.Add("Compromiso", request.compromiso, DbType.Decimal);
                    parameters.Add("Tarjeta", request.tarjeta, DbType.String);
                    parameters.Add("TarjetaVence", request.tarjetavence, DbType.Date);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Detalle de Remesa agregada exitosamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public List<RemesaArchivoData> CC_CA_Remesa_Archivo_Envia(int CodEmpresa, int Cod_Remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<RemesaArchivoData> resp = new List<RemesaArchivoData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Remesa_Archivo_Envia {Cod_Remesa}";
                    resp = connection.Query<RemesaArchivoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Remesa_Cierra(int CodEmpresa, int Cod_Remesa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Remesa_Cierra {Cod_Remesa}, {Usuario}";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    if (resp.Code != -1)
                    {
                        resp.Description = $"La Remesa {Cod_Remesa} ha sido cerrada con �xito";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Remesa_Autorizaciones_SP(int CodEmpresa, RemesaAutorizacion request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spPrm_CA_Remesa_Autorizaciones @Remesa, @Tarjeta, @Autorizacion, @Monto, @Comision, @Fecha";

                    var parameters = new DynamicParameters();
                    parameters.Add("Remesa", request.codremesa, DbType.Int32);
                    parameters.Add("Tarjeta", request.tarjeta, DbType.String);
                    parameters.Add("Autorizacion", request.autorizacion, DbType.String);
                    parameters.Add("Monto", request.monto, DbType.Decimal);
                    parameters.Add("Comision", request.comision, DbType.Decimal);
                    parameters.Add("Fecha", request.fecha, DbType.Date);
                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Informaci�n Cargada Satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public CaRemesaAplicaInicializa CC_CA_Remesa_Aplica_Inicializa(int CodEmpresa, int Cod_Remesa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CaRemesaAplicaInicializa resp = new CaRemesaAplicaInicializa();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Remesa_Aplica_Inicializa {Cod_Remesa}, '{Usuario}'";
                    resp = connection.Query<CaRemesaAplicaInicializa>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public CaAbonosDetallaMain CC_CA_Abonos_Detalla_Main(int CodEmpresa, int Cod_Remesa, int inicializa, CaRemesaAplicaInicializa request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CaAbonosDetallaMain resp = new CaAbonosDetallaMain();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPrm_CA_Abonos_Detalla_Main {Cod_Remesa}, '{request.rlinea}', {request.proceso},{inicializa},50";
                    resp = connection.Query<CaAbonosDetallaMain>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Abonos_Aplica(int CodEmpresa, int Cod_Remesa, string Usuario, CaRemesaAplicaInicializa request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select count(*) + 1 as Total from prm_ca_creditos
                        where cod_Remesa = {Cod_Remesa}
                        and id_aplicacion = 1 and ind_paso = 0";
                    int registros = connection.Query<int>(query).FirstOrDefault();

                    do
                    {
                        var query2 = $@"exec spPrm_CA_Abonos_Aplica {Cod_Remesa}, {request.proceso}, '{Usuario}', '{request.tipodoc}','{request.numdoc}',50";
                        registros = connection.ExecuteAsync(query2).Result;
                    } while (registros > 0);
                    //Aplica Inconsistencia a Fondos
                    var query3 = $@"exec spPrm_CA_Aplica_Fondos_Main {Cod_Remesa}, '{Usuario}', '{request.tipodoc}','{request.numdoc}'";
                    connection.Query(query3);
                    //Cierra Remesa
                    var query4 = $@"update prm_Ca_Remesas set Estado = 'A'  where COD_REMESA = {Cod_Remesa}";
                    resp.Code = connection.ExecuteAsync(query4).Result;
                    resp.Description = "Proceso finalizado con �xito";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Aplica_Asiento(int CodEmpresa, int Cod_Remesa, string Usuario, CaRemesaAplicaInicializa request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Asiento
                    var query = $@"exec spPrm_CA_Aplica_Asiento '{request.tipodoc}','{request.numdoc}', '{Usuario}', {Cod_Remesa}";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Remesa fue aplicada satisfactoriamente con " + request.tipodoc + " No: " + request.numdoc;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}