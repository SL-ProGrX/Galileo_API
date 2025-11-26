using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_ReportesAlCorteDB
    {
        private readonly IConfiguration _config;
        MColaboradorDB DbColaboradorDB;

        public frmCC_ReportesAlCorteDB(IConfiguration config)
        {
            _config = config;
            DbColaboradorDB = new MColaboradorDB(_config);
        }

        public List<CCGenericList> CC_Periodos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                var query = $@"SELECT id_per_historico, Mes, Anio from ase_per_historico order by anio desc,mes desc";
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                // Lee los resultados
                while (reader.Read())
                {
                    //Obtiene el numero del mes
                    int numeroMes = Convert.ToInt32(reader["Mes"]);

                    //Llama a funcion que pasa del numero al nombre del Mes
                    string nombreMes = MColaboradorDB.ConvierteMes(numeroMes);

                    //Guarda el resultado de la consulta
                    var data = new CCGenericList
                    {
                        idx = reader["id_per_historico"]?.ToString() ?? string.Empty,
                        itmx = reader["Anio"] + " - " + nombreMes
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_INSTITUCION as Idx,descripcion as ItmX from INSTITUCIONES";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_PROFESION as Idx,descripcion as ItmX from AFI_PROFESIONES";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_SECTOR as Idx,descripcion as ItmX from AFI_SECTORES";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_ZONA as 'IdX', rtrim(descripcion) as 'ItmX' from AFI_ZONAS";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_estado) as 'IdX', rtrim(descripcion) as 'ItmX' from  afi_Estados_Persona";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(GARANTIA) as 'IdX', rtrim(descripcion) as 'Itmx' from CRD_GARANTIA_TIPOS order by GARANTIA";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_clasificacion) as 'IdX' , rtrim(descripcion) as 'ItmX' from CBR_CLASIFICACION_CARTERA order by cod_clasificacion";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_oficina) as 'IdX', rtrim(descripcion) as 'Itmx' from SIF_Oficinas order by cod_oficina";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Estado_Civil as 'IdX', Descripcion as 'ItmX' from SYS_ESTADO_CIVIL where Activo = 1 order by Descripcion asc";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ESTADO_LABORAL as 'IdX', Descripcion as 'ItmX' from AFI_ESTADO_LABORAL where Activo = 1 order by Descripcion asc";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Provincia as Idx, rtrim(Descripcion) as ItmX from Provincias";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Canton as Idx, rtrim(Descripcion) as ItmX from Cantones
                        where provincia = '{Provincia}' order by descripcion";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Distrito as Idx, rtrim(Descripcion) as ItmX from Distritos
                        where provincia = '{Provincia}' and canton = {Canton} order by descripcion";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"SELECT CODIGO as Idx,DESCRIPCION as ItmX FROM CATALOGO";
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (!string.IsNullOrEmpty(CodCatalgo))
                    {
                        query += $@"select (R.cod_destino) as 'IdX' , rtrim(R.descripcion) as ItmX 
                            from catalogo_destinos R inner join catalogo_destinosAsg A on R.cod_destino = A.cod_destino 
                            where A.codigo = '{CodCatalgo}'";
                    }
                    else
                    {
                        query = $"select cod_destino as 'IdX' , rtrim(descripcion) as ItmX from  catalogo_destinos";
                    }
                    resp = connection.Query<CCGenericList>(query).ToList();
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (!string.IsNullOrEmpty(CodCatalgo))
                    {
                        query += $@"select (R.cod_grupo) as 'IdX' , rtrim(R.descripcion) as ItmX 
                            from catalogo_grupos R inner join catalogo_AsignaGrp A on R.cod_grupo = A.cod_grupo 
                            where A.codigo = '{CodCatalgo}'";
                    }
                    else
                    {
                        query = $"select cod_grupo as 'IdX', rtrim(descripcion) as ItmX from catalogo_grupos";
                    }
                    resp = connection.Query<CCGenericList>(query).ToList();
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (!string.IsNullOrEmpty(CodInstitucion))
                    {
                        query += $@"select cod_departamento as idx, descripcion as itmx from afDepartamentos 
                            where cod_institucion = '{CodInstitucion}'";
                    }
                    else
                    {
                        query = $"select cod_departamento as idx,descripcion as itmx from afDepartamentos";
                    }
                    resp = connection.Query<CCGenericList>(query).ToList();
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
            List<CCGenericList> resp = new List<CCGenericList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (!string.IsNullOrEmpty(CodInstitucion) && !string.IsNullOrEmpty(CodDepartamento))
                    {
                        query += $@"select cod_seccion as IdX, descripcion as ItmX from afSecciones
                            where cod_institucion = '{CodInstitucion}' and cod_departamento = '{CodDepartamento}'";
                    }
                    else
                    {
                        query = $"select cod_departamento as idx,descripcion as itmx from afSecciones";
                    }
                    resp = connection.Query<CCGenericList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CbrAnalisisCubosData> CbrAnalisis_Cubos_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CbrAnalisisCubosData> resp = new List<CbrAnalisisCubosData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec {nombreSP} {Anio}, {Mes}";
                    resp = connection.Query<CbrAnalisisCubosData>(query).ToList();
                }
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
            List<CbrEstimacionData> resp = new List<CbrEstimacionData>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec {nombreSP} {Anio}, {Mes}";
                    resp = connection.Query<CbrEstimacionData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

    }
}