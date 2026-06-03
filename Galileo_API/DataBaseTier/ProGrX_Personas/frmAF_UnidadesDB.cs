using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAfUnidadesDb
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private const string CampoCodigo = "Codigo";
        private const string CampoDescripcion = "Descripcion";

        public FrmAfUnidadesDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de provincias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Unidades_Provincias_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Provincia as item, Descripcion from Provincias");
        }

        /// <summary>
        /// Obtener lista de unidades
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="rbTipo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_Unidades_Lista_Obtener(int CodEmpresa, int rbTipo, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de unidades son requeridos.", -2, new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<AfUnidadesDto>()
                });
            }

            var resultadoVacio = new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<AfUnidadesDto>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<AfUnidadesDto>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldUnidades(rbTipo, filtros.sortField);
                var sortDirection = ObtenerSortDirectionUnidades(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;
                var filtroParametro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%";

                if (rbTipo == 0)
                {
                    salida.total = connection.QueryFirstOrDefault<int>(
                        @"Select COUNT(U.Codigo)
                          from UProgramatica U left join Provincias P on U.Provincia = P.Provincia
                          where @Filtro is null
                             or U.Descripcion like @Filtro
                             or U.Codigo like @Filtro");

                    var query = @"
                        Select U.Codigo,
                               U.Descripcion,
                               isnull(P.Provincia,'') as Provincia,
                               isnull(P.Descripcion,'') as ProvinciaDesc
                        from UProgramatica U
                        left join Provincias P on U.Provincia = P.Provincia
                        where @Filtro is null
                           or U.Descripcion like @Filtro
                           or U.Codigo like @Filtro
                        order by
                            CASE WHEN @SortField = 'Codigo' AND @SortDirection = 'ASC' THEN U.Codigo END ASC,
                            CASE WHEN @SortField = 'Codigo' AND @SortDirection = 'DESC' THEN U.Codigo END DESC,
                            CASE WHEN @SortField = 'Descripcion' AND @SortDirection = 'ASC' THEN U.Descripcion END ASC,
                            CASE WHEN @SortField = 'Descripcion' AND @SortDirection = 'DESC' THEN U.Descripcion END DESC,
                            CASE WHEN @SortField = 'Provincia' AND @SortDirection = 'ASC' THEN P.Provincia END ASC,
                            CASE WHEN @SortField = 'Provincia' AND @SortDirection = 'DESC' THEN P.Provincia END DESC,
                            CASE WHEN @SortField = 'ProvinciaDesc' AND @SortDirection = 'ASC' THEN P.Descripcion END ASC,
                            CASE WHEN @SortField = 'ProvinciaDesc' AND @SortDirection = 'DESC' THEN P.Descripcion END DESC,
                            U.Codigo ASC";

                    if (fetchRows > 0)
                    {
                        query += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                    }

                    salida.lista = connection.Query<AfUnidadesDto>(
                        query,
                        new
                        {
                            Filtro = filtroParametro,
                            SortField = sortField,
                            SortDirection = sortDirection,
                            OffsetRows = offsetRows,
                            FetchRows = fetchRows
                        }).ToList();
                }
                else
                {
                    salida.total = connection.QueryFirstOrDefault<int>(
                        @"Select COUNT(U.UT_Codigo)
                          from UTrabajo U
                          where @Filtro is null
                             or U.UT_Descripcion like @Filtro
                             or U.UT_Codigo like @Filtro",
                        new { Filtro = filtroParametro });

                    var query = @"
                        Select U.UT_Codigo as Codigo,
                               U.UT_Descripcion as Descripcion,
                               '' as Provincia,
                               '' as ProvinciaDesc
                        from UTrabajo U
                        where @Filtro is null
                           or U.UT_Descripcion like @Filtro
                           or U.UT_Codigo like @Filtro
                        order by
                            CASE WHEN @SortField = 'Codigo' AND @SortDirection = 'ASC' THEN U.UT_Codigo END ASC,
                            CASE WHEN @SortField = 'Codigo' AND @SortDirection = 'DESC' THEN U.UT_Codigo END DESC,
                            CASE WHEN @SortField = 'Descripcion' AND @SortDirection = 'ASC' THEN U.UT_Descripcion END ASC,
                            CASE WHEN @SortField = 'Descripcion' AND @SortDirection = 'DESC' THEN U.UT_Descripcion END DESC,
                            U.UT_Codigo ASC";

                    if (fetchRows > 0)
                    {
                        query += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                    }

                    salida.lista = connection.Query<AfUnidadesDto>(
                        query,
                        new
                        {
                            Filtro = filtroParametro,
                            SortField = sortField,
                            SortDirection = sortDirection,
                            OffsetRows = offsetRows,
                            FetchRows = fetchRows
                        }).ToList();
                }

                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener unidades.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        /// <summary>
        /// Obtener unidad por codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="rbTipo"></param>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        public ErrorDto<AfUnidadesDto> AF_Unidades_BuscarPorCodigo_Obtener(int CodEmpresa, int rbTipo, string Codigo)
        {
            var query = rbTipo == 0
                ? @"Select U.Codigo, U.Descripcion, P.Descripcion as ProvinciaDesc, isnull(P.Provincia,'') as Provincia
                    from UProgramatica U
                    left join Provincias P on U.Provincia = P.Provincia
                    Where U.Codigo = @Codigo"
                : @"Select UT_Codigo as CODIGO, UT_DESCRIPCION as DESCRIPCION, '' as ProvinciaDesc, '' as PROVINCIA
                    from UTrabajo
                    Where UT_Codigo = @Codigo";

            var result = DbHelper.ExecuteSingleQuery<AfUnidadesDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                new { Codigo });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfUnidadesDto())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener unidad.", result.Code.GetValueOrDefault(-1), new AfUnidadesDto());
        }

        /// <summary>
        /// Guardar unidad, actualiza o inserta segun corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="rbTipo"></param>
        /// <param name="Editar"></param>
        /// <param name="Info"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Unidades_Guardar(int CodEmpresa, int rbTipo, bool Editar, AfUnidadesDto Info, string Usuario)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de la unidad son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                GuardarUnidad(connection, CodEmpresa, rbTipo, Editar, Info, Usuario));

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar unidad.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Eliminar unidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="rbTipo"></param>
        /// <param name="Codigo"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Unidades_Eliminar(int CodEmpresa, int rbTipo, string Codigo, string Usuario)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                rbTipo == 0
                    ? "Delete From UProgramatica where Codigo = @Codigo"
                    : "Delete From UTrabajo where UT_Codigo = @Codigo",
                new { Codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar unidad.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    Usuario,
                    $"Elimino Unidad {(rbTipo == 0 ? "Programatica" : "Trabajo")} {Codigo}",
                    "Borra - WEB");

                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }
        private ErrorDto GuardarUnidad(SqlConnection connection, int codEmpresa, int rbTipo, bool editar, AfUnidadesDto info, string usuario)
        {
            if (rbTipo == 0)
            {
                connection.Execute(
                    editar
                        ? @"Update UProgramatica Set Codigo = @Codigo, Descripcion = @Descripcion, Provincia = @Provincia Where Codigo = @Codigo"
                        : @"Insert UProgramatica (Codigo,Descripcion,Provincia) Values(@Codigo, @Descripcion, @Provincia)",
                    new
                    {
                        Codigo = info.codigo.Trim(),
                        Descripcion = info.descripcion.Trim(),
                        Provincia = info.provincia
                    });

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    $"{(editar ? "Modifico" : "Registro")} Unidad Programatica {info.codigo}",
                    editar ? "Modifica - WEB" : "Registra - WEB");

                return DbHelper.OkResponse("Ok");
            }

            connection.Execute(
                editar
                    ? @"Update UTrabajo Set UT_Codigo = @Codigo, UT_Descripcion = @Descripcion Where UT_Codigo = @Codigo"
                    : @"Insert into UTrabajo (UT_Codigo,UT_Descripcion) Values(@Codigo, @Descripcion)",
                new
                {
                    Codigo = info.codigo.Trim(),
                    Descripcion = info.descripcion.Trim()
                });

            RegistrarBitacora(
                codEmpresa,
                usuario,
                $"{(editar ? "Modifico" : "Registro")} Unidad Trabajo {info.codigo}",
                editar ? "Modifica - WEB" : "Registra - WEB");

            return DbHelper.OkResponse("Ok");
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        private static string ObtenerSortFieldUnidades(int rbTipo, string? sortField)
        {
            if (rbTipo == 0)
            {
                return sortField switch
                {
                    CampoCodigo => CampoCodigo,
                    CampoDescripcion => CampoDescripcion,
                    "Provincia" => "Provincia",
                    "ProvinciaDesc" => "ProvinciaDesc",
                    _ => CampoDescripcion
                };
            }

            return sortField switch
            {
                CampoCodigo => CampoCodigo,
                CampoDescripcion => CampoDescripcion,
                _ => CampoDescripcion
            };
        }

        private static string ObtenerSortDirectionUnidades(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}