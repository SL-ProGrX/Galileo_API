using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConPortalesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private readonly MCntXProfesionalDb _mCntXProfesional;
        private const int vModulo = 20;

        public FrmCntXConPortalesDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config),
                  new MCntXProfesionalDb(config))
        {
        }

        public FrmCntXConPortalesDb(
            PortalDB portalDb,
            MSecurityMainDb mSecurityMain,
            MCntXProfesionalDb mCntXProfesional)
        {
            _portalDb = portalDb;
            _mSecurityMain = mSecurityMain;
            _mCntXProfesional = mCntXProfesional;
        }

        /// <summary>
        /// Obtiene un portal por código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPortal"></param>
        /// <returns></returns>
        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Consulta_Obtener(int codEmpresa, int codPortal)
        {
            const string query = @"
            select top 1
                cod_portal,
                isnull(descripcion, '') as descripcion,
                isnull(observacion, '') as observacion,
                isnull(por_user, '') as por_user,
                isnull(por_password, '') as por_password,
                isnull(por_server, '') as por_server,
                isnull(por_database, '') as por_database,
                isnull(creado_usuario, '') as registro_usuario,
                isnull(convert(varchar(19), creado_fecha, 120), '') as registro_fecha,
                isnull(modificado_usuario, '') as actualiza_usuario,
                isnull(convert(varchar(19), modificado_fecha, 120), '') as actualiza_fecha
            from CNTX_CONSOLIDA_PORTALES
            where cod_portal = @codPortal;";

            var resp = DbHelper.ExecuteSingleQuery<CntXConPortalesDefinicionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { codPortal });

            if (resp.Code == -1 || resp.Result == null)
            {
                return new ErrorDto<CntXConPortalesDefinicionData?>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = null
                };
            }

            resp.Result.por_password = MCntXProfesionalDb.FxPortalCifrado(
                resp.Result.por_password,
                "D");

            return resp;
        }

        /// <summary>
        /// Obtiene el portal anterior o siguiente según el código actual.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPortalActual"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Scroll_Obtener(
            int codEmpresa, int codPortalActual, string direccion)
        {
            var dir = (direccion ?? string.Empty).Trim().ToLower();

            var query = dir == "siguiente"
                ? @"
                select top 1 cod_portal
                from CNTX_CONSOLIDA_PORTALES
                where cod_portal > @codPortalActual
                order by cod_portal asc;"
                    : @"
                select top 1 cod_portal
                from CNTX_CONSOLIDA_PORTALES
                where cod_portal < @codPortalActual
                order by cod_portal desc;";

            var resp = DbHelper.ExecuteSingleQuery<int?>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { codPortalActual });

            if (resp.Code == -1 || resp.Result == null)
            {
                return new ErrorDto<CntXConPortalesDefinicionData?>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = null
                };
            }

            return CntXConPortales_Consulta_Obtener(codEmpresa, resp.Result.Value);
        }

        /// <summary>
        /// Obtiene la lista de portales para búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConPortales_Lista_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cast(cod_portal as varchar(20)) as item,
                    isnull(descripcion, '') as descripcion
                from CNTX_CONSOLIDA_PORTALES
                order by cod_portal;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Prueba la conexión a un portal externo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXConPortales_ProbarConexion(
            int codEmpresa, CntXConPortalesConexionRequest request)
        {
            var connectionString = _mCntXProfesional.FxPortalPrueba(
                request.por_user,
                request.por_password,
                request.por_server,
                request.por_database);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No se pudo establecer conexión..."
                };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Conexión satisfactoria..."
            };
        }

        /// <summary>
        /// Obtiene las contabilidades del portal externo y marca las activas localmente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXConPortalesContabilidadData>> CntXConPortales_Contabilidades_Obtener(
            int codEmpresa, CntXConPortalesConexionRequest request)
        {
            var connectionString = _mCntXProfesional.FxPortalPrueba(
                request.por_user,
                request.por_password,
                request.por_server,
                request.por_database);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new ErrorDto<List<CntXConPortalesContabilidadData>>
                {
                    Code = -2,
                    Description = "Verifique la conexión del portal.",
                    Result = new List<CntXConPortalesContabilidadData>()
                };
            }

            var localConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var portalConnection = new SqlConnection(connectionString);
                using var localConnection = new SqlConnection(localConn);

                portalConnection.Open();
                localConnection.Open();

                var marcadas = request.cod_portal > 0
                    ? localConnection.Query<int>(
                        @"
                        select COD_CONTABILIDAD
                        from CNTX_CONSOLIDA_PORTALES_CONTAS
                        where cod_portal = @codPortal;",
                        new { codPortal = request.cod_portal }).ToHashSet()
                    : new HashSet<int>();

                var externas = portalConnection.Query<CntXConPortalesExternaData>(
                    @"
                    select
                        COD_CONTABILIDAD as cod_contabilidad,
                        isnull(nombre, '') as nombre
                    from CNTX_CONTABILIDADES
                    order by COD_CONTABILIDAD;").ToList();

                var result = externas
                    .Select(x => new CntXConPortalesContabilidadData
                    {
                        cod_contabilidad = x.cod_contabilidad,
                        nombre = x.nombre,
                        @checked = marcadas.Contains(x.cod_contabilidad)
                    })
                    .ToList();

                return new ErrorDto<List<CntXConPortalesContabilidadData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CntXConPortalesContabilidadData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new List<CntXConPortalesContabilidadData>()
                };
            }
        }

        /// <summary>
        /// Guarda un portal y reemplaza las contabilidades activas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXConPortales_Guardar(
            int codEmpresa, string usuario, CntXConPortalesGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "La descripción del portal no es válida."
                };
            }

            var connectionString = _mCntXProfesional.FxPortalPrueba(
                request.por_user,
                request.por_password,
                request.por_server,
                request.por_database);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "La conexión del portal no es válida."
                };
            }

            var localConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(localConn);

                var codPortal = request.cod_portal;

                if (codPortal > 0)
                {
                    connection.Execute(
                        @"
                        update CNTX_CONSOLIDA_PORTALES
                        set descripcion = @descripcion,
                            observacion = @observacion,
                            por_user = @porUser,
                            por_password = @porPassword,
                            por_server = @porServer,
                            por_database = @porDatabase,
                            modificado_usuario = @usuario,
                            modificado_fecha = Getdate()
                        where cod_portal = @codPortal;",
                        new
                        {
                            descripcion = request.descripcion.Trim().ToUpper(),
                            observacion = (request.observacion ?? string.Empty).Trim(),
                            porUser = (request.por_user ?? string.Empty).Trim(),
                            porPassword = MCntXProfesionalDb.FxPortalCifrado(request.por_password, "C"),
                            porServer = (request.por_server ?? string.Empty).Trim(),
                            porDatabase = (request.por_database ?? string.Empty).Trim(),
                            usuario = usuario.Trim(),
                            codPortal
                        });

                    RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Portal Codigo : {codPortal}");
                }
                else
                {
                    connection.Execute(
                        @"
                        insert into CNTX_CONSOLIDA_PORTALES
                        (
                            descripcion,
                            observacion,
                            por_user,
                            por_password,
                            por_server,
                            por_database,
                            creado_usuario,
                            creado_fecha
                        )
                        values
                        (
                            @codPortal,
                            @descripcion,
                            @observacion,
                            @porUser,
                            @porPassword,
                            @porServer,
                            @porDatabase,
                            @usuario,
                            getdate()
                        );",
                        new
                        {
                            descripcion = request.descripcion.Trim().ToUpper(),
                            observacion = (request.observacion ?? string.Empty).Trim(),
                            porUser = (request.por_user ?? string.Empty).Trim(),
                            porPassword = MCntXProfesionalDb.FxPortalCifrado(request.por_password, "C"),
                            porServer = (request.por_server ?? string.Empty).Trim(),
                            porDatabase = (request.por_database ?? string.Empty).Trim(),
                            usuario = usuario.Trim()
                        });

                    RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Portal Codigo : {codPortal}");

                    codPortal = connection.ExecuteScalar<int>(
                        "select isnull(max(cod_portal), 0) + 1 from CNTX_CONSOLIDA_PORTALES");
                }

                connection.Execute(
                    "delete CNTX_CONSOLIDA_PORTALES_CONTAS where cod_portal = @codPortal",
                    new { codPortal });

                foreach (var codContabilidad in request.contabilidades.Distinct().Where(x => x > 0))
                {
                    connection.Execute(
                        @"
                        insert into CNTX_CONSOLIDA_PORTALES_CONTAS
                        (
                            cod_portal,
                            COD_CONTABILIDAD,
                            registro_usuario,
                            registro_fecha
                        )
                        values
                        (
                            @codPortal,
                            @codContabilidad,
                            @usuario,
                            getdate()
                        );",
                        new
                        {
                            codPortal,
                            codContabilidad,
                            usuario = usuario.Trim()
                        });
                }


                return new ErrorDto
                {
                    Code = codPortal,
                    Description = "Información guardada satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Borra un portal y sus contabilidades relacionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPortal"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXConPortales_Borrar(
            int codEmpresa, int codPortal, string usuario)
        {
            if (codPortal <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar un portal válido."
                };
            }

            const string query = @"
                delete CNTX_CONSOLIDA_PORTALES_CONTAS
                where cod_portal = @codPortal;

                delete CNTX_CONSOLIDA_PORTALES
                where cod_portal = @codPortal;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new { codPortal });

            if (resp.Code == -1)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Elimina - WEB",
                $"Portal Codigo : {codPortal}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Registro eliminado satisfactoriamente."
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim().ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
