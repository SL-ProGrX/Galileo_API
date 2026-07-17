using Dapper;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPProveedoresDB
    {
        private readonly IConfiguration _config;
        private readonly MProGrXAuxiliarDB mAuxiliarDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly MCntLinkDB mCntLink;
        private readonly string sendEmail;
        private readonly string Notificaciones;



        private SqlConnection OpenConnection(int codEmpresa)
        {
            var cs = new PortalDB(_config!).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(cs);
        }
        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            try
            {
                using var conn = OpenConnection(codEmpresa);
                var result = action(conn);
                return new ErrorDto<T> { Code = 0, Description = "Ok", Result = result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default };
            }
        }

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPProveedoresDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPProveedoresDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mAuxiliarDB = new MProGrXAuxiliarDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            mCntLink = new MCntLinkDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value ?? string.Empty;
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value ?? string.Empty;
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        #endregion

        #region Consultas básicas

        /// <summary>
        /// Obtiene proveedores con filtros, búsqueda, ordenamiento y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <param name="parametros">Filtros de estado, autogestión y ferias.</param>
        /// <returns>Listado paginado de proveedores.</returns>
        public ErrorDto<TablasListaGenericaModel> Proveedores_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtro,
            CxPProveedorFiltros parametros)
        {
            const string codProveedorField = "COD_PROVEEDOR";

            string? search = filtro.filtro?.Trim();
            string sortField = string.IsNullOrWhiteSpace(filtro.sortField)
                ? codProveedorField
                : filtro.sortField;

            int sortOrder = filtro.sortOrder == 0 ? 1 : filtro.sortOrder;
            int pagina = filtro.pagina;
            int paginacion = filtro.paginacion;

            return WithConn(CodEmpresa, conn =>
            {
                var parameters = new DynamicParameters();

                parameters.Add("@Offset", pagina);
                parameters.Add("@PageSize", paginacion);
                parameters.Add("@Search", string.IsNullOrWhiteSpace(search) ? null : search);
                parameters.Add("@SearchPattern", string.IsNullOrWhiteSpace(search) ? null : $"%{search}%");
                parameters.Add("@Estado", parametros.estado);
                parameters.Add("@AutoGestion", parametros.autoGestion);
                parameters.Add("@Ventas", parametros.ventas);

                const string defaultSortField = codProveedorField;

                var sortMap = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    [codProveedorField] = codProveedorField,
                    ["DESCRIPCION"] = "DESCRIPCION"
                };

                if (!sortMap.TryGetValue(sortField, out string? safeSortField))
                    safeSortField = defaultSortField;

                string safeSortDir = sortOrder == -1 ? "DESC" : "ASC";
                parameters.Add("@SortField", safeSortField);
                parameters.Add("@SortDir", safeSortDir);

                string qTotal = @"
            SELECT COUNT(*)
            FROM CXP_PROVEEDORES
                        WHERE (@Estado = 'T' OR ESTADO = @Estado)
                            AND (
                                        (@AutoGestion = 0 AND @Ventas = 0)
                                        OR (@AutoGestion = 1 AND WEB_AUTO_GESTION = 1)
                                        OR (@Ventas = 1 AND WEB_FERIAS = 1)
                                    )
                            AND (
                                        @Search IS NULL
                                        OR CONVERT(VARCHAR(50), COD_PROVEEDOR) LIKE @SearchPattern
                                        OR DESCRIPCION LIKE @SearchPattern
                                    );
        ";

                int total = conn.QuerySingle<int>(qTotal, parameters);

                string sql = @"
            SELECT
                COD_PROVEEDOR,
                RTRIM(DESCRIPCION) AS DESCRIPCION,
                CEDJUR
            FROM CXP_PROVEEDORES
                        WHERE (@Estado = 'T' OR ESTADO = @Estado)
                            AND (
                                        (@AutoGestion = 0 AND @Ventas = 0)
                                        OR (@AutoGestion = 1 AND WEB_AUTO_GESTION = 1)
                                        OR (@Ventas = 1 AND WEB_FERIAS = 1)
                                    )
                            AND (
                                        @Search IS NULL
                                        OR CONVERT(VARCHAR(50), COD_PROVEEDOR) LIKE @SearchPattern
                                        OR DESCRIPCION LIKE @SearchPattern
                                    )
            ORDER BY
                CASE WHEN @SortField = 'COD_PROVEEDOR' AND @SortDir = 'ASC' THEN COD_PROVEEDOR END ASC,
                CASE WHEN @SortField = 'COD_PROVEEDOR' AND @SortDir = 'DESC' THEN COD_PROVEEDOR END DESC,
                CASE WHEN @SortField = 'DESCRIPCION' AND @SortDir = 'ASC' THEN DESCRIPCION END ASC,
                CASE WHEN @SortField = 'DESCRIPCION' AND @SortDir = 'DESC' THEN DESCRIPCION END DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";

                var lista = conn
                    .Query<ProveedorDto>(sql, parameters)
                    .ToList();

                return new TablasListaGenericaModel
                {
                    total = total,
                    lista = lista
                };
            });
        }


        /// <summary>
        /// Obtiene el detalle completo de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Detalle del proveedor.</returns>
        public ErrorDto<ProveedorDto> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<ProveedorDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.*, RTRIM(C.descripcion) AS TipoProv,
                         ISNULL(Cta.Descripcion, '') AS CuentaConta,
                         dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) AS Banco_Desc
                  FROM cxp_proveedores P
                  INNER JOIN cxp_prov_clas C ON P.cod_clasificacion = C.cod_clasificacion
                  LEFT JOIN CntX_Cuentas Cta ON P.cod_Cuenta = Cta.cod_Cuenta AND Cta.cod_contabilidad = 1
                  WHERE P.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorDto>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle del proveedor.",
                    Result = null
                };
            }

            if (result.Result is not null)
            {
                result.Result.Cod_Cuenta_Mask = mCntLink.fxgCntCuentaFormato(CodEmpresa, true, result.Result.Cod_Cuenta);
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorDto>
                {
                    Code = -2,
                    Description = "No se encontró el proveedor.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene los tipos de proveedor disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de proveedor.</returns>
        public ErrorDto<List<TipoProveedor>> TiposProveedor_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<TipoProveedor>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_clasificacion, descripcion FROM cxp_prov_clas ORDER BY cod_clasificacion");
        }

        /// <summary>
        /// Obtiene las cuentas autorizadas para desembolso.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cuentas de desembolso.</returns>
        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<CuentaDesembolso>("[spCxP_Bancos_Autorizados]", commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CuentaDesembolso>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener cuentas de desembolso.", result.Code.GetValueOrDefault(-1), new List<CuentaDesembolso>());
        }

        /// <summary>
        /// Obtiene las cuentas bancarias registradas para una identificación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Identificacion">Identificación del proveedor.</param>
        /// <returns>Listado de cuentas bancarias.</returns>
        public ErrorDto<List<Cuenta>> Cuentas_Obtener(int CodEmpresa, string? Identificacion)
        {
            return DbHelper.ExecuteListQuery<Cuenta>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(B.Descripcion) AS Banco,
                         CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS Tipo_Desc,
                         C.cod_Divisa,
                         C.CUENTA_INTERNA,
                         C.CUENTA_INTERBANCA,
                         C.ACTIVA,
                         C.DESTINO,
                         C.REGISTRO_FECHA,
                         C.REGISTRO_USUARIO
                  FROM SYS_CUENTAS_BANCARIAS C
                  INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                  WHERE C.Identificacion = @Identificacion",
                new { Identificacion });
        }

        /// <summary>
        /// Obtiene la siguiente secuencia disponible para proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Siguiente secuencia disponible.</returns>
        public int ObtenerSequencia(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT ISNULL(MAX(CAST(cod_proveedor AS int)), 0) + 1 FROM cxp_proveedores",
                0);

            return result.Code == 0 ? result.Result : 0;
        }

        /// <summary>
        /// Obtiene el proveedor anterior o siguiente según el filtro y la dirección indicada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código actual del proveedor.</param>
        /// <param name="tipo">Dirección del desplazamiento: asc o desc.</param>
        /// <param name="filtro">Filtros del formulario.</param>
        /// <returns>Código del proveedor encontrado.</returns>
        public int ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string tipo, ProveedorDataFiltros filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var autoGestion = filtro?.autoGestion == true;
                var ventas = filtro?.ventas == true;
                var usarDesc = tipo == "desc";

                var query = @"SELECT TOP 1 cod_proveedor
                      FROM cxp_proveedores
                      WHERE (
                            (@UsarFiltro = 0 AND ESTADO = 'A')
                            OR
                            (@UsarFiltro = 1 AND ESTADO IN ('A','T','S','I'))
                          )
                        AND (
                            @UsarFiltro = 0
                            OR (@AutoGestion = 0 AND @Ventas = 0)
                            OR (@AutoGestion = 1 AND @Ventas = 0 AND WEB_AUTO_GESTION = 1)
                            OR (@AutoGestion = 0 AND @Ventas = 1 AND WEB_FERIAS = 1)
                            OR (@AutoGestion = 1 AND @Ventas = 1 AND (WEB_AUTO_GESTION = 1 OR WEB_FERIAS = 1))
                        )
                        AND (
                            (@UsarDesc = 1 AND (@Cod_Proveedor <= 0 OR cod_proveedor < @Cod_Proveedor))
                            OR
                            (@UsarDesc = 0 AND cod_proveedor > @Cod_Proveedor)
                        )
                      ORDER BY
                        CASE WHEN @UsarDesc = 1 THEN cod_proveedor END DESC,
                        CASE WHEN @UsarDesc = 0 THEN cod_proveedor END ASC";

                var encontrado = connection.QueryFirstOrDefault<int>(
                    query,
                    new
                    {
                        UsarFiltro = filtro is not null,
                        AutoGestion = autoGestion,
                        Ventas = ventas,
                        UsarDesc = usarDesc,
                        Cod_Proveedor
                    });

                return encontrado == 0 || encontrado == Cod_Proveedor ? Cod_Proveedor : encontrado;
            });

            return result.Code == 0 ? result.Result : Cod_Proveedor;
        }

        /// <summary>
        /// Valida si existe otra cédula jurídica igual para un proveedor distinto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor actual.</param>
        /// <param name="cedula">Cédula jurídica a validar.</param>
        /// <returns>Cantidad de coincidencias encontradas.</returns>
        public int ValidaCedJuridica(int CodEmpresa, int Cod_Proveedor, string cedula)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT ISNULL(COUNT(*),0) FROM cxp_proveedores WHERE cod_proveedor <> @Cod_Proveedor AND cedJur = @cedula",
                0,
                new { Cod_Proveedor, cedula });

            return result.Code == 0 ? result.Result : 0;
        }

        /// <summary>
        /// Obtiene la divisa asociada a una cuenta contable.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cuenta">Cuenta contable.</param>
        /// <returns>Divisa de la cuenta.</returns>
        public ErrorDto<CuentaDivisa> ObtenerDivisaCuenta(int CodEmpresa, string Cuenta)
        {
            var result = DbHelper.ExecuteSingleQuery<CuentaDivisa>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_divisa FROM Cntx_Cuentas WHERE cod_contabilidad = 1 AND cod_cuenta = @Cuenta",
                null,
                new { Cuenta });

            if (result.Code != 0)
            {
                return new ErrorDto<CuentaDivisa>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener la divisa de la cuenta.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<CuentaDivisa>
                {
                    Code = -2,
                    Description = "No se encontró la cuenta.",
                    Result = null
                };
        }

        #endregion

        #region CRUD proveedor

        /// <summary>
        /// Actualiza la información de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del proveedor a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Proveedor_Actualizar(int CodEmpresa, ProveedorDto request)
        {
            var gestion = request.Web_Auto_Gestion ? 1 : 0;
            var ferias = request.Web_Ferias ? 1 : 0;

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var estadoAnterior = connection.QueryFirstOrDefault<string>(
                    "SELECT ESTADO FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @Cod_Proveedor",
                    new { request.Cod_Proveedor }) ?? string.Empty;

                connection.Execute(
                    @"UPDATE cxp_proveedores SET
                            descripcion = @Descripcion,
                            cod_alter = @Cod_Alter,
                            cedJur = @Cedjur,
                            tipo = @Tipo,
                            observacion = @Observacion,
                            estado = @Estado,
                            direccion = @Direccion,
                            aptopostal = @Aptopostal,
                            email = @Email,
                            telefono = @Telefono,
                            email_02 = @Email_02,
                            fax = @Fax,
                            contacto_compras = @Contacto_Compras,
                            contacto_ventas = @Contacto_Ventas,
                            cod_cuenta = @Cod_Cuenta,
                            descuento_porc = @Descuento_Porc,
                            credito_plazo = @Credito_Plazo,
                            credito_monto = @Credito_Monto,
                            cod_clasificacion = @Cod_Clasificacion,
                            nit_Codigo = @Nit_Codigo,
                            nit_nombre = @Nit_Nombre,
                            cod_divisa = @Cod_Divisa,
                            cod_Banco = @Cod_Banco,
                            web_auto_gestion = @Gestion,
                            web_ferias = @Ferias,
                            registro_fecha = @Registro_Fecha,
                            fecha_vencimiento = @Fecha_Vencimiento,
                            representante_legal = @Representante_Legal,
                            convenio = @Convenio,
                            plazo = @Plazo,
                            criticidad = @Criticidad
                      WHERE cod_proveedor = @Cod_Proveedor",
                    new
                    {
                        request.Descripcion,
                        request.Cod_Alter,
                        Cedjur = request.Cedjur,
                        request.Tipo,
                        request.Observacion,
                        request.Estado,
                        request.Direccion,
                        request.Aptopostal,
                        request.Email,
                        request.Telefono,
                        request.Email_02,
                        request.Fax,
                        request.Contacto_Compras,
                        request.Contacto_Ventas,
                        request.Cod_Cuenta,
                        request.Descuento_Porc,
                        request.Credito_Plazo,
                        request.Credito_Monto,
                        request.Cod_Clasificacion,
                        request.Nit_Codigo,
                        request.Nit_Nombre,
                        request.Cod_Divisa,
                        request.Cod_Banco,
                        Gestion = gestion,
                        Ferias = ferias,
                        Registro_Fecha = request.registro_fecha,
                        Fecha_Vencimiento = request.fecha_vencimiento,
                        Representante_Legal = request.representante_legal,
                        Convenio = request.convenio,
                        Plazo = request.plazo,
                        Criticidad = request.criticidad,
                        request.Cod_Proveedor
                    });

                return estadoAnterior;
            });

            if (result.Code == 0 && result.Result != request.Estado)
            {
                mAuxiliarDB.BitacoraProveedor(new BitacoraProveedorInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    cod_proveedor = request.Cod_Proveedor.ToString(),
                    consec = 0,
                    movimiento = "Inserta",
                    detalle = request.justificacion_estado ?? string.Empty,
                    registro_usuario = request.user_modifica ?? string.Empty
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del proveedor a insertar.</param>
        /// <returns>Resultado de la operación con el código generado.</returns>
        public ErrorDto Proveedor_Insertar(int CodEmpresa, ProveedorDto request)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var siguiente = connection.QueryFirstOrDefault<int>(
                    "SELECT ISNULL(MAX(CAST(cod_proveedor AS int)),0) + 1 FROM cxp_proveedores");

                connection.Execute(
                    @"INSERT INTO cxp_proveedores(
                            cod_proveedor, tipo, cod_clasificacion, descripcion, cod_alter, observacion,
                            estado, contacto_ventas, contacto_compras, telefono, telefono_ext, fax, fax_ext,
                            email, email_02, aptopostal, direccion, credito_plazo, credito_monto,
                            descuento_porc, saldo, cod_cuenta, cedJur, Nit_Codigo, Nit_Nombre,
                            cod_divisa, saldo_divisa_real, cod_banco, fecha_vencimiento, registro_fecha,
                            plazo, convenio, representante_legal, criticidad)
                      VALUES(
                            @Cod_Proveedor, @Tipo, @Cod_Clasificacion, @Descripcion, @Cod_Alter, @Observacion,
                            @Estado, @Contacto_Ventas, @Contacto_Compras, @Telefono, @Telefono_Ext, @Fax, @Fax_Ext,
                            @Email, @Email_02, @Aptopostal, @Direccion, @Credito_Plazo, @Credito_Monto,
                            @Descuento_Porc, @Saldo, @Cod_Cuenta, @Cedjur, @Nit_Codigo, @Nit_Nombre,
                            @Cod_Divisa, @Saldo_Divisa_Real, @Cod_Banco, @Fecha_Vencimiento, @Registro_Fecha,
                            @Plazo, @Convenio, @Representante_Legal, @Criticidad)",
                    new
                    {
                        Cod_Proveedor = siguiente,
                        request.Tipo,
                        request.Cod_Clasificacion,
                        request.Descripcion,
                        request.Cod_Alter,
                        request.Observacion,
                        request.Estado,
                        request.Contacto_Ventas,
                        request.Contacto_Compras,
                        request.Telefono,
                        request.Telefono_Ext,
                        request.Fax,
                        request.Fax_Ext,
                        request.Email,
                        request.Email_02,
                        request.Aptopostal,
                        request.Direccion,
                        request.Credito_Plazo,
                        request.Credito_Monto,
                        request.Descuento_Porc,
                        request.Saldo,
                        request.Cod_Cuenta,
                        Cedjur = request.Cedjur,
                        request.Nit_Codigo,
                        request.Nit_Nombre,
                        request.Cod_Divisa,
                        request.Saldo_Divisa_Real,
                        request.Cod_Banco,
                        Fecha_Vencimiento = request.fecha_vencimiento,
                        Registro_Fecha = request.registro_fecha,
                        Plazo = request.plazo,
                        Convenio = request.convenio,
                        Representante_Legal = request.representante_legal,
                        Criticidad = request.criticidad
                    });

                return siguiente;
            });

            return result.Code == 0
                ? new ErrorDto { Code = result.Result, Description = "Ok" }
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Proveedor_Borrar(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE cxp_proveedores WHERE cod_proveedor = @Cod_Proveedor",
                new { Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar proveedor.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Autorizaciones

        /// <summary>
        /// Obtiene las autorizaciones asociadas a un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de autorizaciones.</returns>
        public ErrorDto<List<Autorizacion>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<Autorizacion>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT (cedula + ' - ' + CONVERT(VARCHAR(10), cod_proveedor)) AS dataKey,
                         cod_proveedor,
                         cedula,
                         nombre
                  FROM cxp_autorizaciones
                  WHERE cod_proveedor = @Cod_Proveedor
                  ORDER BY cedula",
                new { Cod_Proveedor });
        }

        /// <summary>
        /// Actualiza una autorización de proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la autorización.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizacion_Actualizar(int CodEmpresa, Autorizacion request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_autorizaciones
                  SET nombre = @Nombre
                  WHERE cod_proveedor = @Cod_Proveedor
                    AND cedula = @Cedula",
                new
                {
                    request.Nombre,
                    request.Cod_Proveedor,
                    request.Cedula
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar autorización.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una nueva autorización para proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la autorización.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizacion_Insertar(int CodEmpresa, Autorizacion request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT INTO cxp_autorizaciones(cod_proveedor, cedula, nombre)
                  VALUES(@Cod_Proveedor, @Cedula, @Nombre)",
                new
                {
                    request.Cod_Proveedor,
                    request.Cedula,
                    request.Nombre
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Autorización agregada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar autorización.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una autorización de proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la autorización.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizacion_Borrar(int CodEmpresa, Autorizacion request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE cxp_autorizaciones WHERE cod_proveedor = @Cod_Proveedor AND cedula = @Cedula",
                new
                {
                    request.Cod_Proveedor,
                    request.Cedula
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Autorización eliminada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar autorización.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Suspensiones

        /// <summary>
        /// Obtiene los tipos de suspensión disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de suspensión.</returns>
        public ErrorDto<List<TipoSuspension>> TipoSuspension_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<TipoSuspension>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_suspension, descripcion FROM CXP_SUSPENSION_TIPOS");
        }

        /// <summary>
        /// Obtiene el listado paginado de suspensiones de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por notas, suspensión o usuario.</param>
        /// <returns>Listado paginado de suspensiones.</returns>
        public ErrorDto<SuspensionLista> Suspensiones_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new SuspensionLista
                {
                    Total = 0,
                    Suspensiones = new List<Suspension>()
                };

                var parametros = new DynamicParameters();
                parametros.Add("Cod_Proveedor", Cod_Proveedor);

                var totalBuilder = new StringBuilder("SELECT COUNT(*) FROM vCxP_Suspensiones WHERE cod_proveedor = @Cod_Proveedor");
                var detalleBuilder = new StringBuilder("SELECT * FROM vCxP_Suspensiones WHERE cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" AND (NOTAS LIKE @Filtro OR suspension_Desc LIKE @Filtro OR registro_Usuario LIKE @Filtro)");
                    detalleBuilder.Append(" AND (NOTAS LIKE @Filtro OR suspension_Desc LIKE @Filtro OR registro_Usuario LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" ORDER BY VENCIMIENTO");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Suspensiones = connection.Query<Suspension>(detalleBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new SuspensionLista { Total = 0, Suspensiones = new List<Suspension>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener suspensiones.", result.Code.GetValueOrDefault(-1), new SuspensionLista { Total = 0, Suspensiones = new List<Suspension>() });
        }

        /// <summary>
        /// Inserta o actualiza una suspensión de proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la suspensión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Suspencion_InsertUpdate(int CodEmpresa, Suspension request)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var values = new
                {
                    ProveedorId = request.Cod_Proveedor,
                    Codigo = request.Cod_Suspension,
                    Activa = request.Activa,
                    Notas = request.Activa == 1 ? request.Notas : request.Reactiva_Notas,
                    Vencimiento = request.Activa == 1 ? request.Vencimiento : null,
                    Usuario = request.Activa == 1 ? request.Registro_Usuario : request.Reactiva_Usuario,
                };

                return connection.Query<int>("[spCxP_Suspension]", values, commandType: CommandType.StoredProcedure).FirstOrDefault();
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Suspensión procesada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar suspensión.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Fusiones

        /// <summary>
        /// Obtiene el detalle del proveedor principal asociado a una fusión.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor fusionado.</param>
        /// <returns>Detalle de la fusión del proveedor.</returns>
        public ErrorDto<ProveedorFusion> ProveedorFusion_ObtenerDetalle(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<ProveedorFusion>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.cod_proveedor, P.descripcion
                  FROM cxp_fusiones F
                  INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                  INNER JOIN cxp_proveedores X ON F.cod_proveedor_fus = X.cod_proveedor
                  WHERE F.cod_proveedor_fus = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorFusion>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle de fusión del proveedor.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorFusion>
                {
                    Code = -2,
                    Description = "No se encontró información de fusión.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene la lista paginada de proveedores fusionados asociados a un proveedor principal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor principal.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por código, descripción o fecha de fusión.</param>
        /// <returns>Listado paginado de fusiones.</returns>
        public ErrorDto<ProveedorFusionLista> ProveedorFusion_ObtenerLista(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new ProveedorFusionLista
                {
                    Total = 0,
                    Fusiones = new List<ProveedorFusion>()
                };

                var parametros = new DynamicParameters();
                parametros.Add("Cod_Proveedor", Cod_Proveedor);

                var totalBuilder = new StringBuilder(@"SELECT COUNT(*)
                                                      FROM cxp_fusiones F
                                                      INNER JOIN cxp_proveedores X ON F.cod_proveedor_fus = X.cod_proveedor
                                                      WHERE F.cod_proveedor = @Cod_Proveedor");

                var detalleBuilder = new StringBuilder(@"SELECT X.cod_proveedor, X.descripcion, X.fusion
                                                        FROM cxp_fusiones F
                                                        INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                                                        INNER JOIN cxp_proveedores X ON F.cod_proveedor_fus = X.cod_proveedor
                                                        WHERE F.cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" AND (CAST(X.cod_proveedor AS varchar(50)) LIKE @Filtro OR X.descripcion LIKE @Filtro OR CONVERT(varchar(25), X.fusion, 120) LIKE @Filtro)");
                    detalleBuilder.Append(" AND (CAST(X.cod_proveedor AS varchar(50)) LIKE @Filtro OR X.descripcion LIKE @Filtro OR CONVERT(varchar(25), X.fusion, 120) LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" ORDER BY F.cod_proveedor");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Fusiones = connection.Query<ProveedorFusion>(detalleBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new ProveedorFusionLista { Total = 0, Fusiones = new List<ProveedorFusion>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener fusiones del proveedor.", result.Code.GetValueOrDefault(-1), new ProveedorFusionLista { Total = 0, Fusiones = new List<ProveedorFusion>() });
        }

        #endregion

        #region Usuarios y eventos

        /// <summary>
        /// Obtiene la lista de usuarios asociados a un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de usuarios del proveedor.</returns>
        public ErrorDto<List<ProveedorUsuariosListaDatos>> ProveedorUsuariosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<ProveedorUsuariosListaDatos>(
                    "[spCxP_Proveedores_Usuarios_List]",
                    new { Proveedor = Cod_Proveedor },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<ProveedorUsuariosListaDatos>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener usuarios del proveedor.", result.Code.GetValueOrDefault(-1), new List<ProveedorUsuariosListaDatos>());
        }

        /// <summary>
        /// Obtiene la lista de eventos asociados a un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de eventos del proveedor.</returns>
        public ErrorDto<List<ProveedorEventosListaDatos>> ProveedorEventosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<ProveedorEventosListaDatos>(
                    "[spCxP_Proveedores_Eventos_List]",
                    new { Proveedor = Cod_Proveedor },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<ProveedorEventosListaDatos>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener eventos del proveedor.", result.Code.GetValueOrDefault(-1), new List<ProveedorEventosListaDatos>());
        }

        /// <summary>
        /// Asigna o desasigna un proveedor a un evento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Evento">Código del evento.</param>
        /// <param name="Activa">Indicador de activación.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ProveedorEventos_Asigna(int CodEmpresa, int Cod_Proveedor, int Evento, bool Activa, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<int>(
                    "[spCxP_Proveedores_Eventos_Asigna]",
                    new
                    {
                        Proveedor = Cod_Proveedor,
                        Evento,
                        Activa,
                        Usuario = usuario
                    },
                    commandType: CommandType.StoredProcedure).FirstOrDefault());

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al asignar evento al proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Agrega un usuario asociado a un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="datos">Datos del usuario a agregar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CxPProveedoresUsuario_Agregar(int CodEmpresa, ProveedorUsuariosListaDatos datos)
        {
            int portal = datos.web_auto_gestion ? 1 : 0;
            int ferias = datos.web_ferias ? 1 : 0;
            int activo = datos.activo ? 1 : 0;

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<int>(
                    "[spCxP_Proveedores_Usuario_Add]",
                    new
                    {
                        Proveedor = datos.cod_proveedor,
                        Usuario = datos.usuario,
                        Nombre = datos.nombre,
                        Email = datos.email,
                        Portal = portal,
                        Ferias = ferias,
                        Activo = activo,
                        Registro_Usuario = datos.registro_usuario
                    },
                    commandType: CommandType.StoredProcedure).FirstOrDefault());

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al agregar usuario del proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Renueva la clave de AutoGestión del usuario de un proveedor.
        /// </summary>
        public ErrorDto ProveedorUsuario_RenovarClaveWeb(int CodEmpresa, int CodProveedor, string usuario, string email, string usuarioSesion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            const string sp = @"
                exec spuProGrX_MOBILE_Proveedor_WebKey_Renueva
                    @Proveedor,
                    @Usuario,
                    @Email,
                    @RegistroUsuario,
                    @Token";

            try
            {
                connection.Execute(sp, new
                {
                    Proveedor = CodProveedor,
                    Usuario = usuario.Trim(),
                    Email = email?.Trim() ?? string.Empty,
                    RegistroUsuario = usuarioSesion,
                    Token = string.Empty
                });

                return DbHelper.OkResponse("Clave de AutoGestion Renovada satisfactoriamente (Enviada por E-mail)");
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al renovar la clave de AutoGestion. Por favor, intente nuevamente o contacte al soporte.");
            }
        }

        #endregion

        #region Bitácora y notificaciones

        /// <summary>
        /// Obtiene la bitácora del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_proveedor">Código del proveedor.</param>
        /// <returns>Listado de movimientos en bitácora.</returns>
        public ErrorDto<List<BitacoraProveedorDto>> BitacoraProducto_Obtener(int CodEmpresa, int cod_proveedor)
        {
            return DbHelper.ExecuteListQuery<BitacoraProveedorDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ID_BITACORA,
                         CONSEC,
                         REGISTRO_FECHA,
                         COD_PROVEEDOR,
                         REGISTRO_USUARIO,
                         DETALLE,
                         MOVIMIENTO
                  FROM BITACORA_PROVEEDOR
                  WHERE cod_proveedor = @cod_proveedor
                  ORDER BY 1 ASC",
                new { cod_proveedor });
        }

        /// <summary>
        /// Obtiene los proveedores próximos a vencer y envía la notificación correspondiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de proveedores notificados.</returns>
        public async Task<ErrorDto<List<ProveedorDto>>> Proveedor_NotificacionVencimiento(int CodEmpresa)
        {
            var diasResult = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT valor FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'VCXP'",
                0);

            if (diasResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(diasResult.Description ?? "Error al obtener los días de notificación.", diasResult.Code.GetValueOrDefault(-1), new List<ProveedorDto>());
            }

            var dias = diasResult.Result;
            var proveedoresResult = DbHelper.ExecuteListQuery<ProveedorDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT *
                  FROM cxp_proveedores
                  WHERE fecha_vencimiento BETWEEN GETDATE() AND DATEADD(DAY, @dias, GETDATE())",
                new { dias });

            if (proveedoresResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(proveedoresResult.Description ?? "Error al obtener proveedores por vencer.", proveedoresResult.Code.GetValueOrDefault(-1), new List<ProveedorDto>());
            }

            var proveedores = proveedoresResult.Result ?? new List<ProveedorDto>();
            await CorreoNotificacionVencimiento_Enviar(CodEmpresa, proveedores, dias);
            return DbHelper.CreateOkResponse(proveedores);
        }

        /// <summary>
        /// Envía el correo de notificación por vencimiento de registro a los proveedores indicados.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="proveedores">Listado de proveedores a notificar.</param>
        /// <param name="dias">Cantidad de días previos al vencimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public async Task<ErrorDto> CorreoNotificacionVencimiento_Enviar(int CodCliente, List<ProveedorDto> proveedores, int dias)
        {
            var info = new ErrorDto { Code = 0 };
            var correoConfigResponse = _envioCorreoDB.CorreoConfig(CodCliente, Notificaciones);
            var eConfig = correoConfigResponse?.Result ?? new EnvioCorreoModels();

            foreach (var proveedor in proveedores)
            {
                try
                {
                    string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Solicitud de Cotización</title>
                                <style>
                                    body {{ font-family: Arial, sans-serif; }}
                                    .container {{ width: 600px; margin: 0 auto; border: 1px solid #eaeaea; padding: 20px; }}
                                    .header {{ background-color: #e8f3ff; padding: 10px; }}
                                    .header img {{ width: auto; height: 50px; }}
                                    .content {{ margin-top: 20px; }}
                                    .content h2 {{ font-size: 16px; color: #0072ce; }}
                                    .table {{ width: 100%; margin-top: 20px; border-collapse: collapse; }}
                                    .table th, .table td {{ padding: 10px; border: 1px solid #dcdcdc; text-align: left; }}
                                    .table th {{ background-color: #0072ce; color: white; }}
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                    </div>
                                    <div class=""content"">
                                        <h2><strong>Notificación de vencimiento de registro</strong></h2>
                                        <p>Estimado Proveedor <strong>{proveedor.Descripcion}</strong></p>
                                        <p>Mediante la presente se le comunica el vencimiento de su registro en {dias} día(s).</p>
                                    </div>
                                </div>
                            </body>
                        </html>";

                    var emailDestino = proveedor.Email ?? string.Empty;
                    if (sendEmail == "Y" && !string.IsNullOrWhiteSpace(emailDestino))
                    {
                        var emailRequest = new EmailRequest
                        {
                            To = emailDestino,
                            From = eConfig.User,
                            Subject = "Notificación de vencimiento de registro",
                            Body = body
                        };

                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }
                }
                catch (Exception ex)
                {
                    info.Code = -1;
                    info.Description = ex.Message;
                }
            }

            return info;
        }

        #endregion


        /// <summary>
        /// Obtiene el estado actual del proveedor.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="CodProveedor">Código del proveedor.</param>
        /// <returns>Estado actual del proveedor en la descripción.</returns>
        public ErrorDto ProveedorEstado_Obtener(int CodCliente, int CodProveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<string>(
                CreatePortalDb(),
                CodCliente,
                "SELECT ESTADO FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @CodProveedor",
                string.Empty,
                new { CodProveedor });

            return result.Code == 0
                ? new ErrorDto { Code = 0, Description = result.Result ?? string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al obtener estado del proveedor.", result.Code.GetValueOrDefault(-1));
        }


    }
}
