using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoGruposDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _mSecurity;
        private readonly int vModulo = 4;

        public FrmCoGruposDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCoGruposDb(PortalDB portalDB, MSecurityMainDb mSecurity)
        {
            _portalDB = portalDB;
            _mSecurity = mSecurity;
        }

        /// <summary>
        /// Obtiene la lista de Grupos de Cobros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoGruposData>> CO_Grupos_Obtener(int CodEmpresa)
        {
            const string sqlGrupos = @"select ID_GRUPO, DESCRIPCION, convert(int,ACTIVO) as 'ACTIVO'
                from CBR_GRUPOS order by ID_GRUPO";

            return DbHelper.ExecuteListQuery<CoGruposData>(
                _portalDB, CodEmpresa, sqlGrupos);
        }

        /// <summary>
        /// Guarda o Actualiza un Grupo de Cobros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CO_Grupos_Guardar(int CodEmpresa, CoGruposData data)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var descripcion = (data.descripcion ?? "").Trim();
                var activo = data.activo ? 1 : 0; 
                var usuario = (data.usuario ?? "").Trim().ToUpper();

                if (data.id_grupo == 0)
                {
                    const string insertSql = @"
                        insert into CBR_GRUPOS (DESCRIPCION, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
                        values (@Descripcion, @Activo, @Usuario, GETDATE());";

                    connection.Execute(insertSql, new
                    {
                        Descripcion = descripcion,
                        Activo = activo,
                        Usuario = usuario
                    });

                    const string getIdSql = @"select isnull(max(ID_GRUPO), 0) from CBR_GRUPOS;";
                    var newId = connection.QuerySingle<int>(getIdSql);

                    _mSecurity.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Movimiento = "Registra - WEB",
                        DetalleMovimiento = $"Grupo de Cobros: {newId}",
                        Modulo = vModulo
                    });
                }
                else
                {
                    const string updateSql = @"
                        update CBR_GRUPOS
                           set DESCRIPCION = @Descripcion,
                               ACTIVO = @Activo,
                               MODIFICA_FECHA = GETDATE(),
                               MODIFICA_USUARIO = @Usuario
                         where ID_GRUPO = @IdGrupo;
";

                    connection.Execute(updateSql, new
                    {
                        Descripcion = descripcion,
                        Activo = activo,
                        Usuario = usuario,
                        IdGrupo = data.id_grupo
                    });

                    _mSecurity.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Movimiento = "Modifica - WEB",
                        DetalleMovimiento = $"Grupo de Cobros: {data.id_grupo}",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina un Grupo de Cobros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="IdGrupo"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto CO_Grupos_Eliminar(int CodEmpresa, int GrupoId, string Usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"exec spCbr_Grupos_Elimina @GrupoId, @Usuario";

                var spResp = connection.QueryFirstOrDefault(sql, new
                {
                    GrupoId,
                    Usuario = (Usuario ?? "").Trim().ToUpper()
                });

                if (spResp == null)
                {
                    result.Code = -1;
                    result.Description = "El procedimiento no retorn&oacute; respuesta.";
                    return result;
                }

                if (spResp.Pass == 1)
                {
                    _mSecurity.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = (Usuario ?? "").Trim().ToUpper(),
                        Movimiento = "Elimina - WEB",
                        DetalleMovimiento = $"Grupo de Cobros: {GrupoId}",
                        Modulo = vModulo
                    });

                    result.Description = $"Grupo de Cobros: {GrupoId}, Eliminado Satisfactoriamente!";
                }
                else
                {
                    result.Code = -1;
                    result.Description = spResp.Mensaje ?? "No se pudo eliminar el Grupo de Cobros.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Obtiene lista de asignaciones para un grupo de cobros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CoGruposAsignacionData>> CO_Grupos_Asignacion_Obtener(int CodEmpresa, CoGruposAsignacionFiltros filtros)
        {
            const string sql = @"exec spCbr_Grupos_List_Asignacion @GrupoId, @Tipo, @Filtro";

            return DbHelper.ExecuteListQuery<CoGruposAsignacionData>(
            _portalDB,
            CodEmpresa,
            sql,
            new
            {
                GrupoId = filtros.id_grupo,
                Tipo = filtros.tipo,
                Filtro = (filtros.filtro ?? "").Trim()
            });
        }

        /// <summary>
        /// Asigna o Remueve un item a un grupo de cobros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="GrupoId"></param>
        /// <param name="Tipo"></param>
        /// <param name="Codigo"></param>
        /// <param name="IsChecked"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto CO_Grupos_Asignar(int CodEmpresa, int GrupoId, string Tipo, string Codigo, bool IsChecked, string Usuario)
        {
            var accion = IsChecked ? "A" : "E";

            const string sql = @"exec spCbr_Grupos_List_Asignacion_Add @GrupoId, @Tipo, @Codigo, @Usuario, @Mov";

            return DbHelper.ExecuteNonQuery(
            _portalDB,
            CodEmpresa,
            sql,
            new
            {
                GrupoId,
                Tipo,
                Codigo = (Codigo ?? "").Trim(),
                Usuario = (Usuario ?? "").Trim().ToUpper(),
                Mov = accion
            });
        }
    }
}
