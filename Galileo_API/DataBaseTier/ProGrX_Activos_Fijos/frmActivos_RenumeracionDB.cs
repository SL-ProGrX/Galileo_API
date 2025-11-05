using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_RenumeracionDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_RenumeracionDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Metodo para consultar el listado de placas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosDataLista> Activos_Buscar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ActivosDataLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosDataLista
                {
                    total = 0,
                    lista = new List<ActivosData>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select COUNT(num_placa) from Activos_Principal";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( num_placa LIKE '%" + filtros.filtro + "%' " +
                            " OR Placa_Alterna LIKE '%" + filtros.filtro + "%' " +
                             " OR Nombre LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "num_placa";
                    }

                    query = $@"select  num_placa, Placa_Alterna, Nombre from Activos_Principal  
                                        {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    response.Result.lista = connection.Query<ActivosData>(query).ToList();

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
        /// Metodo para consultar el detalle del numero de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="num_placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosRenumeracionData> Activos_Renumeracion_Obtener(int CodEmpresa, string num_placa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ActivosRenumeracionData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosRenumeracionData()
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select A.num_placa,A.nombre,T.descripcion
                                    from Activos_Principal A inner join Activos_tipo_Activo T
                                    on A.tipo_activo = T.tipo_activo
                                     where A.num_placa =@num_placa";
                    result.Result = connection.Query<ActivosRenumeracionData>(query, new { num_placa }).FirstOrDefault();
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
        /// Metodo para actualizar el numero de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="num_placa"></param>
        /// <param name="nuevo_num"></param>
        /// <returns></returns>
        public ErrorDto Activos_Renumeracion_Actualizar(int CodEmpresa, string usuario, string num_placa, string nuevo_num)
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
                    var query = $@"UPDATE  Activos_Principal
                                    SET num_placa = @nuevo_num                                      
                                    WHERE num_placa = @num_placa";
                    connection.Execute(query, new
                    {
                        num_placa,
                        nuevo_num
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Renumeriación: {num_placa} a {nuevo_num}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
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
