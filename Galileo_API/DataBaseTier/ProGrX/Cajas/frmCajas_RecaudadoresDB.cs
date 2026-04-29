using Dapper;
using System.Text;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasRecaudadoresDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCajasRecaudadoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Lista de recaudadores
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasRecaudadoresLista> Cajas_Recaudadores_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<CajasRecaudadoresLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasRecaudadoresLista()
                {
                    lista = new List<CajasRecaudadorData>()
                }
            };

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                var parametros = CrearParametrosListaRecaudadores(filtros, out string where);

                resp.Result.total = cn.QueryFirstOrDefault<int>(BuildRecaudadoresTotalQuery(where), parametros);
                resp.Result.lista = cn.Query<CajasRecaudadorData>(BuildRecaudadoresDataQuery(where, filtros), parametros).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = new List<CajasRecaudadorData>();
            }

            return resp;
        }

        private static DynamicParameters CrearParametrosListaRecaudadores(FiltrosLazyLoadData filtros, out string where)
        {
            var parametros = new DynamicParameters();
            var filtroTexto = (filtros.filtro ?? string.Empty).Trim();
            var whereBuilder = new StringBuilder(" WHERE 1 = 1 ");

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                whereBuilder.Append(" AND (R.cod_recaudador LIKE @like OR R.descripcion LIKE @like) ");
                parametros.Add("@like", $"%{filtroTexto}%");
            }

            AgregarPaginacionRecaudadores(filtros, parametros);
            where = whereBuilder.ToString();
            return parametros;
        }

        private static void AgregarPaginacionRecaudadores(FiltrosLazyLoadData filtros, DynamicParameters parametros)
        {
            if (filtros.pagina <= 0)
            {
                return;
            }

            var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
            var pageSize = filtros.paginacion != 0 ? filtros.paginacion : 10;

            parametros.Add("@Offset", offset);
            parametros.Add("@PageSize", pageSize);
        }

        private static string BuildPaginacionRecaudadores(FiltrosLazyLoadData filtros)
        {
            return filtros.pagina <= 0
                ? string.Empty
                : " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY ";
        }

        private static string BuildRecaudadoresTotalQuery(string where)
        {
            return $@"
                SELECT COUNT(*)
                FROM dbo.CAJAS_RECAUDADOR R
                {where};";
        }

        private static string BuildRecaudadoresDataQuery(string where, FiltrosLazyLoadData filtros)
        {
            return $@"
                SELECT
                    RTRIM(R.cod_recaudador)   AS cod_recaudador,
                    RTRIM(R.descripcion)      AS descripcion,
                    RTRIM(R.notas)            AS notas,
                    ISNULL(R.activo,1)        AS activo,
                    R.cod_cuenta,
                    R.cod_cuenta_iv,
                    R.cod_cuenta_comision,
                    R.registro_usuario,
                    R.registro_fecha
                FROM dbo.CAJAS_RECAUDADOR R
                {where}
                ORDER BY R.cod_recaudador
                {BuildPaginacionRecaudadores(filtros)};";
        }

        /// <summary>
        /// Navegación (scroll) entre recaudadores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Scroll(int CodEmpresa, int cod_contabilidad, int scroll, string? cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<CajasRecaudadorData>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string q = @"
            SELECT TOP 1 cod_recaudador
            FROM dbo.CAJAS_RECAUDADOR ";

                if (scroll > 0)
                {
                    q += @"WHERE (@cod_recaudador = '' OR cod_recaudador > @cod_recaudador)
                   ORDER BY cod_recaudador ASC;";
                }
                else
                {
                    q += @"WHERE (@cod_recaudador = '' OR cod_recaudador < @cod_recaudador)
                   ORDER BY cod_recaudador DESC;";
                }

                var siguiente = cn.QueryFirstOrDefault<string>(q, CrearParametroCodigoRecaudador(cod_recaudador));

                if (!string.IsNullOrEmpty(siguiente))
                {
                    return Cajas_Recaudadores_Obtener(CodEmpresa, cod_contabilidad, siguiente);
                }

                resp.Code = -2;
                resp.Description = "No hay más recaudadores en esa dirección.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene la información completa de un recaudador específico.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="cod_recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Obtener(int CodEmpresa, int cod_contabilidad, string cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<CajasRecaudadorData>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                const string q = @"
            SELECT
                R.cod_recaudador,
                RTRIM(ISNULL(R.descripcion, ''))          AS descripcion,
                RTRIM(ISNULL(R.notas, ''))                AS notas,
                ISNULL(R.activo, 1)                       AS activo,

                RTRIM(ISNULL(R.cod_cuenta, ''))           AS cod_cuenta,
                RTRIM(ISNULL(Cta.Descripcion, ''))        AS cod_cuenta_desc,

                RTRIM(ISNULL(R.cod_cuenta_iv, ''))        AS cod_cuenta_iv,
                RTRIM(ISNULL(CtaIv.Descripcion, ''))      AS cod_cuenta_iv_desc,

                RTRIM(ISNULL(R.cod_cuenta_comision, ''))  AS cod_cuenta_comision,
                RTRIM(ISNULL(CtaCom.Descripcion, ''))     AS cod_cuenta_comision_desc,

                ISNULL(R.registro_usuario, '')            AS registro_usuario,
                R.registro_fecha
            FROM dbo.CAJAS_RECAUDADOR R
            LEFT JOIN dbo.CntX_Cuentas Cta
                   ON R.cod_cuenta = Cta.Cod_Cuenta
                  AND Cta.Cod_Contabilidad = @conta
            LEFT JOIN dbo.CntX_Cuentas CtaIv
                   ON R.cod_cuenta_iv = CtaIv.Cod_Cuenta
                  AND CtaIv.Cod_Contabilidad = @conta
            LEFT JOIN dbo.CntX_Cuentas CtaCom
                   ON R.cod_cuenta_comision = CtaCom.Cod_Cuenta
                  AND CtaCom.Cod_Contabilidad = @conta
            WHERE R.cod_recaudador = @cod_recaudador;";

                var data = cn.QueryFirstOrDefault<CajasRecaudadorData>(
                    q,
                    CrearParametrosRecaudadorContabilidad(cod_recaudador, cod_contabilidad));

                if (data == null)
                {
                    resp.Code = -2;
                    resp.Description = "El recaudador indicado no existe.";
                }
                else
                {
                    resp.Result = data;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Verifica si el código de recaudador está libre u ocupador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Existe_Obtener(int CodEmpresa, string cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                const string q = @"
            SELECT ISNULL(COUNT(1), 0)
            FROM dbo.CAJAS_RECAUDADOR
            WHERE UPPER(cod_recaudador) = @cod;";

                int existe = cn.ExecuteScalar<int>(q, CrearParametroCodigoRecaudadorMayuscula(cod_recaudador));

                if (existe == 0)
                {
                    resp.Code = 0;
                    resp.Description = "Recaudador: Libre!";
                }
                else
                {
                    resp.Code = -2;
                    resp.Description = "Recaudador: Ocupado!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Guarda (insert/update) un recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Guardar(int CodEmpresa, string usuario, CajasRecaudadorData recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (string.IsNullOrWhiteSpace(recaudador.cod_recaudador))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el código de recaudador.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(recaudador.descripcion))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar la descripción del recaudador.";
                    return resp;
                }

                const string qExiste = @"
            SELECT ISNULL(COUNT(1), 0)
            FROM dbo.CAJAS_RECAUDADOR
            WHERE cod_recaudador = @cod_recaudador;";

                int existe = cn.QueryFirstOrDefault<int>(
                    qExiste,
                    CrearParametroCodigoRecaudador(recaudador.cod_recaudador));

                if (recaudador.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El recaudador {recaudador.cod_recaudador} ya existe.";
                        return resp;
                    }

                    resp = Cajas_Recaudadores_Insertar(CodEmpresa, usuario, recaudador);
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El recaudador {recaudador.cod_recaudador} no existe.";
                        return resp;
                    }

                    resp = Cajas_Recaudadores_Actualizar(CodEmpresa, usuario, recaudador);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Inserta un nuevo recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="recaudador"></param> 
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Recaudadores_Insertar(int CodEmpresa, string usuario, CajasRecaudadorData recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qInsert = @"
            INSERT INTO dbo.CAJAS_RECAUDADOR
                (cod_recaudador,
                 descripcion,
                 notas,
                 activo,
                 cod_cuenta,
                 cod_cuenta_iv,
                 cod_cuenta_comision,
                 registro_fecha,
                 registro_usuario)
            VALUES
                (@cod_recaudador,
                 @descripcion,
                 @notas,
                 @activo,
                 @cod_cuenta,
                 @cod_cuenta_iv,
                 @cod_cuenta_comision,
                 dbo.MyGetdate(),
                 @usuario);";

                cn.Execute(qInsert, CrearParametrosRecaudador(recaudador, usuario));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Registra - WEB",
                    $"Cajas_Recaudadores: Recaudador {recaudador.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza un recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="recaudador"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Recaudadores_Actualizar(int CodEmpresa, string usuario, CajasRecaudadorData recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qUpdate = @"
                UPDATE dbo.CAJAS_RECAUDADOR
                SET descripcion         = @descripcion,
                    notas               = @notas,
                    activo              = @activo,
                    cod_cuenta          = @cod_cuenta,
                    cod_cuenta_comision = @cod_cuenta_comision,
                    cod_cuenta_iv       = @cod_cuenta_iv
                WHERE cod_recaudador    = @cod_recaudador;";

                cn.Execute(qUpdate, CrearParametrosRecaudador(recaudador, usuario));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Modifica - WEB",
                    $"Cajas_Recaudadores: Recaudador {recaudador.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina un recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Eliminar(int CodEmpresa, string usuario, string cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                cn.Open();
                using var tran = cn.BeginTransaction();

                const string qDelete = @"
            DELETE FROM dbo.CAJAS_RECAUDADOR
            WHERE cod_recaudador = @cod_recaudador;";

                cn.Execute(qDelete, CrearParametroCodigoRecaudador(cod_recaudador), tran);

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Cajas_Recaudadores: Recaudador {cod_recaudador}");

                tran.Commit();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Lista todos los contactos de un recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasRecaudadorContactoData>> Cajas_Recaudadores_Contactos_Lista_Obtener(int CodEmpresa, string cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<List<CajasRecaudadorContactoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasRecaudadorContactoData>()
            };

            try
            {
                const string q = @"
                SELECT
                    cod_recaudador,
                    linea,
                    RTRIM(ISNULL(identificacion, '')) AS identificacion,
                    RTRIM(ISNULL(nombre, ''))         AS nombre,
                    RTRIM(ISNULL(tel_trabajo, ''))    AS tel_trabajo,
                    RTRIM(ISNULL(tel_celular, ''))    AS tel_celular,
                    RTRIM(ISNULL(email, ''))          AS email
                FROM dbo.CAJAS_RECAUDADOR_CONTACTOS
                WHERE cod_recaudador = @cod_recaudador
                ORDER BY linea;";

                resp.Result = cn.Query<CajasRecaudadorContactoData>(
                    q,
                    CrearParametroCodigoRecaudador(cod_recaudador)).AsList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Guarda (insert/update) un contacto del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="contacto"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Contactos_Guardar(int CodEmpresa, string usuario, CajasRecaudadorContactoData contacto)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(contacto.cod_recaudador))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el recaudador.";
                    return resp;
                }

                const string qExiste = @"
                SELECT ISNULL(COUNT(1), 0)
                FROM dbo.CAJAS_RECAUDADOR_CONTACTOS
                WHERE cod_recaudador = @cod_recaudador
                  AND linea          = @linea;";

                int existe = 0;
                if (contacto.linea > 0)
                {
                    existe = cn.ExecuteScalar<int>(qExiste, CrearParametrosContactoClave(contacto.cod_recaudador, contacto.linea));
                }

                if (contacto.isNew || contacto.linea == 0 || existe == 0)
                {
                    resp = Cajas_Recaudadores_Contactos_Insertar(CodEmpresa, usuario, contacto);
                }
                else
                {
                    resp = Cajas_Recaudadores_Contactos_Actualizar(CodEmpresa, usuario, contacto);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Inserta un nuevo contacto del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="contacto"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Recaudadores_Contactos_Insertar(int CodEmpresa, string usuario, CajasRecaudadorContactoData contacto)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                const string qNextLinea = @"
                SELECT ISNULL(MAX(linea), 0) + 1
                FROM dbo.CAJAS_RECAUDADOR_CONTACTOS
                WHERE cod_recaudador = @cod_recaudador;";

                contacto.linea = cn.ExecuteScalar<int>(qNextLinea, CrearParametroCodigoRecaudador(contacto.cod_recaudador));

                const string qInsert = @"
                INSERT INTO dbo.CAJAS_RECAUDADOR_CONTACTOS
                    (cod_recaudador, linea, identificacion, nombre,
                     tel_trabajo, tel_celular, email)
                VALUES
                    (@cod_recaudador, @linea, @identificacion, @nombre,
                     @tel_trabajo, @tel_celular, @email);";

                cn.Execute(qInsert, CrearParametrosContacto(contacto));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Registra - WEB",
                    $"Cajas_Recaudadores: Contacto {contacto.linea} recaudador {contacto.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza un contacto del recaudador existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="contacto"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Recaudadores_Contactos_Actualizar(int CodEmpresa, string usuario, CajasRecaudadorContactoData contacto)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                const string qUpdate = @"
            UPDATE dbo.CAJAS_RECAUDADOR_CONTACTOS
            SET identificacion = @identificacion,
                nombre         = @nombre,
                tel_trabajo    = @tel_trabajo,
                tel_celular    = @tel_celular,
                email          = @email
            WHERE cod_recaudador = @cod_recaudador
              AND linea          = @linea;";

                cn.Execute(qUpdate, CrearParametrosContacto(contacto));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Modifica - WEB",
                    $"Cajas_Recaudadores: Contacto {contacto.linea} recaudador {contacto.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina un contacto del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="linea"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Contactos_Eliminar(int CodEmpresa, string usuario, string cod_recaudador, int linea)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                const string qDelete = @"
            DELETE FROM dbo.CAJAS_RECAUDADOR_CONTACTOS
            WHERE cod_recaudador = @cod_recaudador
              AND linea          = @linea;";

                cn.Execute(qDelete, CrearParametrosContactoClave(cod_recaudador, linea));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Cajas_Recaudadores: Contacto {linea} recaudador {cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Lista los servicios del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasRecaudadorServicioItem>> Cajas_Recaudadores_Servicios_Lista_Obtener(int CodEmpresa, string cod_recaudador)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<List<CajasRecaudadorServicioItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasRecaudadorServicioItem>()
            };

            try
            {
                const string q = @"
                    SELECT
                        @cod_recaudador      AS cod_recaudador,
                        S.cod_servicio,
                        RTRIM(ISNULL(S.descripcion, '')) AS descripcion
                    FROM dbo.CAJAS_SERVICIOS S
                    WHERE S.cod_recaudador = @cod_recaudador
                    ORDER BY S.cod_servicio;";

                resp.Result = cn.Query<CajasRecaudadorServicioItem>(
                    q,
                    CrearParametroCodigoRecaudador(cod_recaudador)).AsList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Lista de cajas con indicador de si están vinculadas al servicio del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasServiciosCajasVinculadasData>> Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto<List<CajasServiciosCajasVinculadasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasServiciosCajasVinculadasData>()
            };

            try
            {
                const string q = @"
            SELECT
                @cod_servicio                                           AS concepto,
                C.COD_CAJA                                              AS cod_caja,
                RTRIM(ISNULL(C.DESCRIPCION, ''))                        AS desc_caja,
                CASE WHEN X.cod_caja IS NULL THEN CAST(0 AS smallint)
                     ELSE CAST(1 AS smallint) END                       AS asignada
            FROM dbo.CAJAS_DEFINICION C
            LEFT JOIN dbo.CAJAS_SERVICIOS_ASIGNADOS X
                   ON X.cod_caja       = C.COD_CAJA
                  AND X.cod_recaudador = @cod_recaudador
                  AND X.cod_servicio   = @cod_servicio
            ORDER BY C.DESCRIPCION;";

                resp.Result = cn.Query<CajasServiciosCajasVinculadasData>(
                    q,
                    CrearParametrosServicioAsignado(cod_recaudador, cod_servicio)).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Guarda la asignación de una caja a un servicio de un recaudador
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// <param name="cod_caja"></param>
        /// <param name="asignada">
        /// </param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, string cod_caja, short asignada)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qExiste = @"
            SELECT ISNULL(COUNT(1), 0)
            FROM dbo.CAJAS_SERVICIOS_ASIGNADOS
            WHERE cod_recaudador = @cod_recaudador
              AND cod_servicio   = @cod_servicio
              AND cod_caja       = @cod_caja;";

                int existe = cn.ExecuteScalar<int>(
                    qExiste,
                    CrearParametrosServicioCaja(cod_recaudador, cod_servicio, cod_caja));

                if (asignada == 1)
                {
                    if (existe == 0)
                    {
                        const string qInsert = @"
                    INSERT INTO dbo.CAJAS_SERVICIOS_ASIGNADOS
                        (cod_recaudador, cod_servicio, cod_caja, registro_fecha, registro_usuario)
                    VALUES
                        (@cod_recaudador, @cod_servicio, @cod_caja, dbo.MyGetdate(), @usuario);";

                        cn.Execute(
                            qInsert,
                            CrearParametrosServicioCajaConUsuario(cod_recaudador, cod_servicio, cod_caja, usuario));

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            "Registra - WEB",
                            $"Cajas_Servicios: Asigna caja {cod_caja} al servicio {cod_servicio} del recaudador {cod_recaudador}");
                    }
                    else
                    {
                        resp.Description = "La caja ya estaba asignada. No se realizaron cambios.";
                    }
                }
                else
                {
                    if (existe > 0)
                    {
                        const string qDelete = @"
                    DELETE FROM dbo.CAJAS_SERVICIOS_ASIGNADOS
                    WHERE cod_recaudador = @cod_recaudador
                      AND cod_servicio   = @cod_servicio
                      AND cod_caja       = @cod_caja;";

                        cn.Execute(
                            qDelete,
                            CrearParametrosServicioCaja(cod_recaudador, cod_servicio, cod_caja));

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            "Elimina - WEB",
                            $"Cajas_Servicios: Quita caja {cod_caja} del servicio {cod_servicio} del recaudador {cod_recaudador}");
                    }
                    else
                    {
                        resp.Description = "La caja no estaba asignada. No se realizaron cambios.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private static string TextoSeguro(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private static object CrearParametroCodigoRecaudador(string? codRecaudador)
        {
            return new { cod_recaudador = TextoSeguro(codRecaudador) };
        }

        private static object CrearParametroCodigoRecaudadorMayuscula(string? codRecaudador)
        {
            return new { cod = TextoSeguro(codRecaudador).ToUpperInvariant() };
        }

        private static object CrearParametrosRecaudadorContabilidad(string? codRecaudador, int codContabilidad)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                conta = codContabilidad
            };
        }

        private static DynamicParameters CrearParametrosRecaudador(CajasRecaudadorData recaudador, string? usuario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@cod_recaudador", TextoSeguro(recaudador.cod_recaudador));
            parametros.Add("@descripcion", TextoSeguro(recaudador.descripcion));
            parametros.Add("@notas", TextoSeguro(recaudador.notas));
            parametros.Add("@activo", recaudador.activo ? 1 : 0);
            parametros.Add("@cod_cuenta", TextoSeguro(recaudador.cod_cuenta));
            parametros.Add("@cod_cuenta_iv", TextoSeguro(recaudador.cod_cuenta_iv));
            parametros.Add("@cod_cuenta_comision", TextoSeguro(recaudador.cod_cuenta_comision));
            parametros.Add("@usuario", usuario ?? string.Empty);
            return parametros;
        }

        private static object CrearParametrosContactoClave(string? codRecaudador, int linea)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                linea
            };
        }

        private static DynamicParameters CrearParametrosContacto(CajasRecaudadorContactoData contacto)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@cod_recaudador", TextoSeguro(contacto.cod_recaudador));
            parametros.Add("@linea", contacto.linea);
            parametros.Add("@identificacion", TextoSeguro(contacto.identificacion));
            parametros.Add("@nombre", TextoSeguro(contacto.nombre));
            parametros.Add("@tel_trabajo", TextoSeguro(contacto.tel_trabajo));
            parametros.Add("@tel_celular", TextoSeguro(contacto.tel_celular));
            parametros.Add("@email", TextoSeguro(contacto.email));
            return parametros;
        }

        private static object CrearParametrosServicioAsignado(string? codRecaudador, string? codServicio)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio)
            };
        }

        private static object CrearParametrosServicioCaja(string? codRecaudador, string? codServicio, string? codCaja)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio),
                cod_caja = TextoSeguro(codCaja)
            };
        }

        private static object CrearParametrosServicioCajaConUsuario(string? codRecaudador, string? codServicio, string? codCaja, string? usuario)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio),
                cod_caja = TextoSeguro(codCaja),
                usuario = usuario ?? string.Empty
            };
        }

        private void RegistrarBitacora(int codEmpresa, string? usuario, string movimiento, string detalleMovimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                Modulo = vModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalleMovimiento
            });
        }
    }
}