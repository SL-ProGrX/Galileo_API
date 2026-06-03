using Dapper;
using Microsoft.Data.SqlClient; 
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCcCaLineasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        private const string CampoCodLinea = "Cod_Linea";
        private const string CampoDescripcion = "descripcion";
        private const string CampoCodPlan = "cod_plan";

        public FrmCcCaLineasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consutal de listado de tipos de lineas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CcCaLineasLista> CC_CA_Lineas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de líneas son requeridos.", -2, CrearResultadoLineasVacio());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new CcCaLineasLista
                {
                    total = connection.QueryFirstOrDefault<int>("select COUNT(1) from PRM_CA_LINEAS"),
                    lista = new List<CcCaLineasData>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldLineas(filtros.sortField);
                var sortDirection = ObtenerSortDirectionLineas(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var query = @"
                    select Cod_Linea,
                           descripcion,
                           cod_plan,
                           activo
                    from PRM_CA_LINEAS
                    where (
                        @Filtro is null
                        or Cod_Linea like @Filtro
                        or descripcion like @Filtro
                        or cod_plan like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'Cod_Linea' AND @SortDirection = 'ASC' THEN Cod_Linea END ASC,
                        CASE WHEN @SortField = 'Cod_Linea' AND @SortDirection = 'DESC' THEN Cod_Linea END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'ASC' THEN cod_plan END ASC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'DESC' THEN cod_plan END DESC,
                        Cod_Linea ASC";

                if (fetchRows > 0)
                {
                    query += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                salida.lista = connection.Query<CcCaLineasData>(
                    query,
                    new
                    {
                        Filtro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%",
                        SortField = sortField,
                        SortDirection = sortDirection,
                        OffsetRows = offsetRows,
                        FetchRows = fetchRows
                    }).ToList();

                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearResultadoLineasVacio())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar líneas.", result.Code.GetValueOrDefault(-1), CrearResultadoLineasVacio());
        }

        /// <summary>
        ///  Inserta o actualiza un registro de tipos de lineas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CC_CA_Lineas_Guardar(int CodEmpresa, string usuario, CcCaLineasData request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la línea son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(
                    "select isnull(count(*),0) as Existe from PRM_CA_LINEAS where Cod_Linea = @cod_linea",
                    new { request.cod_linea });

                if (request.isNew)
                {
                    return existe > 0
                        ? DbHelper.ErrorResponse($"La línea con el código {request.cod_linea} ya existe.", -2)
                        : CC_CA_Lineas_Insertar(connection, CodEmpresa, usuario, request);
                }

                return existe == 0
                    ? DbHelper.ErrorResponse($"La línea con el código {request.cod_linea} no existe.", -2)
                    : CC_CA_Lineas_Actualizar(connection, CodEmpresa, usuario, request);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar línea.", result.Code.GetValueOrDefault(-1));
        }
        
        /// <summary>
        /// Actualiza un tipo de linea
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CC_CA_Lineas_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CcCaLineasData datos)
        {
            connection.Execute(
                @"update PRM_CA_LINEAS
                  set descripcion = @descripcion,
                      Cod_Plan = @cod_plan,
                      Activo = @activo
                  where Cod_Linea = @cod_linea",
                new
                {
                    datos.cod_linea,
                    datos.descripcion,
                    datos.cod_plan,
                    datos.activo,
                    usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico - Tipo Linea: {datos.cod_linea}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }
       
        /// <summary>
        /// Inserta un nuevo tipo de linea
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CC_CA_Lineas_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CcCaLineasData datos)
        {
            connection.Execute(
                @"insert into PRM_CA_LINEAS(Cod_Linea, descripcion, cod_plan, Activo, Registro_Usuario, Registro_Fecha)
                  values(@cod_linea, @descripcion, @cod_plan, @activo, @usuario, Getdate())",
                new
                {
                    datos.cod_linea,
                    datos.descripcion,
                    datos.cod_plan,
                    datos.activo,
                    usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico - Tipo Linea: {datos.cod_linea}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un tipo de linea
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="cod_Linea"></param>
        /// <returns></returns>
        public ErrorDto CC_CA_CatalogoLineas_Delete(int CodEmpresa, string Usuario, string cod_Linea)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete PRM_CA_LINEAS where Cod_Linea = @cod_Linea",
                new { cod_Linea });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de línea.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Cargo Automatico - Tipo Linea: {cod_Linea}", "Elimina - WEB");
                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        /// <summary>
        /// Consulta de listado de lineas activas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Lineas_Cbo_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(Cod_Linea) as item,
                         rtrim(Cod_Linea) + '-' + descripcion as descripcion
                  FROM PRM_CA_LINEAS
                  where activo = 1");
        }

        /// <summary>
        /// Consultar catalodo de codigos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Linea"></param>
        /// <returns></returns>
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoLineas_Obtener(int CodEmpresa, string cod_Linea)
        {
            return DbHelper.ExecuteListQuery<CcCaCatalogoLineasData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Cat.Codigo,
                         Cat.Descripcion,
                         isnull(Dt.Codigo,'-1') as Existe
                  from Catalogo Cat
                  left join prm_Ca_Lineas_Dt Dt on Cat.codigo = Dt.Codigo and Dt.cod_Linea = @cod_Linea
                  Order by isnull(Dt.Codigo,'ZZZZZZZ'), Cat.Codigo",
                new { cod_Linea });
        }

        /// <summary>
        /// Asignar un codigo a una linea 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_Linea"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto CC_CA_LineasDetalle_Insertar(int CodEmpresa, string usuario, string cod_Linea, string codigo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"insert prm_ca_lineas_dt(cod_linea, codigo, registro_usuario, registro_Fecha)
                      values(@cod_Linea, @codigo, @usuario, dbo.mygetdate())",
                    new
                    {
                        cod_Linea,
                        codigo,
                        usuario
                    });

                RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico: Linea: {cod_Linea} Cod: {codigo} ", "Registra - WEB");
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al asignar código a la línea.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Eliminar un codigo a una linea 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="cod_Linea"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto CC_CA_LineasDetalle_Delete(int CodEmpresa, string Usuario, string cod_Linea, string codigo)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete prm_ca_lineas_dt where cod_linea = @cod_Linea and codigo = @codigo",
                new { cod_Linea, codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar código de la línea.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Cargo Automatico: Linea: {cod_Linea} Cod: {codigo} ", "Elimina - WEB");
                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }
        private static CcCaLineasLista CrearResultadoLineasVacio()
        {
            return new CcCaLineasLista
            {
                total = 0,
                lista = new List<CcCaLineasData>()
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string ObtenerSortFieldLineas(string? sortField)
        {
            return sortField switch
            {
                CampoCodLinea => CampoCodLinea,
                CampoDescripcion => CampoDescripcion,
                CampoCodPlan => CampoCodPlan,
                _ => CampoCodLinea
            };
        }

        private static string ObtenerSortDirectionLineas(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}