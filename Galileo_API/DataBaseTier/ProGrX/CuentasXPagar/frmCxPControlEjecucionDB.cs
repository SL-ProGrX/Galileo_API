using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCxPControlEjecucionDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPControlEjecucionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPControlEjecucionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            DBBitacora = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad.
        /// </summary>
        /// <param name="data">Datos del movimiento a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Ejecuta la sincronización de tesorería para reportes de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Resultado de la sincronización.</returns>
        public ErrorDto SincronizaTesoreriaCxPReportes(int CodEmpresa)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "spCxP_SincronizaTesoreria");

            return result.Code == 0
                ? DbHelper.OkResponse("Sincronización finalizada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al sincronizar tesorería de CxP.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene la lista paginada de proveedores para ejecución de pagos.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro libre para código o descripción.</param>
        /// <param name="filtroQ">Filtro adicional ya construido por la lógica del formulario.</param>
        /// <param name="CodContabilidad">Código de la contabilidad activa.</param>
        /// <param name="Vence">Fecha límite para pagos pendientes.</param>
        /// <param name="SoloPendientes">Indica si limita la lista a proveedores con pagos pendientes.</param>
        /// <returns>Listado paginado de proveedores.</returns>
        public ErrorDto<ProveedoresPagosLista> Proveedores_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro,
            string? filtroQ,
            int CodContabilidad = 1,
            DateTime? Vence = null,
            bool SoloPendientes = false)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = new ProveedoresPagosLista
                {
                    Total = 0,
                    Proveedores = new List<ProveedorPagos>()
                };

                var filtroTexto = string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();
                var offset = pagina.GetValueOrDefault();
                var fetch = paginacion.GetValueOrDefault();
                var whereAdicional = ConstruirFiltroProveedorSeguro(filtroQ);
                const string filtroPendientes = @"
                    AND (
                        @SoloPendientes = 0
                        OR EXISTS (
                            SELECT 1
                            FROM CXP_PAGOPROV PP
                            WHERE PP.COD_PROVEEDOR = P.COD_PROVEEDOR
                              AND PP.TESORERIA IS NULL
                              AND PP.FECHA_VENCIMIENTO < DATEADD(day, 1, CONVERT(date, @Vence))
                        )
                    ) ";

                var totalQuery = @"SELECT COUNT(*)
                                   FROM CXP_PROVEEDORES P
                                   INNER JOIN CntX_Divisas D ON P.cod_divisa = D.cod_divisa
                                                               AND D.cod_contabilidad = @CodContabilidad
                                   WHERE 1 = 1 "
                                   + whereAdicional
                                   + filtroPendientes;

                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    totalQuery += " AND (CONVERT(varchar(50), P.COD_PROVEEDOR) LIKE @Filtro OR P.DESCRIPCION LIKE @Filtro) ";
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    totalQuery,
                    new
                    {
                        Filtro = filtroTexto is null ? null : $"%{filtroTexto}%",
                        CodContabilidad,
                        Vence,
                        SoloPendientes
                    });

                var paginaSql = pagina.HasValue && paginacion.HasValue
                    ? " OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY "
                    : string.Empty;

                var query = @"SELECT P.cod_proveedor,
                                      P.descripcion,
                                      RTRIM(D.cod_divisa) + ' - ' + RTRIM(D.descripcion) AS Divisa,
                                      P.CedJur AS Cedjuridica,
                                      P.cod_banco,
                                      dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) AS Cuenta_Default
                               FROM cxp_proveedores P
                               INNER JOIN CntX_Divisas D ON P.cod_divisa = D.cod_divisa
                                                           AND D.cod_contabilidad = @CodContabilidad
                               WHERE 1 = 1 "
                            + whereAdicional
                            + filtroPendientes;

                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    query += " AND (CONVERT(varchar(50), P.COD_PROVEEDOR) LIKE @Filtro OR P.DESCRIPCION LIKE @Filtro) ";
                }

                query += " ORDER BY COD_PROVEEDOR " + paginaSql;

                respuesta.Proveedores = connection.Query<ProveedorPagos>(
                    query,
                    new
                    {
                        Filtro = filtroTexto is null ? null : $"%{filtroTexto}%",
                        Offset = offset,
                        Fetch = fetch,
                        CodContabilidad,
                        Vence,
                        SoloPendientes
                    }).ToList();

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new ProveedoresPagosLista { Total = 0, Proveedores = new List<ProveedorPagos>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener proveedores.", result.Code.GetValueOrDefault(-1), new ProveedoresPagosLista { Total = 0, Proveedores = new List<ProveedorPagos>() });
        }

        /// <summary>
        /// Construye un filtro seguro para proveedores a partir de un conjunto limitado de expresiones conocidas.
        /// </summary>
        /// <param name="filtroQ">Filtro adicional recibido desde la pantalla.</param>
        /// <returns>Fragmento SQL seguro para el WHERE.</returns>
        private static string ConstruirFiltroProveedorSeguro(string? filtroQ)
        {
            if (string.IsNullOrWhiteSpace(filtroQ))
            {
                return string.Empty;
            }

            var valor = filtroQ.Trim();
            var permitido = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AND P.WEB_AUTO_GESTION = 1"] = " AND P.WEB_AUTO_GESTION = 1 ",
                ["AND P.WEB_FERIAS = 1"] = " AND P.WEB_FERIAS = 1 ",
                ["AND P.ESTADO = 'A'"] = " AND P.ESTADO = 'A' ",
                ["AND P.ESTADO = 'I'"] = " AND P.ESTADO = 'I' ",
                ["AND P.ESTADO = 'S'"] = " AND P.ESTADO = 'S' ",
                ["AND P.ESTADO = 'T'"] = " AND P.ESTADO = 'T' ",
                ["AND (P.WEB_AUTO_GESTION = 1 OR P.WEB_FERIAS = 1)"] = " AND (P.WEB_AUTO_GESTION = 1 OR P.WEB_FERIAS = 1) ",
                ["AND P.WEB_AUTO_GESTION = 1 AND P.ESTADO = 'A'"] = " AND P.WEB_AUTO_GESTION = 1 AND P.ESTADO = 'A' ",
                ["AND P.WEB_FERIAS = 1 AND P.ESTADO = 'A'"] = " AND P.WEB_FERIAS = 1 AND P.ESTADO = 'A' ",
                ["AND (P.WEB_AUTO_GESTION = 1 OR P.WEB_FERIAS = 1) AND P.ESTADO = 'A'"] = " AND (P.WEB_AUTO_GESTION = 1 OR P.WEB_FERIAS = 1) AND P.ESTADO = 'A' "
            };

            return permitido.TryGetValue(valor, out var resultado)
                ? resultado
                : string.Empty;
        }

        /// <summary>
        /// Obtiene la divisa funcional configurada para la contabilidad principal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Divisa funcional encontrada.</returns>
        public ErrorDto<Divisa> DivisaFuncional_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<Divisa>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(cod_divisa) as Cod_Divisa,
                         rtrim(descripcion) as Descripcion
                  from CntX_Divisas
                  where cod_contabilidad = 1
                  order by divisa_local desc, cod_divisa",
                null);

            if (result.Code != 0)
            {
                return new ErrorDto<Divisa>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener la divisa funcional.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<Divisa>
                {
                    Code = -2,
                    Description = "No se encontró la divisa funcional.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el listado de cargos adicionales activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cargos adicionales.</returns>
        public ErrorDto<List<Cargo>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<Cargo>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(COD_CARGO) AS cod_cargo,
                         RTRIM(descripcion) AS descripcion
                  FROM cxp_cargos
                  WHERE activo = 1");
        }

        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa, int CodContabilidad)
        {
            return DbHelper.ExecuteListQuery<Divisa>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(cod_divisa) AS Cod_Divisa,
                         RTRIM(descripcion) AS Descripcion
                  FROM CntX_Divisas
                  WHERE cod_contabilidad = @CodContabilidad
                  ORDER BY divisa_local DESC, cod_divisa",
                new { CodContabilidad });
        }

        public ErrorDto<List<UsuarioEjecucion>> Usuarios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<UsuarioEjecucion>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(Nombre) AS Item,
                         RTRIM(Descripcion) AS Descripcion
                  FROM Usuarios
                  ORDER BY Nombre");
        }

        /// <summary>
        /// Obtiene las facturas pendientes de pago para un proveedor y criterios seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros de consulta.</param>
        /// <returns>Listado de facturas pendientes.</returns>
        public ErrorDto<List<FacturaPendientePago>> FacturasPendientePago_Obtener(int CodEmpresa, FactPenReq request)
        {
            var result = DbHelper.ExecuteListQuery<FacturaPendientePago>(
                CreatePortalDb(),
                CodEmpresa,
                "spCxP_FacturasPendientesPago",
                new
                {
                    Proveedor = request.Proveedor,
                    Divisa = request.Divisa,
                    Corte = request.Corte,
                    Usuario = request.Usuario == "" ? null : request.Usuario,
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener facturas pendientes de pago.", result.Code.GetValueOrDefault(-1), new List<FacturaPendientePago>());
            }

            var facturas = result.Result ?? new List<FacturaPendientePago>();
            var corteCargos = DateTime.TryParse(request.CorteCargos, out var fechaCargos)
                ? fechaCargos
                : DateTime.Today;
            decimal saldoCargoFlotante = 0;

            if (facturas.Count > 0)
            {
                using var connection = CreatePortalDb().CreateConnection(CodEmpresa);
                saldoCargoFlotante = connection.QuerySingleOrDefault<decimal>(
                    "SELECT dbo.fxCxP_CargoFlotanteSaldo(@Proveedor, @Corte)",
                    new { request.Proveedor, Corte = corteCargos.Date.AddDays(1).AddTicks(-1) });

                foreach (var item in facturas)
                {
                    var porcentaje = connection.QuerySingleOrDefault<decimal>(
                        @"SELECT ISNULL(SUM(valor / 100), 0)
                          FROM cxp_cargosPer
                          WHERE cod_proveedor = @Proveedor
                            AND tipo = 'P'
                            AND vence >= @Vence",
                        new
                        {
                            Proveedor = item.Cod_Proveedor,
                            Vence = item.Fecha_Vencimiento ?? corteCargos
                        });
                    var cargoPorcentual = item.Apl_Cargo_Flotante
                        ? Math.Max(0, (item.Monto - item.Cargos) * porcentaje)
                        : 0;
                    var disponibleCargoMonto = Math.Max(0, item.Monto - item.Cargos - cargoPorcentual);
                    var cargoPorMonto = item.Apl_Cargo_Flotante
                        ? Math.Min(saldoCargoFlotante, disponibleCargoMonto)
                        : 0;
                    saldoCargoFlotante = Math.Max(0, saldoCargoFlotante - cargoPorMonto);

                    item.Cargo_Directo = item.Cargos;
                    item.Cargo_Flotante = cargoPorcentual + cargoPorMonto;
                    item.Neto = item.Monto - item.Cargo_Directo - item.Cargo_Flotante;
                    item.Cargos_DivReal = item.Tipo_Cambio == 0
                        ? 0
                        : (item.Cargo_Directo + item.Cargo_Flotante) / item.Tipo_Cambio;
                }
            }

            foreach (var item in facturas)
            {
                item.Datakey = item.Npago + "-" + item.Cod_Proveedor + "-" + item.Cod_Factura;
            }

            return DbHelper.CreateOkResponse(facturas);
        }

        /// <summary>
        /// Obtiene el detalle financiero del proveedor para la ejecución de pagos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Vence">Fecha de corte.</param>
        /// <returns>Detalle del proveedor.</returns>
        public ErrorDto<Detalle> DetalleProveedor_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            var result = DbHelper.ExecuteSingleQuery<Detalle>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT p.credito_plazo as Credito,
                         CONVERT(varchar(19), p.ultimo_pago, 126) AS Ultimo_Pago,
                         p.saldo,
                         dbo.fxCxP_CargoFlotanteSaldo(@Cod_Proveedor, @Vence) AS Car_Per_Saldo,
                         ISNULL((SELECT SUM(valor)
                                 FROM cxp_cargosPer
                                 WHERE cod_proveedor = @Cod_Proveedor AND tipo = 'P' AND vence >= @Vence), 0) AS Car_Per_Porc
                  FROM cxp_proveedores p
                  WHERE p.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor, Vence });

            if (result.Code != 0)
            {
                return new ErrorDto<Detalle>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener el detalle del proveedor.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<Detalle>
                {
                    Code = -2,
                    Description = "No se encontró el detalle del proveedor.",
                    Result = null
                };
        }

        /// <summary>
        /// Reactiva pagos cuya solicitud de tesorería fue eliminada pero aún quedó referenciada en cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="User">Usuario que ejecuta la revisión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto RevisionPagos_Reactivar(int CodEmpresa, string User)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE P
                     SET P.TESORERIA = NULL,
                         P.FECHA_TRASLADA = NULL,
                         P.USER_TRASLADA = NULL
                  FROM CXP_PAGOPROV P
                  LEFT JOIN TES_TRANSACCIONES T ON P.TESORERIA = T.NSOLICITUD
                  WHERE ISNULL(P.tesoreria, 0) > 0
                    AND T.NSOLICITUD IS NULL");

            if (result.Code == 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = User,
                    DetalleMovimiento = "Revisión de Pagos de Facturas en Bancos con Solicitud eliminada",
                    Movimiento = "APLICA - WEB",
                    Modulo = 30
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Revisión de pagos reactivada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al reactivar pagos en revisión.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene las autorizaciones registradas para un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de autorizaciones.</returns>
        public ErrorDto<List<Autorizado>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<Autorizado>(
                CreatePortalDb(),
                CodEmpresa,
                @"select *
                  from cxp_autorizaciones
                  where cod_proveedor = @Cod_Proveedor",
                new { Cod_Proveedor });
        }

        /// <summary>
        /// Obtiene la información de fusión asociada a un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor fusionado.</param>
        /// <returns>Información de la fusión encontrada.</returns>
        public ErrorDto<Fusion> Fusion_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<Fusion>(
                CreatePortalDb(),
                CodEmpresa,
                @"select F.cod_proveedor,
                         F.cod_proveedor_fus,
                         rtrim(P.descripcion) as Proveedor
                  from cxp_fusiones F
                  inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor
                  where F.cod_proveedor_fus = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<Fusion>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener la fusión del proveedor.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<Fusion>
                {
                    Code = -2,
                    Description = "No se encontró información de fusión.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene las cuentas autorizadas para desembolso.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cuentas de desembolso.</returns>
        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CuentaDesembolso>(
                CreatePortalDb(),
                CodEmpresa,
                "spCxP_Bancos_Autorizados");
        }

        /// <summary>
        /// Obtiene la información de una cuenta bancaria autorizada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Banco">Código del banco.</param>
        /// <returns>Información de la cuenta bancaria.</returns>
        public ErrorDto<InfoCuenta> InfoCuenta_Obtener(int CodEmpresa, int Cod_Banco)
        {
            var result = DbHelper.ExecuteSingleQuery<InfoCuenta>(
                CreatePortalDb(),
                CodEmpresa,
                @"select id_banco,
                         descripcion,
                         cod_divisa,
                         dbo.fxCntXTipoCambio(1, COD_DIVISA, Getdate(), 'V') as Tipo_Cambio,
                         CTACONTA
                  from Tes_Bancos
                  where id_banco = @Cod_Banco",
                null,
                new { Cod_Banco });

            if (result.Code != 0)
            {
                return new ErrorDto<InfoCuenta>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener la cuenta bancaria.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<InfoCuenta>
                {
                    Code = -2,
                    Description = "No se encontró información de la cuenta.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene las cuentas bancarias de una identificación según banco y validación de divisa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Identificacion">Identificación del beneficiario.</param>
        /// <param name="BancoId">Banco a consultar.</param>
        /// <param name="DivisaCheck">Indicador de validación de divisa.</param>
        /// <returns>Listado de cuentas bancarias.</returns>
        public ErrorDto<List<CuentaBancaria>> CuentasBancarias_Obtener(int CodEmpresa, string Identificacion, int BancoId, int DivisaCheck)
        {
            var identificacionNormalizada = (Identificacion ?? string.Empty).Replace("undefined", string.Empty).Replace(" ", string.Empty).Trim();

            return DbHelper.ExecuteListQuery<CuentaBancaria>(
                CreatePortalDb(),
                CodEmpresa,
                "spSys_Cuentas_Bancarias",
                new
                {
                    Identificacion = identificacionNormalizada,
                    BancoId,
                    DivisaCheck
                });
        }

        /// <summary>
        /// Obtiene los cargos porcentuales vigentes para un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Vence">Fecha de vigencia a evaluar.</param>
        /// <returns>Listado de cargos porcentuales.</returns>
        public ErrorDto<List<CargoPorcentual>> CargoPorcentual_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            return DbHelper.ExecuteListQuery<CargoPorcentual>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT cod_proveedor,
                         (valor / 100) AS Porcentaje
                  FROM cxp_cargosPer
                  WHERE cod_proveedor = @Cod_Proveedor
                    AND tipo = 'P'
                    AND vence >= @Vence",
                new { Cod_Proveedor, Vence });
        }

        /// <summary>
        /// Obtiene el proveedor anterior o siguiente con pagos pendientes según el orden indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Proveedor actual.</param>
        /// <param name="Vence">Fecha de corte.</param>
        /// <param name="tipo">Dirección del desplazamiento: asc o desc.</param>
        /// <returns>Proveedor encontrado según el criterio de navegación.</returns>
        public ErrorDto<ProveedorPagos> ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, int CodContabilidad, string Vence, string tipo)
        {
            string query;
            object parametros;

            if (tipo == "desc")
            {
                if (Cod_Proveedor == 0)
                {
                    query = @"select TOP 1
                                    P.cod_proveedor,
                                    P.descripcion,
                                    rtrim(D.cod_divisa) as Cod_Divisa,
                                    P.CedJur,
                                    rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS Divisa,
                                    P.cod_divisa,
                                    P.CedJur as Cedjuridica,
                                    P.cod_banco,
                                    dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as Cuenta_Default
                              from cxp_proveedores P
                              inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = @CodContabilidad
                              where P.cod_proveedor in(
                                    select cod_proveedor
                                    from cxp_PagoProv
                                    where tesoreria Is Null
                                      and fecha_vencimiento <= @VenceFin)
                              order by cod_proveedor desc";
                    parametros = new { VenceFin = Vence + " 23:59:59", CodContabilidad };
                }
                else
                {
                    query = @"select TOP 1
                                    P.cod_proveedor,
                                    P.descripcion,
                                    rtrim(D.cod_divisa) as Cod_Divisa,
                                    P.CedJur,
                                    rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS Divisa,
                                    P.cod_divisa,
                                    P.CedJur as Cedjuridica,
                                    P.cod_banco,
                                    dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as Cuenta_Default
                              from cxp_proveedores P
                              inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = @CodContabilidad
                              where P.cod_proveedor in(
                                    select cod_proveedor
                                    from cxp_PagoProv
                                    where tesoreria Is Null
                                      and fecha_vencimiento <= @VenceFin
                                      and cod_proveedor < @Cod_Proveedor
                                    group by cod_proveedor)
                              order by cod_proveedor desc";
                    parametros = new { VenceFin = Vence + " 23:59:59", Cod_Proveedor, CodContabilidad };
                }
            }
            else
            {
                query = @"select TOP 1
                                P.cod_proveedor,
                                P.descripcion,
                                rtrim(D.cod_divisa) as Cod_Divisa,
                                P.CedJur,
                                rtrim(D.cod_divisa) + ' - ' + rtrim(D.descripcion) AS Divisa,
                                P.cod_divisa,
                                P.CedJur as Cedjuridica,
                                P.cod_banco,
                                dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) as Cuenta_Default
                          from cxp_proveedores P
                          inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = @CodContabilidad
                          where P.cod_proveedor in(
                                select cod_proveedor
                                from cxp_PagoProv
                                where tesoreria Is Null
                                  and fecha_vencimiento <= @VenceFin
                                  and cod_proveedor > @Cod_Proveedor
                                group by cod_proveedor)
                          order by cod_proveedor asc";
                parametros = new { VenceFin = Vence + " 23:59:59", Cod_Proveedor, CodContabilidad };
            }

            var result = DbHelper.ExecuteSingleQuery<ProveedorPagos>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                parametros);

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorPagos>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al consultar navegación de proveedores.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorPagos>
                {
                    Code = -2,
                    Description = "No se encontró proveedor para el criterio indicado.",
                    Result = null
                };
        }

        public ErrorDto<ProveedorPagos> Proveedor_Obtener(int CodEmpresa, int Cod_Proveedor, int CodContabilidad)
        {
            var result = DbHelper.ExecuteSingleQuery<ProveedorPagos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.cod_proveedor,
                         RTRIM(P.descripcion) AS Descripcion,
                         RTRIM(D.cod_divisa) AS Divisa,
                         RTRIM(P.CedJur) AS Cedjuridica,
                         P.cod_Banco,
                         dbo.fxSys_Cuenta_Bancos_Desc(P.cod_Banco) AS Cuenta_Default
                  FROM cxp_proveedores P
                  INNER JOIN CntX_Divisas D ON P.cod_divisa = D.cod_divisa
                                           AND D.cod_contabilidad = @CodContabilidad
                  WHERE P.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor, CodContabilidad });

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse<ProveedorPagos>(
                    result.Description ?? "No se encontró el proveedor.", result.Code.GetValueOrDefault(-2));
        }

        /// <summary>
        /// Inserta el detalle contable de una transacción de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Detalle a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Detalle_Insertar(int CodEmpresa, TesTransAsiento data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT Tes_Trans_Asiento(nsolicitud, cuenta_contable, monto, debehaber, linea, cod_unidad, cod_cc, cod_divisa, tipo_cambio)
                  VALUES(@NSolicitud, @Cuenta_Contable, @Monto, @DebeHaber, @Linea, @Cod_Unidad, @Cod_Cc, @Cod_Divisa, @Tipo_Cambio)",
                new
                {
                    data.NSolicitud,
                    data.Cuenta_Contable,
                    data.Monto,
                    data.DebeHaber,
                    data.Linea,
                    data.Cod_Unidad,
                    data.Cod_Cc,
                    data.Cod_Divisa,
                    data.Tipo_Cambio
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro agregado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar detalle de tesorería.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el monto de cargos asociados a anticipos ya trasladados a tesorería para un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Información de anticipos aplicada al proveedor.</returns>
        public ErrorDto<Anticipo> MontoAnticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<Anticipo>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ISNULL(SUM(Pc.monto), 0) AS Cargos
                  FROM CXP_CARGOSPER Cp
                  INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                  INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                  INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                  WHERE Cp.COD_PROVEEDOR = @Cod_Proveedor
                    AND Pf.user_traslada = 'xBITxTesx'",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<Anticipo>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener monto de anticipos.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<Anticipo>
                {
                    Code = -2,
                    Description = "No se encontró información de anticipos.",
                    Result = null
                };
        }

        /// <summary>
        /// Inserta una solicitud en tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Información de la solicitud.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Tesoreria_Insertar(int CodEmpresa, TesTransacciones data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT Tes_Transacciones(id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud, estado, estadoi,
                                           modulo, submodulo, cta_ahorros, detalle1, detalle2, referencia, op, genera, actualiza, cod_unidad,
                                           cod_concepto, user_solicita, autoriza, fecha_autorizacion, user_autoriza, tipo_beneficiario, tipo_cambio, cod_divisa)
                  VALUES(@Id_Banco, @Tipo, @Codigo, @Beneficiario, @Monto, @Fecha_Solicitud, @Estado, @Estadoi,
                         @Modulo, @Submodulo, @Cta_Ahorros, @Detalle1, @Detalle2, @Referencia, @Op, @Genera, @Actualiza, @Cod_Unidad,
                         @Cod_Concepto, @User_Solicita, @Autoriza, @Fecha_Autorizacion, @User_Autoriza, @Tipo_Beneficiario, @Tipo_Cambio, @Cod_Divisa)",
                new
                {
                    data.Id_Banco,
                    data.Tipo,
                    data.Codigo,
                    data.Beneficiario,
                    data.Monto,
                    data.Fecha_Solicitud,
                    data.Estado,
                    data.Estadoi,
                    data.Modulo,
                    data.Submodulo,
                    data.Cta_Ahorros,
                    data.Detalle1,
                    data.Detalle2,
                    data.Referencia,
                    data.Op,
                    data.Genera,
                    data.Actualiza,
                    data.Cod_Unidad,
                    data.Cod_Concepto,
                    data.User_Solicita,
                    data.Autoriza,
                    data.Fecha_Autorizacion,
                    data.User_Autoriza,
                    data.Tipo_Beneficiario,
                    data.Tipo_Cambio,
                    data.Cod_Divisa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro agregado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar solicitud de tesorería.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene una solicitud de tesorería por número.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="nSolicitud">Número de solicitud.</param>
        /// <returns>Información de la solicitud encontrada.</returns>
        public ErrorDto<TesTransacciones> Tesoreria_Obtener(int CodEmpresa, int nSolicitud)
        {
            var result = DbHelper.ExecuteSingleQuery<TesTransacciones>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM Tes_Transacciones WHERE nsolicitud = @nSolicitud",
                null,
                new { nSolicitud });

            if (result.Code != 0)
            {
                return new ErrorDto<TesTransacciones>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener tesorería.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<TesTransacciones>
                {
                    Code = -2,
                    Description = "No se encontró la solicitud de tesorería.",
                    Result = null
                };
        }

        public ErrorDto EjecucionPagosCargos_Registra(int CodEmpresa, FacturaPendientePago data)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                var procedure = "spCxP_EjecucionPagos_RegistroCargos";

                var values = new
                {
                    Proveedor = data.Proveedor,
                    Factura = data.Cod_Factura,
                    NPago = data.Npago,
                    CodCargo = data.Cod_Cargo,
                    Divisa = data.Cod_Divisa,
                    Monto = data.Monto,
                    TipoCambio = data.Tipo_Cambio,
                    Usuario = data.Usuario,

                };

                resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                resp.Description = "Registro agregado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }





        public ErrorDto EjecucionPagos_CargosFlotantes_Aplicar(int CodEmpresa, FacturaPendientePago data)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                var procedure = "spCxP_EjecucionPagos_AplicaCargosFlotantes";

                var values = new
                {
                    Proveedor = data.Cod_Proveedor,
                    Factura = data.Cod_Factura,
                    NPago = data.Npago,
                    Disponible = data.Neto,
                    Corte = $"{data.Fecha_Vencimiento:yyyy/MM/dd} 23:59:59",
                    AplicaCargos = data.Apl_Cargo_Flotante,
                    Usuario = data.Usuario,

                };

                resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                resp.Description = "Cargos aplicados correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto EjecucionPagos_SaldosCargoPorc_Actualizar(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                var procedure = "spCxP_EjecucionPagos_ActualizaSaldosConCargosPorc";

                resp.Code = connection.Query<int>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                resp.Description = "Cargos actualizados correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los desembolsos netos acumulados de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Totales netos del proveedor.</returns>
        public ErrorDto<DesembolsoNetos> DesembolsoNetos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<DesembolsoNetos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CEDJUR,
                         cod_proveedor,
                         SUM(monto - cargos) AS Neto,
                         SUM(Divisa_Real_Neto) AS Divisa_Real_Neto
                  FROM vCXP_Pagos
                  WHERE cod_Proveedor = @Cod_Proveedor
                  GROUP BY cod_proveedor, CEDJUR",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<DesembolsoNetos>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener desembolsos netos.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<DesembolsoNetos>
                {
                    Code = -2,
                    Description = "No se encontró información de desembolsos netos.",
                    Result = null
                };
        }

        /// <summary>
        /// Actualiza los indicadores de pagos enviados a tesorería para un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Indicadores_Actualizar(int CodEmpresa, PagoProvUpdate data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_pagoprov
                  SET tesoreria = @Tesoreria,
                      fecha_traslada = GETDATE(),
                      user_traslada = @User_Traslada,
                      pago_tercero = @Pago_Tercero
                  WHERE user_traslada = 'xBITxTesx'
                    AND cod_proveedor = @Cod_Proveedor",
                new
                {
                    data.Tesoreria,
                    data.User_Traslada,
                    Pago_Tercero = data.IsPagoTerceroChecked ? data.Pago_Tercero : string.Empty,
                    data.Cod_Proveedor
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar indicadores de pago.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza la cancelación de cargos trasladados a tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CancelacionCargos_Actualizar(int CodEmpresa, int Cod_Proveedor, string Usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_pagoprov
                  SET tesoreria = 0,
                      Tipo_Cancelacion = 'C',
                      Tesoreria_Estado = 'E',
                      fecha_traslada = GETDATE(),
                      user_traslada = @Usuario,
                      pago_tercero = '',
                      Tesoreria_Emision = GETDATE()
                  WHERE user_traslada = 'xBITxTesx'
                    AND cod_proveedor = @Cod_Proveedor",
                new { Usuario, Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Cargos actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar cancelación de cargos.", result.Code.GetValueOrDefault(-1));
        }

        public ErrorDto EjecucionPagos_TesoreriaDetalle_Actualizar(int CodEmpresa)
        {
            ErrorDto resp = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                var procedure = "spCxP_Tesoreria_Detalle_Update";

                resp.Code = connection.Query<int>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                resp.Description = "Registro actualizado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los cargos periódicos asociados a anticipos trasladados del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de cargos periódicos.</returns>
        public ErrorDto<List<CargoPer>> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<CargoPer>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT Cr.COD_CARGO,
                         Cr.DESCRIPCION,
                         Cr.COD_CUENTA,
                         Pc.monto,
                         Pc.COD_DIVISA
                  FROM CXP_CARGOSPER Cp
                  INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                  INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                  INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                  INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                  WHERE Cp.COD_PROVEEDOR = @Cod_Proveedor
                    AND Pf.user_traslada = 'xBITxTesx'",
                new { Cod_Proveedor });
        }

        /// <summary>
        /// Obtiene la información del proveedor necesaria para tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <returns>Información del proveedor para tesorería.</returns>
        public ErrorDto<ProveedorInfoEjecucion> ProveedorTesoreria_Obtener(int CodEmpresa, int Cod_Proveedor, int cod_contabilidad)
        {
            var result = DbHelper.ExecuteSingleQuery<ProveedorInfoEjecucion>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.CEDJUR,
                         P.cod_proveedor,
                         P.descripcion,
                         P.cod_cuenta,
                         P.cod_divisa,
                         D.cod_cuenta AS CtaDivDifIng,
                         D.cod_cuenta_Gasto AS CtaDivDifGst,
                         dbo.fxCntXTipoCambio(1, P.COD_DIVISA, GETDATE(), 'V') AS TipoCambio,
                         GETDATE() AS Fecha
                  FROM Cxp_Proveedores P
                  INNER JOIN CntX_Divisas D ON P.cod_divisa = D.cod_divisa
                  WHERE D.cod_contabilidad = @cod_contabilidad
                    AND P.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor, cod_contabilidad });

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorInfoEjecucion>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener proveedor para tesorería.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorInfoEjecucion>
                {
                    Code = -2,
                    Description = "No se encontró información del proveedor para tesorería.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene los anticipos trasladados asociados al proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de anticipos.</returns>
        public ErrorDto<List<Anticipo>> Anticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<Anticipo>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT Cr.COD_CARGO,
                         Cr.DESCRIPCION,
                         Cr.COD_CUENTA,
                         Pc.monto,
                         Pc.COD_DIVISA
                  FROM CXP_CARGOSPER Cp
                  INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                  INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                  INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                  INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                  WHERE Cp.COD_PROVEEDOR = @Cod_Proveedor
                    AND Pf.user_traslada = 'xBITxTesx'",
                new { Cod_Proveedor });
        }

        public ErrorDto<EjecucionPagosResultado> EjecucionPagos_Aplicar(int CodEmpresa, EjecucionPagosAplicar data)
        {
            if (data.Cod_Proveedor <= 0 || data.Pagos.Count == 0 || string.IsNullOrWhiteSpace(data.Usuario))
            {
                return DbHelper.CreateErrorResponse<EjecucionPagosResultado>(
                    "Debe seleccionar un proveedor y al menos un pago.", -1);
            }

            if (data.Pagos.Any(pago => pago.Cod_Proveedor != data.Cod_Proveedor))
            {
                return DbHelper.CreateErrorResponse<EjecucionPagosResultado>(
                    "Los pagos seleccionados no corresponden al proveedor indicado.", -1);
            }

            if (data.Tipo_Cancelacion == "C" && string.IsNullOrWhiteSpace(data.Cod_Cargo))
            {
                return DbHelper.CreateErrorResponse<EjecucionPagosResultado>(
                    "Debe seleccionar el cargo para cerrar los pagos.", -1);
            }

            if (data.Tipo_Cancelacion == "D" &&
                string.Equals(data.Tipo_Pago, "Transferencia", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(data.Cuenta_Banco))
            {
                return DbHelper.CreateErrorResponse<EjecucionPagosResultado>(
                    "No es posible crear la transferencia sin una cuenta bancaria del proveedor.", -1);
            }

            try
            {
                using var connection = CreatePortalDb().CreateConnection(CodEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

                try
                {
                    foreach (var pago in data.Pagos)
                    {
                        if (data.Tipo_Cancelacion == "C")
                        {
                            connection.Execute(
                                "spCxP_EjecucionPagos_RegistroCargos",
                                new
                                {
                                    Proveedor = pago.Cod_Proveedor,
                                    Factura = pago.Cod_Factura,
                                    NPago = pago.Npago,
                                    CodCargo = data.Cod_Cargo,
                                    Divisa = pago.Cod_Divisa,
                                    Monto = pago.Neto,
                                    TipoCambio = pago.Tipo_Cambio,
                                    Usuario = data.Usuario
                                },
                                transaction,
                                commandType: CommandType.StoredProcedure);
                        }

                        connection.Execute(
                            "spCxP_EjecucionPagos_AplicaCargosFlotantes",
                            new
                            {
                                Proveedor = pago.Cod_Proveedor,
                                Factura = pago.Cod_Factura,
                                NPago = pago.Npago,
                                Disponible = pago.Monto - pago.Cargo_Directo,
                                Corte = data.Corte_Cargos.Date.AddDays(1).AddTicks(-1),
                                AplicaCargos = pago.Apl_Cargo_Flotante,
                                Usuario = data.Usuario
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                    }

                    connection.Execute(
                        "spCxP_EjecucionPagos_ActualizaSaldosConCargosPorc",
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);

                    var solicitud = data.Tipo_Cancelacion == "D"
                        ? CrearSolicitudTesoreria(connection, transaction, data)
                        : 0;

                    if (data.Tipo_Cancelacion == "D")
                    {
                        connection.Execute(
                            @"UPDATE cxp_pagoprov
                                 SET tesoreria = @Solicitud,
                                     fecha_traslada = dbo.MyGetdate(),
                                     user_traslada = @Usuario,
                                     pago_tercero = @PagoTercero
                               WHERE user_traslada = 'xBITxTesx'
                                 AND cod_proveedor = @Proveedor",
                            new
                            {
                                Solicitud = solicitud,
                                data.Usuario,
                                PagoTercero = data.Pagar_Tercero ? data.Pago_Tercero : string.Empty,
                                Proveedor = data.Cod_Proveedor
                            },
                            transaction);
                    }
                    else
                    {
                        connection.Execute(
                            @"UPDATE cxp_pagoprov
                                 SET tesoreria = 0,
                                     Tipo_Cancelacion = 'C',
                                     Tesoreria_Estado = 'E',
                                     fecha_traslada = dbo.MyGetdate(),
                                     user_traslada = @Usuario,
                                     pago_tercero = '',
                                     Tesoreria_Emision = dbo.MyGetdate()
                               WHERE user_traslada = 'xBITxTesx'
                                 AND cod_proveedor = @Proveedor",
                            new { data.Usuario, Proveedor = data.Cod_Proveedor },
                            transaction);
                    }

                    connection.Execute(
                        "spCxP_Tesoreria_Detalle_Update",
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                    return DbHelper.CreateOkResponse(
                        new EjecucionPagosResultado
                        {
                            NSolicitud = solicitud,
                            Tipo_Cancelacion = data.Tipo_Cancelacion
                        },
                        data.Tipo_Cancelacion == "D"
                            ? $"Pago registrado en Tesorería. Solicitud: {solicitud}"
                            : "Cuenta por pagar descontada vía cargos.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<EjecucionPagosResultado>(
                    ex.Message, -1);
            }
        }

        private static int CrearSolicitudTesoreria(
            IDbConnection connection,
            IDbTransaction transaction,
            EjecucionPagosAplicar data)
        {
            var desembolso = connection.QuerySingleOrDefault<DesembolsoNetos>(
                @"SELECT CEDJUR,
                         cod_proveedor,
                         SUM(monto - cargos) AS Neto,
                         SUM(Divisa_Real_Neto) AS Divisa_Real_Neto
                  FROM vCXP_Pagos
                  WHERE cod_Proveedor = @Proveedor
                  GROUP BY cod_proveedor, CEDJUR",
                new { Proveedor = data.Cod_Proveedor },
                transaction) ?? throw new InvalidOperationException(
                    "No se encontraron pagos preparados para el desembolso.");

            if (desembolso.Neto <= 0)
            {
                throw new InvalidOperationException(
                    "El monto a girar es cero; utilice la opción Cerrar / Excluir.");
            }

            var proveedor = connection.QuerySingleOrDefault<ProveedorInfoEjecucion>(
                @"SELECT P.CEDJUR,
                         P.cod_proveedor,
                         P.descripcion,
                         P.cod_cuenta,
                         P.cod_divisa,
                         D.cod_cuenta AS CtaDivDifIng,
                         D.cod_cuenta_Gasto AS CtaDivDifGst,
                         dbo.fxCntXTipoCambio(@Contabilidad, P.COD_DIVISA, dbo.MyGetdate(), 'V') AS TipoCambio,
                         dbo.MyGetdate() AS Fecha
                  FROM Cxp_Proveedores P
                  INNER JOIN CntX_Divisas D ON P.cod_divisa = D.cod_divisa
                                           AND D.cod_contabilidad = @Contabilidad
                  WHERE P.cod_proveedor = @Proveedor",
                new
                {
                    Contabilidad = data.Cod_Contabilidad,
                    Proveedor = data.Cod_Proveedor
                },
                transaction) ?? throw new InvalidOperationException(
                    "No se encontró la información contable del proveedor.");

            var cuentaBanco = connection.QuerySingleOrDefault<string>(
                "SELECT CTACONTA FROM Tes_Bancos WHERE id_banco = @Banco",
                new { Banco = data.Banco_Id },
                transaction) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cuentaBanco))
            {
                throw new InvalidOperationException(
                    "El banco seleccionado no tiene una cuenta contable configurada.");
            }

            var parametros = connection.Query<(string Cod_Parametro, string Valor)>(
                @"SELECT cod_parametro, valor
                  FROM cxp_parametros
                  WHERE cod_parametro IN ('01', '02')",
                transaction: transaction)
                .ToDictionary(item => item.Cod_Parametro, item => item.Valor);
            var unidad = parametros.GetValueOrDefault("01", "GEN");
            var concepto = parametros.GetValueOrDefault("02", "GEN");
            var tipoPago = string.Equals(data.Tipo_Pago, "Transferencia", StringComparison.OrdinalIgnoreCase) &&
                           !string.IsNullOrWhiteSpace(data.Cuenta_Banco)
                ? "TE"
                : "CK";
            var montoSolicitud = data.Banco_Tipo_Cambio != 1
                ? desembolso.Neto / data.Banco_Tipo_Cambio
                : desembolso.Neto;
            var beneficiario = data.Pagar_Tercero && !string.IsNullOrWhiteSpace(data.Pago_Tercero)
                ? data.Pago_Tercero
                : proveedor.Descripcion;

            var solicitud = connection.QuerySingle<int>(
                @"EXEC spCxP_Tesoreria_Maestro
                    @Proveedor, @TipoDocumento, @Banco, @Monto, @Codigo, @Beneficiario,
                    @Detalle1, @Detalle2, @Cuenta, @Fecha, @UnidadOmision,
                    @ConceptoOmision, @UsuarioSolicita, @TipoCambio, @Divisa,
                    @OP, @Referencia, @Token",
                new
                {
                    Proveedor = data.Cod_Proveedor,
                    TipoDocumento = tipoPago,
                    Banco = data.Banco_Id,
                    Monto = montoSolicitud,
                    Codigo = proveedor.CedJur,
                    Beneficiario = beneficiario,
                    Detalle1 = "MODULO DE PROVEEDORES",
                    Detalle2 = "PAGO AUTOMATICO",
                    Cuenta = data.Cuenta_Banco,
                    Fecha = proveedor.Fecha,
                    UnidadOmision = unidad,
                    ConceptoOmision = concepto,
                    UsuarioSolicita = data.Usuario,
                    TipoCambio = data.Banco_Tipo_Cambio,
                    Divisa = data.Banco_Divisa,
                    OP = 0,
                    Referencia = 0,
                    Token = string.Empty
                },
                transaction);

            var linea = 1;
            InsertarDetalleTesoreria(
                connection, transaction, solicitud, cuentaBanco, desembolso.Neto,
                "H", linea, unidad, data.Banco_Divisa, data.Banco_Tipo_Cambio);

            var anticipos = connection.Query<Anticipo>(
                @"SELECT Cr.COD_CARGO,
                         Cr.DESCRIPCION,
                         Cr.COD_CUENTA,
                         Pc.monto,
                         Pc.COD_DIVISA
                  FROM CXP_CARGOSPER Cp
                  INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR
                                             AND Cp.COD_CARGO = Ca.COD_CARGO
                                             AND Cp.ID = Ca.ID_CARGO
                  INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                  INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR
                                                  AND Pf.COD_FACTURA = Pc.COD_FACTURA
                                                  AND Pc.NPAGO = Pf.NPAGO
                                                  AND Pc.ID = Cp.ID
                  INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                  WHERE Cp.COD_PROVEEDOR = @Proveedor
                    AND Pf.user_traslada = 'xBITxTesx'",
                new { Proveedor = data.Cod_Proveedor },
                transaction)
                .ToList();

            foreach (var anticipo in anticipos)
            {
                linea++;
                InsertarDetalleTesoreria(
                    connection, transaction, solicitud, anticipo.Cod_Cuenta,
                    anticipo.Monto, "H", linea, unidad, anticipo.Cod_Divisa, 1);
            }

            var montoAnticipos = anticipos.Sum(item => item.Monto);
            var montoProveedor = desembolso.Neto + montoAnticipos;
            var divisaFuncional = connection.QuerySingleOrDefault<string>(
                @"SELECT RTRIM(COD_DIVISA)
                  FROM CNTX_DIVISAS
                  WHERE DIVISA_LOCAL = 1
                    AND COD_CONTABILIDAD = @Contabilidad",
                new { Contabilidad = data.Cod_Contabilidad },
                transaction) ?? string.Empty;
            var tipoCambioProveedor = string.Equals(
                proveedor.Cod_Divisa, divisaFuncional, StringComparison.OrdinalIgnoreCase)
                ? proveedor.Tipo_Cambio
                : desembolso.Divisa_Real_Neto == 0
                    ? proveedor.Tipo_Cambio
                    : desembolso.Neto / desembolso.Divisa_Real_Neto;

            linea++;
            InsertarDetalleTesoreria(
                connection, transaction, solicitud, proveedor.Cod_Cuenta,
                montoProveedor, "D", linea, unidad, proveedor.Cod_Divisa,
                tipoCambioProveedor);

            return solicitud;
        }

        private static void InsertarDetalleTesoreria(
            IDbConnection connection,
            IDbTransaction transaction,
            int solicitud,
            string cuenta,
            decimal monto,
            string debeHaber,
            int linea,
            string unidad,
            string divisa,
            decimal tipoCambio)
        {
            connection.Execute(
                @"INSERT Tes_Trans_Asiento(
                        nsolicitud, cuenta_contable, monto, debehaber, linea,
                        cod_unidad, cod_cc, cod_divisa, tipo_cambio)
                  VALUES(
                        @Solicitud, @Cuenta, @Monto, @DebeHaber, @Linea,
                        @Unidad, '', @Divisa, @TipoCambio)",
                new
                {
                    Solicitud = solicitud,
                    Cuenta = cuenta.Trim(),
                    Monto = monto,
                    DebeHaber = debeHaber,
                    Linea = linea,
                    Unidad = unidad,
                    Divisa = divisa,
                    TipoCambio = tipoCambio
                },
                transaction);
        }
        
        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
