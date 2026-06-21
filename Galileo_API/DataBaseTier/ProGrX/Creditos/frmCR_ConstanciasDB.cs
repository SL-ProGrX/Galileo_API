using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConstanciasDB
    {
        private readonly PortalDB _portalDB;
        private const string TODAS = "TODAS";
        public FrmCrConstanciasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla de constancias.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="nombre"></param>
        /// <param name="corte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrConstanciasInicialDto> CR_Constancias_Inicial_Obtener(
            int CodEmpresa,
            string cedula,
            string nombre,
            DateTime? corte,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoInicial(cedula, nombre, corte, usuario);
                var validacion = ValidarContextoInicial(ctx);

                if (validacion.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<CrConstanciasInicialDto>(validacion.Description ?? string.Empty);
                }

                var result = new CrConstanciasInicialDto
                {
                    cedula = ctx.Cedula,
                    nombre = ObtenerNombrePersona(conn, ctx),
                    corte = ctx.Corte,
                    emitido_por = ObtenerUsuarioDescripcion(conn, ctx.Usuario),
                    puesto = string.Empty,
                    cuentas_iban = ObtenerCuentasIban(conn, ctx.Cedula),
                    parentescos = ObtenerParentescos(conn),
                    ciclos = ObtenerCiclos()
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConstanciasInicialDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene lista de educación por tipo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConstanciasEducacionDto>> CR_Constancias_Educacion_List_Obtener(
            int CodEmpresa,
            string tipo,
            string? codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoEducacion(tipo, codigo);
                var validacion = ValidarContextoEducacion(ctx);

                if (validacion.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<List<CrConstanciasEducacionDto>>(validacion.Description ?? string.Empty);
                }

                var lista = conn.Query<CrConstanciasEducacionDto>(
                    "spSys_Educacion_List",
                    new
                    {
                        Tipo = ctx.Tipo,
                        Codigo = string.IsNullOrWhiteSpace(ctx.Codigo) ? null : ctx.Codigo
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrConstanciasEducacionDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el nombre de padrón nacional.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<CrConstanciasPadronDto> CR_Constancias_Padron_Nombre_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var id = (identificacion ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(id))
                {
                    return DbHelper.CreateErrorResponse<CrConstanciasPadronDto>("Debe indicar la identificación.");
                }

                var result = ObtenerPadronNombre(conn, id);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConstanciasPadronDto>(ex.Message);
            }
        }

        /// <summary>
        /// Registra la bitácora de emisión de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Constancias_Bitacora_Registra(
            int CodEmpresa,
            CrConstanciasBitacoraRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoBitacora(request);
                var validacion = ValidarContextoBitacora(ctx);

                if (validacion.Code != 0)
                {
                    return validacion;
                }

                conn.Execute(
                    "spSys_Bitacora_Operaciones_Registra",
                    new
                    {
                        Gestion = ctx.Gestion,
                        Cedula = ctx.Cedula,
                        Notas = ctx.Notas,
                        Usuario = ctx.Usuario
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene personas del padrón nacional para búsqueda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConstanciasPadronBusquedaDto>> CR_Constancias_Padron_Buscar(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = ObtenerPadronBusqueda(conn);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrConstanciasPadronBusquedaDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene personas del padrón nacional para búsqueda.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static List<CrConstanciasPadronBusquedaDto> ObtenerPadronBusqueda(SqlConnection conn)
        {
            const string sql = @"
        select
            rtrim(Identificacion) as identificacion,
            rtrim(Nombre) as nombre
        from vSys_Padron_Nacional
        order by Nombre;";

            return conn.Query<CrConstanciasPadronBusquedaDto>(sql).ToList();
        }

        /// <summary>
        /// Valida el contexto inicial.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarContextoInicial(CrConstanciasInicialContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (string.IsNullOrWhiteSpace(ctx.Usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida el contexto de educación.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarContextoEducacion(CrConstanciasEducacionContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Tipo))
            {
                return DbHelper.ErrorResponse("Debe indicar el tipo de educación.");
            }

            if (ctx.Tipo.Length > 1)
            {
                return DbHelper.ErrorResponse("El tipo de educación no es válido.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida el contexto de bitácora.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarContextoBitacora(CrConstanciasBitacoraContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Gestion))
            {
                return DbHelper.ErrorResponse("Debe indicar la gestión.");
            }

            if (string.IsNullOrWhiteSpace(ctx.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (string.IsNullOrWhiteSpace(ctx.Usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Obtiene la descripción del usuario emisor.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string ObtenerUsuarioDescripcion(SqlConnection conn, string usuario)
        {
            const string sql = @"
                select top 1 rtrim(isnull(descripcion, nombre)) as descripcion
                from Usuarios
                where Nombre = @usuario;";

            return conn.QueryFirstOrDefault<string>(sql, new
            {
                usuario
            })?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el nombre de la persona.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static string ObtenerNombrePersona(SqlConnection conn, CrConstanciasInicialContext ctx)
        {
            if (!string.IsNullOrWhiteSpace(ctx.Nombre))
            {
                return ctx.Nombre;
            }

            const string sql = @"
                select top 1 rtrim(nombre) as nombre
                from socios
                where cedula = @cedula;";

            return conn.QueryFirstOrDefault<string>(sql, new
            {
                cedula = ctx.Cedula
            })?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene las cuentas IBAN internas.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private static List<CrConstanciasCuentaIbanDto> ObtenerCuentasIban(SqlConnection conn, string cedula)
        {
            var raw = conn.Query<CrConstanciasCuentaIbanDbDto>(
                "spSys_Cuenta_SINPE",
                new
                {
                    Cedula = cedula
                },
                commandType: CommandType.StoredProcedure).ToList();

            var lista = raw.Select(MapCuentaIban).ToList();

            lista.Add(new CrConstanciasCuentaIbanDto
            {
                cedula = cedula,
                cuenta_cliente = string.Empty,
                iban = TODAS,
                iban_mask = TODAS,
                IdX = TODAS,
                ItmX = TODAS
            });

            return lista;
        }

        /// <summary>
        /// Obtiene los parentescos activos.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static List<CrConstanciasParentescoDto> ObtenerParentescos(SqlConnection conn)
        {
            const string sql = @"
                select
                    rtrim(cod_parentesco) as IdX,
                    rtrim(descripcion) as ItmX
                from sys_parentescos
                where activo = 1
                order by descripcion;";

            return conn.Query<CrConstanciasParentescoDto>(sql).ToList();
        }

        /// <summary>
        /// Obtiene el nombre del padrón nacional.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        private static CrConstanciasPadronDto ObtenerPadronNombre(SqlConnection conn, string identificacion)
        {
            const string sql = @"
                select top 1
                    rtrim(Identificacion) as identificacion,
                    rtrim(Nombre) as nombre
                from vSys_Padron_Nacional
                where Identificacion = @identificacion;";

            return conn.QueryFirstOrDefault<CrConstanciasPadronDto>(sql, new
            {
                identificacion
            }) ?? new CrConstanciasPadronDto
            {
                identificacion = identificacion,
                nombre = string.Empty
            };
        }

        /// <summary>
        /// Obtiene los ciclos fijos.
        /// </summary>
        /// <returns></returns>
        private static List<CrConstanciasCicloDto> ObtenerCiclos()
        {
            return new List<CrConstanciasCicloDto>
            {
                MapCiclo("I   Quatrimestre"),
                MapCiclo("II  Quatrimestre"),
                MapCiclo("III Quatrimestre"),
                MapCiclo("IV  Quatrimestre"),
                MapCiclo("I   Semestre"),
                MapCiclo("II  Semestre")
            };
        }

        /// <summary>
        /// Mapea cuenta IBAN.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static CrConstanciasCuentaIbanDto MapCuentaIban(CrConstanciasCuentaIbanDbDto data)
        {
            return new CrConstanciasCuentaIbanDto
            {
                cedula = data.CEDULA?.Trim() ?? string.Empty,
                cuenta_cliente = data.CUENTA_CLIENTE?.Trim() ?? string.Empty,
                iban = data.IBAN?.Trim() ?? string.Empty,
                iban_mask = data.IBAN_MASK?.Trim() ?? string.Empty,
                IdX = data.IBAN?.Trim() ?? string.Empty,
                ItmX = data.IBAN_MASK?.Trim() ?? string.Empty
            };
        }

        /// <summary>
        /// Mapea ciclo.
        /// </summary>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        private static CrConstanciasCicloDto MapCiclo(string descripcion)
        {
            return new CrConstanciasCicloDto
            {
                IdX = descripcion,
                ItmX = descripcion
            };
        }

        /// <summary>
        /// Crea el contexto inicial.
        /// </summary>
        /// <param name="cedula"></param>
        /// <param name="nombre"></param>
        /// <param name="corte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static CrConstanciasInicialContext CrearContextoInicial(
            string cedula,
            string nombre,
            DateTime? corte,
            string usuario)
        {
            return new CrConstanciasInicialContext
            {
                Cedula = (cedula ?? string.Empty).Trim(),
                Nombre = (nombre ?? string.Empty).Trim(),
                Corte = corte,
                Usuario = (usuario ?? string.Empty).Trim()
            };
        }

        /// <summary>
        /// Crea el contexto de educación.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private static CrConstanciasEducacionContext CrearContextoEducacion(string tipo, string? codigo)
        {
            return new CrConstanciasEducacionContext
            {
                Tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant(),
                Codigo = (codigo ?? string.Empty).Trim()
            };
        }

        /// <summary>
        /// Crea el contexto de bitácora.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static CrConstanciasBitacoraContext CrearContextoBitacora(CrConstanciasBitacoraRequest? request)
        {
            request ??= new CrConstanciasBitacoraRequest();

            return new CrConstanciasBitacoraContext
            {
                Gestion = (request.gestion ?? string.Empty).Trim(),
                Cedula = (request.cedula ?? string.Empty).Trim(),
                Notas = (request.notas ?? string.Empty).Trim(),
                Usuario = (request.usuario ?? string.Empty).Trim()
            };
        }

        public sealed class CrConstanciasInicialContext
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public DateTime? Corte { get; set; }
            public string Usuario { get; set; } = string.Empty;
        }

        public sealed class CrConstanciasEducacionContext
        {
            public string Tipo { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
        }

        public sealed class CrConstanciasBitacoraContext
        {
            public string Gestion { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }

        public sealed class CrConstanciasCuentaIbanDbDto
        {
            public string CEDULA { get; set; } = string.Empty;
            public string CUENTA_CLIENTE { get; set; } = string.Empty;
            public string IBAN { get; set; } = string.Empty;
            public string IBAN_MASK { get; set; } = string.Empty;
        }
    }
}