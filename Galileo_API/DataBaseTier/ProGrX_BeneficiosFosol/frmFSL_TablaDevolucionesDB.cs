using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_TablaDevolucionesDB
    {
        private readonly IConfiguration _config;

        public frmFSL_TablaDevolucionesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<FslGarantiasData>> FslGarantias_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<FslGarantiasData>>();

            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Garantia as 'item', rtrim(Garantia) + ' - ' + rtrim(descripcion) as 'descripcion'
		                                       from CRD_Garantia_Tipos
		                                       order by Garantia";
                    response.Result = connection.Query<FslGarantiasData>(query).ToList();
                   
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslGarantias_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response; ;
        }

        public ErrorDTO<FslDevolucionesDataLista> FslDevoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<FslDevolucionesDataLista>();

            response.Result = new FslDevolucionesDataLista();
            response.Code = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = "SELECT COUNT(*) from FSL_TABLA_DEVOLUCIONES ";
                    response.Code = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " where COD_DEVOLUCION LIKE '%" + filtro + "%' OR Gar.descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }


                    query = $@"select Fsl.COD_DEVOLUCION,Fsl.Fecha_Inicio,Fsl.Fecha_Corte
		                          , rtrim(Fsl.Garantia) as 'GARANTIA', BASE_APLICACION as _base
		                          ,Fsl.Porcentaje, Fsl.Registro_Fecha,Fsl.Registro_Usuario
		                           from FSL_TABLA_DEVOLUCIONES Fsl inner join CRD_Garantia_Tipos Gar on Fsl.Garantia = Gar.Garantia
                                   {filtro} 
		                           order by Fsl.Fecha_Inicio
                                        {paginaActual}
                                        {paginacionActual} ";
                    response.Result.devoluciones = connection.Query<FslDevolucionesData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslDevoluciones_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }


        public ErrorDTO ParametroDevolucion_Guardar(int CodCliente, FslDevolucionesData devolucion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0; ;

            try
            {
                if (!Devolucion_Existe(CodCliente, devolucion.cod_devolucion))
                {
                    info = FslDevolucion_Insertar(CodCliente, devolucion);
                }
                else
                {
                    info = FslDevolucion_Actualizar(CodCliente, devolucion);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }


        public ErrorDTO FslDevolucion_Insertar(int CodCliente, FslDevolucionesData devolucion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (Devolucion_Existe(CodCliente, devolucion.cod_devolucion))
                    {
                        info.Code = 1;
                        info.Description = "La devolución ya existe";
                        return info;
                    }
                    else
                    {

                        var query = $@"select coalesce(max(COD_DEVOLUCION),0) + 1 as Ultimo from FSL_TABLA_DEVOLUCIONES";
                        int ultimo = connection.Query<int>(query).FirstOrDefault();

                        query = $@"insert into FSL_TABLA_DEVOLUCIONES(COD_DEVOLUCION,Fecha_Inicio,Fecha_Corte,Garantia,Base_Aplicacion,Porcentaje,registro_fecha,registro_usuario) values
                                             ('{ultimo}','{devolucion.fecha_inicio}','{devolucion.fecha_corte}','{devolucion.garantia}','{devolucion._base}',{devolucion.porcentaje}, getdate(),'{devolucion.registro_usuario}')";
                        connection.Execute(query, devolucion);
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }

        private bool Devolucion_Existe(int CodCliente, int cod_devolucion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool resp = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(count(*),0) as Existe from FSL_TABLA_DEVOLUCIONES where COD_DEVOLUCION = '{cod_devolucion}' ";

                    var info = connection.Query<string>(query).ToList();
                    if (info.Count > 0)
                    {
                        resp = info[0] == "0" ? false : true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = true;
            }
            return resp;
        }

        public ErrorDTO FslDevolucion_Actualizar(int CodCliente, FslDevolucionesData devolucion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update FSL_TABLA_DEVOLUCIONES set Fecha_Inicio = '{devolucion.fecha_inicio}',
                                        Fecha_Corte = '{devolucion.fecha_corte}', Garantia = '{devolucion.garantia}', Base_Aplicacion = '{devolucion._base}' 
                                        , Porcentaje = {devolucion.porcentaje}  where COD_DEVOLUCION = '{devolucion.cod_devolucion}' ";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDTO FslDevolucion_Eliminar(int CodCliente, int cod_devolucion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0; ;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from FSL_TABLA_DEVOLUCIONES where COD_DEVOLUCION = {cod_devolucion} ";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }
    }
}