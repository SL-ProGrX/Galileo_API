using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifOficinasBD
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string UnexpectedErrorMessage = "Error inesperado";
        private const string GenericErrorMessage = "Error";


        public FrmSifOficinasBD(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private ErrorDto<T> WithEmpresaConn<T>(int codEmpresa, Func<SqlConnection, T> action)
            => DbHelper.WithConn(_portalDB, codEmpresa, action);

        private ErrorDto TryBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? UnexpectedErrorMessage);
            }
        }

        private static string? ToSearchLike(FiltrosLazyLoadData? filtros)
        {
            var search = filtros?.filtro?.Trim();
            return string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";
        }

        private static (string sortField, int sortOrder) ToSort(FiltrosLazyLoadData? filtros)
        {
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC
            return (sortField, sortOrder);
        }

        private static (int offset, int fetch) ToPage(FiltrosLazyLoadData? filtros)
        {
            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;
            if (fetch <= 0) fetch = int.MaxValue;
            return (offset, fetch);
        }

        private const string OficinasSelectBase = @"
SELECT cod_oficina,descripcion,COD_UNIDAD,Cod_Centro_Costo,Telefono_01,Telefono_02,DIRECCION,
       Registro_Usuario,Registro_Fecha, Tipo,Oficina_Omision,Estado
FROM Sif_Oficinas
WHERE (@search IS NULL
       OR cod_oficina LIKE @search
       OR descripcion LIKE @search
       OR Registro_Usuario LIKE @search)";

        private const string OficinasOrderByPaged = @"
ORDER BY
    -- ASC
    CASE WHEN @sortOrder = 1 AND @sortField = 'cod_oficina' THEN cod_oficina END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_01' THEN Telefono_01 END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_02' THEN Telefono_02 END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'direccion' THEN DIRECCION END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario' THEN Registro_Usuario END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN Registro_Fecha END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'tipo' THEN Tipo END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'oficina_omision' THEN Oficina_Omision END ASC,
    CASE WHEN @sortOrder = 1 AND @sortField = 'estado' THEN Estado END ASC,

    -- DESC
    CASE WHEN @sortOrder = 0 AND @sortField = 'cod_oficina' THEN cod_oficina END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_01' THEN Telefono_01 END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_02' THEN Telefono_02 END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'direccion' THEN DIRECCION END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario' THEN Registro_Usuario END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN Registro_Fecha END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'tipo' THEN Tipo END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'oficina_omision' THEN Oficina_Omision END DESC,
    CASE WHEN @sortOrder = 0 AND @sortField = 'estado' THEN Estado END DESC,

    cod_oficina ASC
OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private List<SifOficinasData> QueryOficinas(SqlConnection connection, FiltrosLazyLoadData? filtros, bool paged)
        {
            var searchLike = ToSearchLike(filtros);
            var (sortField, sortOrder) = ToSort(filtros);
            var (offset, fetch) = paged ? ToPage(filtros) : (0, int.MaxValue);

            var sql = paged
                ? $"{OficinasSelectBase}\n{OficinasOrderByPaged}"
                : $"{OficinasSelectBase} ORDER BY cod_oficina ASC";

            return connection.Query<SifOficinasData>(sql, new
            {
                search = searchLike,
                sortField,
                sortOrder,
                offset,
                fetch
            }).ToList();
        }

        private ErrorDto ExecuteActualizar(SqlConnection connection, int codEmpresa, SifOficinasData oficinaDatos)
        {
            const string sql = @"update sif_oficinas
                    set descripcion = @Descripcion,
                        cod_unidad = @Cod_unidad,
                        cod_centro_costo= @Cod_centro_costo,
                        Tipo= @tipo,
                        Oficina_Omision= @Oficina_Omision,
                        Estado= @estado,
                        Telefono_01 = @telefono1,
                        Telefono_02 = @telefono2,
                        direccion = @Direccion
                    WHERE cod_oficina = @cod_oficina";

            connection.Execute(sql, new
            {
                cod_oficina = oficinaDatos.cod_oficina.Trim(),
                Descripcion = oficinaDatos.descripcion,
                Cod_unidad = oficinaDatos.cod_unidad,
                Cod_centro_costo = oficinaDatos.cod_centro_costo,
                estado = oficinaDatos.estado,
                Tipo = oficinaDatos.tipo,
                Oficina_Omision = oficinaDatos.oficina_omision,
                telefono1 = oficinaDatos.telefono_01,
                telefono2 = oficinaDatos.telefono_02,
                Direccion = oficinaDatos.direccion
            });

            var bit = TryBitacora(codEmpresa, oficinaDatos.registro_usuario, $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}", "Modifica - WEB");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }

        private ErrorDto ExecuteInsertar(SqlConnection connection, int codEmpresa, SifOficinasData oficinaDatos)
        {
            const string sql = @"insert into sif_oficinas(cod_oficina,descripcion,cod_unidad,cod_centro_costo,Tipo,Oficina_Omision,Estado,Telefono_01,Telefono_02,Direccion,registro_fecha,registro_usuario)
                    VALUES (@cod_oficina, @Descripcion,@Cod_unidad,@Cod_centro_costo,@Tipo,@Oficina_Omision, @estado,@telefono1,@telefono2,@Direccion, dbo.MyGetdate(), @usuario)";

            connection.Execute(sql, new
            {
                cod_oficina = oficinaDatos.cod_oficina.Trim(),
                Descripcion = oficinaDatos.descripcion,
                Cod_unidad = oficinaDatos.cod_unidad,
                Cod_centro_costo = oficinaDatos.cod_centro_costo,
                Tipo = oficinaDatos.tipo,
                Oficina_Omision = oficinaDatos.oficina_omision,
                telefono1 = oficinaDatos.telefono_01,
                telefono2 = oficinaDatos.telefono_02,
                Direccion = oficinaDatos.direccion,
                estado = oficinaDatos.estado,
                usuario = oficinaDatos.registro_usuario
            });

            var bit = TryBitacora(codEmpresa, oficinaDatos.registro_usuario, $"Oficina: {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}", "Registra - WEB");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }

        /// <summary>
        ///  Método para consultar lista de oficinas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifOficinasLista> Sif_OficinasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                const string totalQuery = @"SELECT COUNT(cod_oficina) FROM Sif_Oficinas";
                var total = connection.Query<int>(totalQuery).FirstOrDefault();

                return new SifOficinasLista
                {
                    total = total,
                    lista = QueryOficinas(connection, filtros, paged: true)
                };
            });
        }


        /// <summary>
        /// Método para consultar lista unidades contables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasUnidadContable_Obtener(int CodEmpresa)
        {
            const string query = @"select cod_unidad as 'item',descripcion from CntX_Unidades";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query);
        }
        
        
        /// <summary>
        /// Método para consultar lista de centros de costo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasCentroCostos_Obtener(int CodEmpresa)
        {
            const string query = @"select cod_centro_costo as 'item',descripcion from CNTX_CENTRO_COSTOS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query);
        }
        
        
        /// <summary>
        /// Método para consultar lista de oficinas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_Oficinas_Lista(int CodEmpresa)
        {
            const string query = @"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
                                     from  SIF_Oficinas  where estado = 1 order by cod_oficina";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query);
        }


        /// <summary>
        /// Actualiza los datos de la oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficinaDatos"></param>
        /// <returns></returns>

        public ErrorDto Sif_Oficinas_ActualizarDatos(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            var r = WithEmpresaConn(CodEmpresa, connection =>
            {
                const string sql = @"update SIF_Oficinas
                        set Telefono_01 = @telefono1,
                            Telefono_02 = @telefono2,
                            direccion = @direccion
                        WHERE cod_oficina = @cod_oficina";

                connection.Execute(sql, new
                {
                    cod_oficina = oficinaDatos.cod_oficina.Trim(),
                    telefono1 = oficinaDatos.telefono_01,
                    telefono2 = oficinaDatos.telefono_02,
                    direccion = oficinaDatos.direccion
                });

                var bit = TryBitacora(CodEmpresa, oficinaDatos.registro_usuario, $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}", "Modifica - WEB");
                if ((bit.Code ?? -1) != 0)
                    return bit;

                return DbHelper.CreateOkResponse();
            });

            if ((r.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(r.Description ?? GenericErrorMessage, r.Code ?? -1);

            return r.Result ?? DbHelper.ErrorResponse(UnexpectedErrorMessage);
        }
        
        
        /// <summary>
        /// Método para guardar oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficinaDatos"></param>
        /// <returns></returns>
        public ErrorDto Sif_Oficinas_Guardar(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            var res = WithEmpresaConn(CodEmpresa, connection =>
            {
                // Verifico si existe unidad contable
                const string qUnidad = @"SELECT COUNT(cod_unidad) FROM CntX_Unidades WHERE ACTIVA = 1 AND cod_unidad = @cod_unidad";
                int existeunidad = connection.QueryFirstOrDefault<int>(qUnidad, new { cod_unidad = (oficinaDatos.cod_unidad ?? string.Empty).Trim() });
                if (existeunidad == 0)
                {
                    return new ErrorDto { Code = -2, Description = $"La unidad contable {oficinaDatos.cod_unidad} no existe o no está activo." };
                }

                // Verifico si existe centro de costo
                const string qCentroCosto = @"SELECT COUNT(cod_centro_costo) FROM CNTX_CENTRO_COSTOS WHERE ACTIVO = 1 AND cod_centro_costo = @cod_centro_costo";
                int existeCentroCosto = connection.QueryFirstOrDefault<int>(qCentroCosto, new { cod_centro_costo = (oficinaDatos.cod_centro_costo ?? string.Empty).Trim() });
                if (existeCentroCosto == 0)
                {
                    return new ErrorDto { Code = -2, Description = $"El centro de costo {oficinaDatos.cod_centro_costo} no existe o no está activo." };
                }

                // Verifico si existe la oficina
                const string qExiste = @"select isnull(count(*),0) as Existe from sif_oficinas where cod_oficina = @Cod_oficina";
                var existe = connection.QueryFirstOrDefault<int>(qExiste, new { Cod_oficina = oficinaDatos.cod_oficina });

                if (oficinaDatos.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto { Code = -2, Description = $"La oficina con el código {oficinaDatos.cod_oficina} ya existe." };

                    return ExecuteInsertar(connection, CodEmpresa, oficinaDatos);
                }

                if (existe == 0)
                    return new ErrorDto { Code = -2, Description = $"La oficina con el código {oficinaDatos.cod_oficina} no existe." };

                return ExecuteActualizar(connection, CodEmpresa, oficinaDatos);
            });

            if ((res.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(res.Description ?? GenericErrorMessage, res.Code ?? -1);

            return res.Result ?? DbHelper.ErrorResponse(UnexpectedErrorMessage);
        }


        // Removed unused private methods Sif_Oficinas_Actualizar and Sif_Oficinas_Insertar


        /// <summary>
        /// Medoto para consultar lista de miembros de una oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="filtro"></param>
        /// <param name="apoyo"></param>
        /// <param name="usuariosEstado"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasMiembros>> Sif_OficinasMiembros_Lista(int CodEmpresa, string oficina, string filtro, int apoyo, int usuariosEstado)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                const string sp = "spSys_Oficinas_Miembros_Consultas";
                return connection.Query<SifOficinasMiembros>(sp, new
                {
                    oficina,
                    filtro = (filtro ?? string.Empty).Trim(),
                    apoyo,
                    pendiente = usuariosEstado, // Si usuariosEstado es 0, busco pendientes (no aprobados), si es 1 busco aprobados
                }, commandType: CommandType.StoredProcedure).ToList();
            });
        }
        
        
        /// <summary>
        /// Método para agregar miembros a una oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="usuario"></param>
        /// <param name="apoyo"></param>
        /// <param name="usuarioRegistro"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sif_OficinasMiembros_Agregar(int CodEmpresa, string oficina, string usuario, int apoyo, string usuarioRegistro, string accion)
        {
            var r = WithEmpresaConn(CodEmpresa, connection =>
            {
                const string sp = "spSys_Oficinas_Miembros_Add";
                return connection.Query<int>(sp, new
                {
                    oficina,
                    usuario = (usuario ?? string.Empty).Trim(),
                    apoyo,
                    usuarioRegistro,
                    accion
                }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            });

            if ((r.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(r.Description ?? GenericErrorMessage, r.Code ?? -1);

            return new ErrorDto { Code = r.Result, Description = "Ok" };
        }


        /// <summary>
        /// Medoto para consultar historial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasHistorial>> Sif_OficinasHistorial_Lista(int CodEmpresa, string filtro)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                const string query = @"select * from dbo.SIF_OFICINA_MIEMBROS_H where usuario = @usuario order by cod_historial desc";
                return connection.Query<SifOficinasHistorial>(query, new { usuario = filtro }).ToList();
            });
        }


        /// <summary>
        /// Método para consultar datos a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasData>> Sif_Oficinas_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
                QueryOficinas(connection, filtros, paged: false));
        }
    }
}