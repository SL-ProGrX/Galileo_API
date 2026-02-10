using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.SYS;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysNacionalidadesDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        private const string ErrorValidarNacionalidad = "Error al validar nacionalidad.";

        public FrmSysNacionalidadesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private ErrorDto TryLogBitacora(int empresaId, string usuario, string movimiento, string detalle)
        {
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = empresaId,
                    Usuario = NormalizeUpper(usuario),
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }
        }

        private ErrorDto<int> CountByCodigo(int codEmpresa, string codNacionalidad)
        {
            const string sql = @"SELECT COUNT(*) FROM SYS_NACIONALIDADES WHERE UPPER(COD_NACIONALIDAD) = @cod";
            return DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, new
            {
                cod = NormalizeUpper(codNacionalidad)
            });
        }

        /// <summary>
        /// Obtiene una lista de nacionalidades con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysNacionalidadesLista> Sys_NacionalidadesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = _portalDB;

            // Mapa de columnas permitidas para ordenar (evita SQL dinámico)
            var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["cod_nacionalidad"] = 1,
                ["descripcion"] = 2,
                ["cod_inter"] = 3,
                ["omision"] = 4,
                ["activo"] = 5,
                ["registro_fecha"] = 6,
                ["registro_usuario"] = 7,
                ["item"] = 1
            };

            return DbHelper.WithConn(portalDb, CodEmpresa, conn =>
            {
                // Total (mantiene comportamiento anterior: total sin filtro)
                const string sqlTotal = @"select COUNT(COD_NACIONALIDAD) from SYS_NACIONALIDADES";
                int total = conn.QueryFirstOrDefault<int>(sqlTotal);

                // Lazy load (filtro + orden + paginación)
                var spec = LazyLoadHelper.Build(filtros, sortMap, defaultSort: "cod_nacionalidad");

                const string sql = @"
                    select COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario
                    from SYS_NACIONALIDADES
                    where (@hasFilter = 0 or (
                        COD_NACIONALIDAD like @filtro
                        or descripcion like @filtro
                        or cod_inter like @filtro
                        or Registro_Usuario like @filtro
                    ))
                    order by
                        -- ASC
                        case when @isAsc = 1 and @sortCode = 1 then COD_NACIONALIDAD end asc,
                        case when @isAsc = 1 and @sortCode = 2 then descripcion end asc,
                        case when @isAsc = 1 and @sortCode = 3 then cod_inter end asc,
                        case when @isAsc = 1 and @sortCode = 4 then convert(int, omision) end asc,
                        case when @isAsc = 1 and @sortCode = 5 then convert(int, activo) end asc,
                        case when @isAsc = 1 and @sortCode = 6 then Registro_Fecha end asc,
                        case when @isAsc = 1 and @sortCode = 7 then Registro_Usuario end asc,

                        -- DESC
                        case when @isAsc = 0 and @sortCode = 1 then COD_NACIONALIDAD end desc,
                        case when @isAsc = 0 and @sortCode = 2 then descripcion end desc,
                        case when @isAsc = 0 and @sortCode = 3 then cod_inter end desc,
                        case when @isAsc = 0 and @sortCode = 4 then convert(int, omision) end desc,
                        case when @isAsc = 0 and @sortCode = 5 then convert(int, activo) end desc,
                        case when @isAsc = 0 and @sortCode = 6 then Registro_Fecha end desc,
                        case when @isAsc = 0 and @sortCode = 7 then Registro_Usuario end desc,

                        -- Fallback
                        COD_NACIONALIDAD asc
                    offset @offset rows fetch next @pageSize rows only;";

                var lista = conn.Query<SysNacionalidadesData>(sql, spec.Params).ToList();

                return new SysNacionalidadesLista
                {
                    total = total,
                    lista = lista
                };
            });
        }


        /// <summary>
        /// Obtiene una lista de nacionalidades sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysNacionalidadesData>> Sys_Nacionalidades_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = _portalDB;

            var raw = (filtros?.filtro ?? string.Empty).Trim();
            string? q = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";

            const string sql = @"
                SELECT COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario
                FROM SYS_NACIONALIDADES
                WHERE (@q IS NULL OR (
                      COD_NACIONALIDAD LIKE @q
                      OR descripcion LIKE @q
                      OR Registro_Usuario LIKE @q
                ))
                ORDER BY COD_NACIONALIDAD";

            return DbHelper.ExecuteListQuery<SysNacionalidadesData>(portalDb, CodEmpresa, sql, new { q });
        }


        private ErrorDto ExecuteWrite(int CodEmpresa, string usuario, string movimiento, string detalle, string sql, object parameters)
        {
            var exec = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parameters);

            if ((exec.Code ?? -1) == 0)
            {
                var bit = TryLogBitacora(CodEmpresa, usuario, movimiento, detalle);
                if ((bit.Code ?? -1) != 0)
                    return bit;
            }

            return exec;
        }

        /// <summary>
        /// Inserta una nueva nacionalidad.
        /// </summary>
        private ErrorDto Sys_Nacionalidades_Insertar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
        {
            const string sql = @"INSERT INTO SYS_NACIONALIDADES
                    (COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario)
                    VALUES (@cod_nacionalidad, @descripcion, @cod_inter, @omision, @activo, GETDATE(), @registro_usuario)";

            var detalle = $"Nacionalidad: {nacionalidad.cod_nacionalidad} - {nacionalidad.descripcion}";

            return ExecuteWrite(CodEmpresa, usuario, "Registra - WEB", detalle, sql, new
            {
                cod_nacionalidad = NormalizeUpper(nacionalidad.cod_nacionalidad),
                descripcion = nacionalidad.descripcion,
                cod_inter = nacionalidad.cod_inter,
                omision = nacionalidad.omision,
                activo = nacionalidad.activo,
                registro_usuario = NormalizeUpper(usuario)
            });
        }


        /// <summary>
        /// Actualiza una nacionalidad existente.
        /// </summary>
        private ErrorDto Sys_Nacionalidades_Actualizar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
        {
            const string sql = @"UPDATE SYS_NACIONALIDADES
                    SET descripcion = @descripcion,
                        cod_inter = @cod_inter,
                        omision = @omision,
                        activo = @activo,
                        Registro_Usuario = @registro_usuario,
                        Registro_Fecha = GETDATE()
                    WHERE COD_NACIONALIDAD = @cod_nacionalidad";

            var detalle = $"Nacionalidad: {nacionalidad.cod_nacionalidad} - {nacionalidad.descripcion}";

            return ExecuteWrite(CodEmpresa, usuario, "Modifica - WEB", detalle, sql, new
            {
                cod_nacionalidad = NormalizeUpper(nacionalidad.cod_nacionalidad),
                descripcion = nacionalidad.descripcion,
                cod_inter = nacionalidad.cod_inter,
                omision = nacionalidad.omision,
                activo = nacionalidad.activo,
                registro_usuario = NormalizeUpper(usuario)
            });
        }


        /// <summary>
        /// Inserta o actualiza una nacionalidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Guardar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
        {
            var valida = Sys_Nacionalidades_Valida(CodEmpresa, nacionalidad);

            if (nacionalidad.isNew)
            {
                if ((valida.Code ?? -1) != 0)
                    return DbHelper.ErrorResponse(valida.Description ?? ErrorValidarNacionalidad, -2);

                return Sys_Nacionalidades_Insertar(CodEmpresa, usuario, nacionalidad);
            }

            // Para actualizar, solo valida que exista por código
            var existe = CountByCodigo(CodEmpresa, nacionalidad.cod_nacionalidad);

            if ((existe.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(existe.Description ?? ErrorValidarNacionalidad, -1);

            if (existe.Result <= 0)
                return DbHelper.ErrorResponse($"La nacionalidad con el código {nacionalidad.cod_nacionalidad} no existe.", -2);

            return Sys_Nacionalidades_Actualizar(CodEmpresa, usuario, nacionalidad);
        }


        /// <summary>
        /// Elimina una nacionalidad por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Eliminar(int CodEmpresa, string usuario, string cod_nacionalidad)
        {
            // Debe existir
            var existe = CountByCodigo(CodEmpresa, cod_nacionalidad);

            if ((existe.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(existe.Description ?? ErrorValidarNacionalidad, -1);

            if (existe.Result <= 0)
                return DbHelper.ErrorResponse($"La nacionalidad con el código {cod_nacionalidad} no existe.", -2);

            const string sql = @"DELETE FROM SYS_NACIONALIDADES WHERE UPPER(COD_NACIONALIDAD) = @cod";
            var detalle = $"Nacionalidad eliminada: {cod_nacionalidad}";

            return ExecuteWrite(CodEmpresa, usuario, "Elimina - WEB", detalle, sql, new
            {
                cod = NormalizeUpper(cod_nacionalidad)
            });
        }


        /// <summary>
        /// Valida si un código o descripción de nacionalidad ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Valida(int CodEmpresa, SysNacionalidadesData nacionalidad)
        {
            const string sql = @"SELECT COUNT(*)
                                 FROM SYS_NACIONALIDADES
                                 WHERE UPPER(COD_NACIONALIDAD) = @cod
                                    OR UPPER(descripcion) = @desc";

            var count = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, sql, 0, new
            {
                cod = NormalizeUpper(nacionalidad.cod_nacionalidad),
                desc = NormalizeUpper(nacionalidad.descripcion)
            });

            if (count.Code != 0)
                return DbHelper.ErrorResponse(count.Description ?? ErrorValidarNacionalidad, -1);

            if (count.Result > 0)
                return DbHelper.ErrorResponse("Ya existe una nacionalidad con ese código o descripción.", -1);

            return DbHelper.OkResponse("El código y la descripción de nacionalidad son válidos.");
        }

    }
}