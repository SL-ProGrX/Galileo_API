using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXContabilidadesUsuariosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXContabilidadesUsuariosDb(IConfiguration config)
            : this(new PortalDB(config))
        { }

        public FrmCntXContabilidadesUsuariosDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene el catalogo de contabilidades o usuarios dependiendo del parametro recibido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="obtenerUsuarios"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerCatalogo(
            int codEmpresa, bool obtenerUsuarios)
        {
            var sql = obtenerUsuarios
                ? @"exec [PGX_Portal].[dbo].[spCntX_UsuariosAutorizados] @codEmpresa"
                : @"select cast(cod_contabilidad as varchar) as idX,
                            nombre as itmX,
                            0 as marca
                     from CntX_Contabilidades";

            var param = obtenerUsuarios ? new { codEmpresa } : null;

            return DbHelper.ExecuteListQuery<CntXContabilidadUsuarioData>(
                _portalDb,
                codEmpresa,
                sql,
                param
            );
        }

        /// <summary>
        /// Obtiene las relaciones entre contabilidades y usuarios dependiendo del parámetro recibido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="porContabilidad"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerRelaciones(
            int codEmpresa, bool porContabilidad, string valor)
        {
            var sql = porContabilidad
                ? @"
                    select rtrim(U.Nombre) as idX,
                           rtrim(U.Nombre) as itmX,
                           isnull(A.cod_contabilidad, 0) as marca
                    from vCntX_UsuariosAutorizados U
                    left join CNTX_CONTA_USUARIOS A
                           on U.nombre = A.usuario
                          and A.cod_contabilidad = @valor"
                : @"
                    select cast(I.cod_contabilidad as varchar) as idX,
                           I.nombre as itmX,
                           isnull(A.cod_contabilidad, 0) as marca
                    from CntX_Contabilidades I
                    left join CNTX_CONTA_USUARIOS A
                           on I.cod_contabilidad = A.cod_contabilidad
                          and A.usuario = @valor";

            object param = porContabilidad
                ? new { valor = int.Parse(valor) }
                : new { valor };

            return DbHelper.ExecuteListQuery<CntXContabilidadUsuarioData>(
                _portalDb,
                codEmpresa,
                sql,
                param
            );
        }

        /// <summary>
        /// Guarda la relacion entre una contabilidad y un usuario
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="usuario"></param>
        /// <param name="usuarioRegistro"></param>
        /// <returns></returns>
        public ErrorDto CntXContaUser_GuardarRelacion(
            int codEmpresa, int codContabilidad, string usuario, string usuarioRegistro)
        {
            const string sql = @"
                insert CNTX_CONTA_USUARIOS
                (
                    cod_contabilidad,
                    usuario,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @codContabilidad,
                    @usuario,
                    getdate(),
                    @usuarioRegistro
                )";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    codContabilidad,
                    usuario,
                    usuarioRegistro
                }
            );
        }

        /// <summary>
        /// Elimina la relacion entre una contabilidad y un usuario
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXContaUser_EliminarRelacion(
            int codEmpresa, int? codContabilidad = null, string? usuario = null)
        {
            string sql;
            object param;

            if (codContabilidad.HasValue && !string.IsNullOrWhiteSpace(usuario))
            {
                sql = @"
                    delete CNTX_CONTA_USUARIOS
                    where cod_contabilidad = @codContabilidad
                      and usuario = @usuario";

                param = new
                {
                    codContabilidad,
                    usuario
                };
            }
            else if (codContabilidad.HasValue)
            {
                sql = @"
                    delete CNTX_CONTA_USUARIOS
                    where cod_contabilidad = @codContabilidad";

                param = new
                {
                    codContabilidad
                };
            }
            else if (!string.IsNullOrWhiteSpace(usuario))
            {
                sql = @"
                    delete CNTX_CONTA_USUARIOS
                    where usuario = @usuario";

                param = new
                {
                    usuario
                };
            }
            else
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar codContabilidad, usuario, o ambos."
                };
            }

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                param
            );
        }
    }
}