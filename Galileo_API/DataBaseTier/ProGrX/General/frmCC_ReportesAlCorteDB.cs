using Dapper;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCcReportesAlCorteDb
    {
        private readonly IConfiguration _config;

        public FrmCcReportesAlCorteDb(IConfiguration config)
        {
            _config = config;
        }

        public List<CCGenericList> CC_Periodos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                const string query = @"SELECT id_per_historico, Mes, Anio 
                                       FROM ase_per_historico 
                                       ORDER BY anio DESC, mes DESC";

                using var connection = new SqlConnection(stringConn);
                connection.Open();

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int numeroMes = Convert.ToInt32(reader["Mes"]);
                    string nombreMes = MColaboradorDB.ConvierteMes(numeroMes);

                    var data = new CCGenericList
                    {
                        idx = reader["id_per_historico"]?.ToString() ?? string.Empty,
                        itmx = $"{reader["Anio"]} - {nombreMes}"
                    };
                    resp.Add(data);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Instituciones_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select COD_INSTITUCION as Idx, descripcion as ItmX from INSTITUCIONES";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Profesiones_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select COD_PROFESION as Idx, descripcion as ItmX from AFI_PROFESIONES";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Sectores_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select COD_SECTOR as Idx, descripcion as ItmX from AFI_SECTORES";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Zonas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select COD_ZONA as IdX, rtrim(descripcion) as ItmX from AFI_ZONAS";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Estados_Persona_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select rtrim(cod_estado) as IdX, rtrim(descripcion) as ItmX from afi_Estados_Persona";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Garantias_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select rtrim(GARANTIA) as IdX, rtrim(descripcion) as Itmx 
                                       from CRD_GARANTIA_TIPOS 
                                       order by GARANTIA";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Carteras_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select rtrim(cod_clasificacion) as IdX, rtrim(descripcion) as ItmX 
                                       from CBR_CLASIFICACION_CARTERA 
                                       order by cod_clasificacion";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Oficinas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select rtrim(cod_oficina) as IdX, rtrim(descripcion) as Itmx 
                                       from SIF_Oficinas 
                                       order by cod_oficina";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Estados_Civiles_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select Estado_Civil as IdX, Descripcion as ItmX 
                                       from SYS_ESTADO_CIVIL 
                                       where Activo = 1 
                                       order by Descripcion asc";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Estados_Laborales_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select ESTADO_LABORAL as IdX, Descripcion as ItmX 
                                       from AFI_ESTADO_LABORAL 
                                       where Activo = 1 
                                       order by Descripcion asc";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Provincias_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"select Provincia as Idx, rtrim(Descripcion) as ItmX from Provincias";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Cantones_Obtener(int CodEmpresa, int Provincia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"
                    select Canton as Idx, rtrim(Descripcion) as ItmX 
                    from Cantones
                    where provincia = @Provincia 
                    order by descripcion";

                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query, new { Provincia }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Distritos_Obtener(int CodEmpresa, int Provincia, int Canton)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"
                    select Distrito as Idx, rtrim(Descripcion) as ItmX 
                    from Distritos
                    where provincia = @Provincia 
                      and canton = @Canton 
                    order by descripcion";

                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query, new { Provincia, Canton }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Catalogo_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                const string query = @"SELECT CODIGO as Idx, DESCRIPCION as ItmX FROM CATALOGO";
                using var connection = new SqlConnection(stringConn);
                resp = connection.Query<CCGenericList>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Catalogo_Destinos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!string.IsNullOrEmpty(CodCatalgo))
                {
                    const string queryFiltrado = @"
                        select R.cod_destino as IdX, rtrim(R.descripcion) as ItmX 
                        from catalogo_destinos R 
                        inner join catalogo_destinosAsg A on R.cod_destino = A.cod_destino 
                        where A.codigo = @CodCatalgo";

                    resp = connection.Query<CCGenericList>(queryFiltrado, new { CodCatalgo }).ToList();
                }
                else
                {
                    const string queryTodos = @"
                        select cod_destino as IdX, rtrim(descripcion) as ItmX 
                        from catalogo_destinos";

                    resp = connection.Query<CCGenericList>(queryTodos).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Catalogo_Grupos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!string.IsNullOrEmpty(CodCatalgo))
                {
                    const string queryFiltrado = @"
                        select R.cod_grupo as IdX, rtrim(R.descripcion) as ItmX 
                        from catalogo_grupos R 
                        inner join catalogo_AsignaGrp A on R.cod_grupo = A.cod_grupo 
                        where A.codigo = @CodCatalgo";

                    resp = connection.Query<CCGenericList>(queryFiltrado, new { CodCatalgo }).ToList();
                }
                else
                {
                    const string queryTodos = @"
                        select cod_grupo as IdX, rtrim(descripcion) as ItmX 
                        from catalogo_grupos";

                    resp = connection.Query<CCGenericList>(queryTodos).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Departamentos_Obtener(int CodEmpresa, string? CodInstitucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!string.IsNullOrEmpty(CodInstitucion))
                {
                    const string queryFiltrado = @"
                        select cod_departamento as idx, descripcion as itmx 
                        from afDepartamentos 
                        where cod_institucion = @CodInstitucion";

                    resp = connection.Query<CCGenericList>(queryFiltrado, new { CodInstitucion }).ToList();
                }
                else
                {
                    const string queryTodos = @"
                        select cod_departamento as idx, descripcion as itmx 
                        from afDepartamentos";

                    resp = connection.Query<CCGenericList>(queryTodos).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CCGenericList> CC_Secciones_Obtener(int CodEmpresa, string? CodInstitucion, string? CodDepartamento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!string.IsNullOrEmpty(CodInstitucion) && !string.IsNullOrEmpty(CodDepartamento))
                {
                    const string queryFiltrado = @"
                        select cod_seccion as IdX, descripcion as ItmX 
                        from afSecciones
                        where cod_institucion = @CodInstitucion 
                          and cod_departamento = @CodDepartamento";

                    resp = connection.Query<CCGenericList>(queryFiltrado, new { CodInstitucion, CodDepartamento }).ToList();
                }
                else
                {
                    const string queryTodos = @"
                        select cod_departamento as idx, descripcion as itmx 
                        from afSecciones";

                    resp = connection.Query<CCGenericList>(queryTodos).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        // ========================
        //  Lista blanca de SP
        // ========================

        private static readonly HashSet<string> AllowedAnalisisProcedures = new()
        {
            // Reemplaza estos nombres por los SP reales que quieras permitir
            "spCbrAnalisisCubo1",
            "spCbrAnalisisCubo2"
        };

        private static readonly HashSet<string> AllowedEstimacionProcedures = new()
        {
            // Reemplaza estos nombres por los SP reales que quieras permitir
            "spCbrEstimacion1",
            "spCbrEstimacion2"
        };

        private static string GetAnalisisProcedureName(string nombreSP)
        {
            if (!AllowedAnalisisProcedures.Contains(nombreSP))
            {
                throw new ArgumentException("Nombre de procedimiento de análisis no permitido.", nameof(nombreSP));
            }
            return nombreSP;
        }

        private static string GetEstimacionProcedureName(string nombreSP)
        {
            if (!AllowedEstimacionProcedures.Contains(nombreSP))
            {
                throw new ArgumentException("Nombre de procedimiento de estimación no permitido.", nameof(nombreSP));
            }
            return nombreSP;
        }

        public List<CbrAnalisisCubosData> CbrAnalisis_Cubos_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CbrAnalisisCubosData> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                var procName = GetAnalisisProcedureName(nombreSP);

                resp = connection.Query<CbrAnalisisCubosData>(
                    procName,
                    new { Anio, Mes },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CbrEstimacionData> CbrEstimacion_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CbrEstimacionData> resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);

                var procName = GetEstimacionProcedureName(nombreSP);

                resp = connection.Query<CbrEstimacionData>(
                    procName,
                    new { Anio, Mes },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }
    }
}