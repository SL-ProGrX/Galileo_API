using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdMiembrosJuntaDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDB;
        private readonly int vModulo = 40;

        public FrmAfCdMiembrosJuntaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de directores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de directores.</returns>
        public ErrorDto<List<AfCdDirectorDto>> AfCdDirectores_Lista(int codEmpresa)
        {
            var sql = @"
                SELECT COALESCE(cod_director, 1) AS Cod_Director, Nombre, puesto AS Puesto, Activo
                FROM afi_cd_directores";

            return DbHelper.ExecuteListQuery<AfCdDirectorDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Valida si un director está asociado a algún comité.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codDirector">Código del director a validar.</param>
        /// <returns>Lista de comités asociados al director.</returns>
        public ErrorDto<List<AfCdComiteDirectorDto>> AfCdDirectores_ValidarComite(int codEmpresa, int codDirector)
        {
            var sql = @"
                SELECT cod_comite AS Cod_Comite, descripcion AS Descripcion, cod_director AS Cod_Director
                FROM afi_cd_comites
                WHERE cod_director = @Cod_Director";

            var parameters = new { Cod_Director = codDirector };

            return DbHelper.ExecuteListQuery<AfCdComiteDirectorDto>(
                _portalDb,
                codEmpresa,
                sql,
                parameters
            );
        }

        /// <summary>
        /// Inserta o actualiza un director y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos del director.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdDirectores_Guardar(int codEmpresa, AfCdDirectorSaveDto dto)
        {
            if (dto.Cod_Director == 0)
            {
                // Insertar: calcular nuevo código
                var nuevoCodigo = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.QueryFirstOrDefault<int>(
                        "SELECT COALESCE(MAX(cod_director),0) + 1 FROM afi_cd_directores"
                    )
                );
                dto.Cod_Director = nuevoCodigo.Result;

                var sql = @"
                    INSERT INTO afi_cd_directores(cod_director, nombre, puesto, activo)
                    VALUES(@Cod_Director, @Nombre, @Puesto, @Activo)";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });

                // Bitácora
                var detalle = $"Directores: {dto.Nombre} Ced: {dto.Cedula ?? ""} ID.{dto.Cod_Director}";
                _securityMainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = dto.Usuario.ToUpper(),
                    DetalleMovimiento = detalle,
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            else
            {
                // Actualizar
                var sql = @"
                    UPDATE afi_cd_directores
                    SET nombre = @Nombre,
                        puesto = @Puesto,
                        activo = @Activo
                    WHERE cod_director = @Cod_Director";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });

                // Bitácora
                var detalle = $"Directores: {dto.Nombre} ID: {dto.Cedula ?? ""} ID.{dto.Cod_Director}";
                _securityMainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = dto.Usuario.ToUpper(),
                    DetalleMovimiento = detalle,
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Elimina un director y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codDirector">Código del director a eliminar.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdDirectores_Eliminar(int codEmpresa, int codDirector, string usuario)
        {
            // Obtener datos para bitácora antes de eliminar
            var director = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<AfCdDirectorDto>(
                    "SELECT cod_director AS Cod_Director, nombre AS Nombre FROM afi_cd_directores WHERE cod_director = @Cod_Director",
                    new { Cod_Director = codDirector }
                )
            )?.Result;

            var sql = @"DELETE FROM afi_cd_directores WHERE cod_director = @Cod_Director";
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Director = codDirector });
                return true;
            });

            // Bitácora
            var detalle = $"Directores: {(director?.Nombre ?? "")} ID.{codDirector}";
            _securityMainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = "Elimina - WEB",
                Modulo = vModulo
            });

            return new ErrorDto<bool> { Result = true };
        }
    }
}
