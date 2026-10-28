using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCxPFacturasDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        #region Helpers privados

        /// <summary>
        /// Crea una respuesta vacía para el listado de facturas.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static FacturaLista CrearFacturaListaVacia() => new()
        {
            Total = 0,
            Facturas = new List<Factura>()
        };

        /// <summary>
        /// Crea una respuesta vacía para el listado de plantillas de factura.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static FacturaPlantillaLista CrearFacturaPlantillaListaVacia() => new()
        {
            Total = 0,
            Plantillas = new List<FacturaPlantilla>()
        };

        /// <summary>
        /// Crea los parámetros comunes para factura y proveedor.
        /// </summary>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosFacturaProveedor(string Cod_Factura, int Cod_Proveedor) => new
        {
            Cod_Factura,
            Cod_Proveedor
        };

        /// <summary>
        /// Asigna la llave compuesta a una colección de facturas.
        /// </summary>
        /// <param name="facturas">Facturas a procesar.</param>
        private static void AsignarDataKeysFacturas(IEnumerable<Factura> facturas)
        {
            foreach (Factura ft in facturas)
            {
                ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
            }
        }

        /// <summary>
        /// Registra una bitácora de facturas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <param name="detalleMovimiento">Detalle del movimiento.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        private void RegistrarBitacoraFactura(int CodEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = 30
            });
        }

        /// <summary>
        /// Convierte el resultado de una consulta única en una respuesta estándar.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por DbHelper.</param>
        /// <param name="errorMessage">Mensaje de error cuando la consulta falla.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para consultas de una sola entidad.</returns>
        private static ErrorDto<T> CrearRespuestaSingle<T>(ErrorDto<T?> result, string errorMessage, string notFoundMessage)
            where T : class
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<T>
                {
                    Code = -2,
                    Description = notFoundMessage,
                    Result = null
                };
        }

        #endregion
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPFacturasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPFacturasDB(IConfiguration config)
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
        /// Obtiene los parámetros de IVA y sus cuentas asociadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de parámetros de IVA.</returns>
        public ErrorDto<List<ParametrosIva>> ParamIVA_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<ParametrosIva>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.COD_PARAMETRO,
                         P.VALOR,
                         P.DESCRIPCION,
                         C.COD_CUENTA_MASK,
                         C.DESCRIPCION AS DESC_CUENTA
                  FROM SYS_IVA_PARAMETROS P
                  LEFT JOIN CntX_Cuentas C ON C.cod_cuenta = P.VALOR AND C.cod_contabilidad = 1
                  WHERE COD_PARAMETRO in('02','03','08')");
        }

        public ErrorDto<List<TipoIvaFactura>> TiposIVA_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<TipoIvaFactura>(
                    "[spSys_IVA_TIPOS_List]",
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TipoIvaFactura>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de IVA.", result.Code.GetValueOrDefault(-1), new List<TipoIvaFactura>());
        }

        public ErrorDto<TipoIvaFactura> TipoIVA_Consultar(int CodEmpresa, string Cod_IVA)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<TipoIvaFactura>(
                    "[spSys_IVA_TIPOS_Consulta]",
                    new { TipoIVA = Cod_IVA },
                    commandType: CommandType.StoredProcedure));

            return CrearRespuestaSingle(result, "Error al consultar el tipo de IVA.", "No se encontró el tipo de IVA.");
        }

        public ErrorDto<List<FacturaIvaDetalle>> FacturaIVA_Obtener(int CodEmpresa, int Cod_Proveedor, string Cod_Factura)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<FacturaIvaDetalle>(
                    "[spCXP_Factura_IVA_List]",
                    new { Proveedor = Cod_Proveedor, Factura = Cod_Factura },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<FacturaIvaDetalle>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el detalle de IVA.", result.Code.GetValueOrDefault(-1), new List<FacturaIvaDetalle>());
        }

        /// <summary>
        /// Obtiene la divisa local configurada en la contabilidad principal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Divisa local configurada.</returns>
        public ErrorDto<DivisaLocal> DivisaLocal_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<DivisaLocal>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(cod_divisa) as cod_divisa,
                         rtrim(descripcion) as descripcion
                  from CntX_Divisas
                  where cod_contabilidad = 1
                    and Divisa_Local = 1",
                null);

            return CrearRespuestaSingle(
                result,
                "Error al obtener la divisa local.",
                "No se encontró la divisa local.");
        }

        /// <summary>
        /// Obtiene el listado de divisas disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de divisas.</returns>
        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<Divisa>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(cod_divisa) as Cod_Divisa,
                         rtrim(descripcion) as Descripcion
                  from CntX_Divisas
                  where cod_contabilidad = 1
                  order by divisa_local desc, cod_divisa");
        }

        /// <summary>
        /// Obtiene las unidades activas de la contabilidad principal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de unidades.</returns>
        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<Unidad>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT cod_unidad,
                         descripcion
                  FROM CntX_unidades
                  WHERE Activa = 1
                    and cod_contabilidad = 1");
        }

        /// <summary>
        /// Obtiene los centros de costo activos asociados a una unidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Unidad">Código de la unidad.</param>
        /// <returns>Listado de centros de costo.</returns>
        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return DbHelper.ExecuteListQuery<CentroCosto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(COD_CENTRO_COSTO) AS cod_centro_costo,
                         RTRIM(descripcion) AS descripcion
                  FROM CNTX_CENTRO_COSTOS
                  WHERE COD_CONTABILIDAD = 1
                    AND ACTIVO = 1
                    AND COD_CENTRO_COSTO IN (
                        SELECT COD_CENTRO_COSTO
                        FROM CNTX_UNIDADES_CC
                        WHERE COD_CONTABILIDAD = 1
                          AND COD_UNIDAD = @Cod_Unidad)",
                new { Cod_Unidad });
        }

        /// <summary>
        /// Obtiene el listado paginado de facturas con filtro opcional.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor. Use 0 para todos.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas por página.</param>
        /// <param name="filtro">Filtro libre por factura, notas o proveedor.</param>
        /// <returns>Listado paginado de facturas.</returns>
        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = CrearFacturaListaVacia();

                var parametros = new DynamicParameters();
                var totalBuilder = new System.Text.StringBuilder("SELECT COUNT(*) from cxp_facturas F inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor");
                var detalleBuilder = new System.Text.StringBuilder(@"select F.cod_factura, F.cod_proveedor, P.descripcion as Proveedor,
                                                                           F.TOTAL AS TOTAL_FACTURA, F.notas, F.fecha,
                                                                           F.cod_forma_pago as Tipo
                                                                    from cxp_facturas F
                                                                    inner join cxp_proveedores P on F.cod_proveedor = P.cod_proveedor");
                var condiciones = new List<string>();

                if (Cod_Proveedor > 0)
                {
                    condiciones.Add("F.cod_proveedor = @Cod_Proveedor");
                    parametros.Add("Cod_Proveedor", Cod_Proveedor);
                }

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(F.COD_FACTURA LIKE @Filtro OR F.NOTAS LIKE @Filtro OR P.DESCRIPCION LIKE @Filtro OR CAST(F.COD_PROVEEDOR AS varchar(50)) LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                if (condiciones.Count > 0)
                {
                    var where = " WHERE " + string.Join(" AND ", condiciones);
                    totalBuilder.Append(where);
                    detalleBuilder.Append(where);
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" order by F.cod_factura, F.cod_proveedor");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Facturas = connection.Query<Factura>(detalleBuilder.ToString(), parametros).ToList();
                AsignarDataKeysFacturas(respuesta.Facturas);

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearFacturaListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener facturas.", result.Code.GetValueOrDefault(-1), CrearFacturaListaVacia());
        }

        /// <summary>
        /// Obtiene el detalle de una factura y su saldo actual.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor. Use 0 para omitir este filtro.</param>
        /// <returns>Detalle de la factura.</returns>
        public ErrorDto<FacturaDto> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var factura = connection.Query<FacturaDto>(
                    "[spCXP_Factura_Load]",
                    new { Proveedor = Cod_Proveedor, Factura = Cod_Factura },
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault(item => Cod_Proveedor <= 0 || item.Cod_Proveedor == Cod_Proveedor);

                if (factura is not null && factura.Cod_Forma_Pago == "CO")
                {
                    factura.Apl_Cargo_Flotante = connection.QueryFirstOrDefault<int?>(
                        @"SELECT TOP 1 APL_CARGO_FLOTANTE
                          FROM CXP_PAGOPROV
                          WHERE COD_PROVEEDOR = @Cod_Proveedor AND COD_FACTURA = @Cod_Factura
                          ORDER BY NPAGO",
                        new { factura.Cod_Proveedor, factura.Cod_Factura }) ?? 0;
                }

                return factura;
            });

            return CrearRespuestaSingle(
                result,
                "Error al obtener detalle de la factura.",
                "No se encontró la factura.");
        }

        /// <summary>
        /// Obtiene los asientos contables de una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de asientos de la factura.</returns>
        public ErrorDto<List<AsientoFactura>> FacturaAsientos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteListQuery<AsientoFactura>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT D.linea,
                         COALESCE(C.cod_Cuenta_Mask, D.cod_Cuenta) AS Cod_Cuenta_Mask,
                         D.cod_cuenta,
                         C.descripcion as Cuenta,
                         D.debehaber,
                         D.monto,
                         D.cod_unidad,
                         U.descripcion as Unidad,
                         D.cod_centro_costo,
                         X.descripcion as CentroCosto,
                         D.cod_proveedor,
                         D.cod_factura,
                         D.cod_Divisa,
                         Div.Descripcion as Divisa,
                         D.Tipo_Cambio,
                         CASE WHEN D.debehaber = 'D' THEN D.monto ELSE 0 END AS Debito,
                         CASE WHEN D.debehaber = 'H' THEN D.monto ELSE 0 END AS Credito
                  FROM CXP_FACTURAS_DETALLE D
                  INNER JOIN CXP_FACTURAS Ch ON D.cod_factura = Ch.cod_factura AND D.cod_proveedor = Ch.Cod_Proveedor
                  LEFT JOIN CntX_Cuentas C ON D.cod_cuenta = C.cod_cuenta AND D.cod_contabilidad = C.cod_Contabilidad
                  LEFT JOIN CntX_Divisas Div ON D.cod_divisa = Div.cod_Divisa AND D.cod_contabilidad = Div.cod_Contabilidad
                  LEFT JOIN CntX_unidades U ON D.cod_unidad = U.cod_unidad AND U.cod_contabilidad = D.cod_Contabilidad
                  LEFT JOIN CNTX_CENTRO_COSTOS X ON D.cod_centro_costo = X.COD_CENTRO_COSTO AND X.cod_contabilidad = 1
                  WHERE D.cod_factura = @Cod_Factura
                    AND D.cod_proveedor = @Cod_Proveedor
                  ORDER BY D.linea;",
                new { Cod_Factura, Cod_Proveedor });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener asientos de factura.", result.Code.GetValueOrDefault(-1), new List<AsientoFactura>());
            }

            foreach (AsientoFactura af in result.Result ?? new List<AsientoFactura>())
            {
                af.DataKey = af.Cod_Factura + '-' + af.Cod_Proveedor + '-' + af.Cod_Cuenta;
            }

            return DbHelper.CreateOkResponse(result.Result ?? new List<AsientoFactura>());
        }

        /// <summary>
        /// Cambia el número de una factura mediante procedimiento almacenado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del cambio de número.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaNumero_Cambiar(int CodEmpresa, FacturaCambioNo data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var procedure = "[spCxP_Factura_Cambio_No]";
                var values = new
                {
                    Proveedor = data.Cod_Proveedor,
                    Factura = data.Cod_Factura,
                    FactNew = data.Cod_FacturaNew,
                    Usuario = data.Usuario,
                };

                var code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return new ErrorDto
                {
                    Code = code,
                    Description = "Ok"
                };
            });

            var respuesta = result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al cambiar número de factura.", result.Code.GetValueOrDefault(-1));

            if (respuesta.Code == 0)
            {
                RegistrarBitacoraFactura(
                    CodEmpresa,
                    data.Usuario,
                    "Cambio Factura: " + data.Cod_Factura + " --> " + data.Cod_FacturaNew + " Prov.Id: " + data.Cod_Proveedor,
                    "APLICA - WEB");
            }

            return respuesta;
        }

        /// <summary>
        /// Actualiza el impuesto de ventas de una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del impuesto a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaImpuesto_Actualizar(int CodEmpresa, FacturaImpuesto data)
        {
            if (string.IsNullOrWhiteSpace(data.Cod_IVA))
                return DbHelper.ErrorResponse("Seleccione un tipo de IVA.");

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_facturas
                  SET IMPUESTO_VENTAS = @Impuesto_Ventas,
                      COD_IVA = @Cod_IVA
                  WHERE cod_proveedor = @Cod_Proveedor
                    AND cod_factura = @Cod_Factura",
                new
                {
                    data.Impuesto_Ventas,
                    data.Cod_IVA,
                    data.Cod_Proveedor,
                    data.Cod_Factura
                });

            if (result.Code == 0)
            {
                RegistrarBitacoraFactura(
                    CodEmpresa,
                    data.Usuario,
                    "CxP-Factura: " + data.Cod_Factura + " ...Prov: " + data.Cod_Proveedor + " IV: " + data.Impuesto_Ventas + " IVA Id: " + data.Cod_IVA,
                    "MODIFICA - WEB");
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar impuesto de factura.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene la información del proveedor asociada a la factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Información del proveedor para factura.</returns>
        public ErrorDto<ProveedorFactura> ProveedorFactura_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var proveedor = connection.QueryFirstOrDefault<ProveedorFactura>(
                    "[spCxP_Proveedor_Load_Factura_Lite]",
                    new { Proveedor = Cod_Proveedor },
                    commandType: CommandType.StoredProcedure);

                if (proveedor is null) return null;

                var cuenta = connection.QueryFirstOrDefault<CuentaProveedor>(
                    @"SELECT COD_CUENTA_MASK, DESCRIPCION
                      FROM vCNTX_CUENTAS_LOCAL
                      WHERE COD_CUENTA = @Cod_Cuenta",
                    new { proveedor.Cod_Cuenta });
                proveedor.Cod_Cuenta_Mask = cuenta?.Cod_Cuenta_Mask ?? proveedor.Cod_Cuenta;
                proveedor.Desc_Cuenta = cuenta?.Descripcion ?? string.Empty;
                return proveedor;
            });

            return CrearRespuestaSingle(
                result,
                "Error al obtener proveedor de factura.",
                "No se encontró el proveedor de la factura.");
        }

        /// <summary>
        /// Obtiene la factura anterior o siguiente según la dirección indicada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código actual de la factura.</param>
        /// <param name="tipo">Dirección del desplazamiento: asc o desc.</param>
        /// <param name="Cod_Proveedor">C&#243;digo que identifica el elemento relacionado.</param>
        /// <returns>Factura encontrada para el desplazamiento.</returns>
        public ErrorDto<FacturaAntSig> ConsultaAscDesc(int CodEmpresa, string Cod_Factura, string tipo, int Cod_Proveedor)
        {
            string query;
            object parametros;

            bool descendente = tipo == "desc";
            bool primeraFactura = Cod_Factura == "0";
            string operador = descendente ? "<" : ">";
            string orden = descendente ? "desc" : "asc";

            query = @"select Top 1 cod_factura, cod_proveedor
                      from cxp_facturas";
            if (Cod_Proveedor > 0)
            {
                query += " where cod_proveedor = @Cod_Proveedor";
                if (!primeraFactura)
                    query += $" and cod_factura {operador} @Cod_Factura";
                parametros = new { Cod_Factura, Cod_Proveedor };
            }
            else if (!primeraFactura)
            {
                query += $@"
                          where cod_factura {operador} @Cod_Factura
                             or (cod_factura = @Cod_Factura and cod_proveedor {operador} @Cod_Proveedor)";
                parametros = new { Cod_Factura, Cod_Proveedor };
            }
            else
            {
                parametros = new { };
            }
            query += $@"
                      order by cod_factura {orden}, cod_proveedor {orden}";

            var result = DbHelper.ExecuteSingleQuery<FacturaAntSig>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                parametros);

            return CrearRespuestaSingle(
                result,
                "Error al consultar factura anterior o siguiente.",
                "No se encontró una factura para el desplazamiento solicitado.");
        }

        /// <summary>
        /// Anula una factura mediante procedimiento almacenado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la anulación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Factura_Anular(int CodEmpresa, FacturaAnular data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var procedure = "[spCxPFacturaAnula]";
                var values = new
                {
                    Proveedor = data.Cod_Proveedor,
                    Factura = data.Cod_Factura,
                    Usuario = data.Usuario,
                };

                var code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return new ErrorDto
                {
                    Code = code,
                    Description = "Ok"
                };
            });

            var respuesta = result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al anular factura.", result.Code.GetValueOrDefault(-1));

            if (respuesta.Code == 0)
            {
                RegistrarBitacoraFactura(
                    CodEmpresa,
                    data.Usuario,
                    "Cambio Factura: " + data.Cod_Factura + " Prov.Id: " + data.Cod_Proveedor,
                    "APLICA - WEB");
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene el listado paginado de plantillas activas de factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas por página.</param>
        /// <param name="filtro">Filtro opcional por código o descripción.</param>
        /// <returns>Listado paginado de plantillas.</returns>
        public ErrorDto<FacturaPlantillaLista> Plantillas_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = CrearFacturaPlantillaListaVacia();

                var parametros = new DynamicParameters();
                var totalBuilder = new System.Text.StringBuilder("SELECT COUNT(*) from CXP_PLANTILLAS WHERE ACTIVO = 1");
                var detalleBuilder = new System.Text.StringBuilder("SELECT COD_PLANTILLA, DESCRIPCION From CXP_PLANTILLAS WHERE ACTIVO = 1");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" AND (COD_PLANTILLA LIKE @Filtro OR DESCRIPCION LIKE @Filtro)");
                    detalleBuilder.Append(" AND (COD_PLANTILLA LIKE @Filtro OR DESCRIPCION LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" order by COD_PLANTILLA");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Plantillas = connection.Query<FacturaPlantilla>(detalleBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearFacturaPlantillaListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plantillas de factura.", result.Code.GetValueOrDefault(-1), CrearFacturaPlantillaListaVacia());
        }

        /// <summary>
        /// Obtiene los asientos de una plantilla de factura calculando el tipo de cambio y montos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Plantilla">Código de la plantilla.</param>
        /// <param name="fecha">Fecha a usar para el tipo de cambio.</param>
        /// <param name="total">Monto total para prorrateo.</param>
        /// <returns>Listado de asientos calculados.</returns>
        public ErrorDto<List<AsientoFactura>> PlantillaAsientos_Obtener(int CodEmpresa, int Cod_Plantilla, string fecha, decimal total)
        {
            return DbHelper.ExecuteListQuery<AsientoFactura>(
                CreatePortalDb(),
                CodEmpresa,
                @"select P.LINEA,
                         Cta.COD_CUENTA_MASK,
                         Cta.COD_CUENTA,
                         Cta.DESCRIPCION AS Cuenta,
                         P.COD_UNIDAD,
                         P.COD_CENTRO_COSTO,
                         Cta.COD_DIVISA,
                         dbo.fxCntXTipoCambio(P.COD_CONTABILIDAD, Cta.COD_DIVISA, @fecha, 'V') as Tipo_Cambio,
                         @total * P.PORCENTAJE / 100 as Monto,
                         'D' as DebeHaber,
                         @total * P.PORCENTAJE / 100 as Debito,
                         0 as Credito,
                         isnull(D.DESCRIPCION,'') as Divisa_Desc,
                         isnull(U.DESCRIPCION,'') as Unidad_Desc,
                         isnull(C.DESCRIPCION,'') as Centro_Desc
                  from CXP_PLANTILLAS_ASIENTO P
                  inner join CNTX_CUENTAS Cta on P.COD_CONTABILIDAD = Cta.COD_CONTABILIDAD and P.COD_CUENTA = Cta.COD_CUENTA
                  left join CNTX_DIVISAS D on Cta.COD_CONTABILIDAD = D.COD_CONTABILIDAD and Cta.COD_DIVISA = D.COD_DIVISA
                  left join CNTX_UNIDADES U on P.COD_CONTABILIDAD = U.COD_CONTABILIDAD and P.COD_UNIDAD = U.COD_UNIDAD
                  left join CNTX_CENTRO_COSTOS C on P.COD_CONTABILIDAD = C.COD_CONTABILIDAD and P.COD_CENTRO_COSTO = C.COD_CENTRO_COSTO
                  Where COD_PLANTILLA = @Cod_Plantilla
                  order by LINEA;",
                new { Cod_Plantilla, fecha, total });
        }

        /// <summary>
        /// Obtiene las facturas marcadas como plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de facturas plantilla.</returns>
        public ErrorDto<List<Factura>> PlantillaFactura_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<Factura>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT F.cod_factura,
                         F.cod_proveedor,
                         P.descripcion AS Proveedor,
                         F.total as total_factura,
                         F.notas
                  FROM cxp_facturas F
                  INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                  WHERE plantilla = 1;");

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener facturas plantilla.", result.Code.GetValueOrDefault(-1), new List<Factura>());
            }

            AsignarDataKeysFacturas(result.Result ?? new List<Factura>());

            return DbHelper.CreateOkResponse(result.Result ?? new List<Factura>());
        }

        /// <summary>
        /// Obtiene la cuenta contable asociada al proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Cuenta contable del proveedor.</returns>
        public ErrorDto<CuentaProveedor> CuentaProveedor_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<CuentaProveedor>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT C.cod_cuenta,
                         C.descripcion,
                         C.Cod_Cuenta_Mask,
                         P.cod_Divisa AS DivisaProv
                  FROM cxp_proveedores P
                  INNER JOIN Cntx_Cuentas C ON P.cod_cuenta = C.cod_cuenta and C.cod_Contabilidad = 1
                  WHERE P.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener cuenta del proveedor.",
                "No se encontró la cuenta del proveedor.");
        }

        /// <summary>
        /// Obtiene el tipo de cambio de una divisa para una fecha determinada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_Divisa">Código de divisa.</param>
        /// <param name="Fecha">Fecha de consulta.</param>
        /// <returns>Tipo de cambio encontrado.</returns>
        public decimal TipoCambio_Obtener(int CodEmpresa, string cod_Divisa, string Fecha)
        {
            var result = DbHelper.ExecuteSingleQuery<decimal>(
                CreatePortalDb(),
                CodEmpresa,
                "select dbo.fxCntXTipoCambio(1, @cod_Divisa, @Fecha, 'V')",
                0m,
                new { cod_Divisa, Fecha });

            return result.Code == 0 ? result.Result : 0;
        }

        /// <summary>
        /// Elimina los asientos contables de una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaAsientos_Borrar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"DELETE cxp_facturas_detalle
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                CrearParametrosFacturaProveedor(Cod_Factura, Cod_Proveedor));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al borrar asientos de factura.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el saldo por pagar del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Saldo">Saldo local a aplicar.</param>
        /// <param name="Saldo_Divisa">Saldo en divisa real a aplicar.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto SaldoPagarProv_Actualizar(int CodEmpresa, decimal Saldo, decimal Saldo_Divisa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_proveedores
                  SET saldo = ISNULL(saldo,0) + @Saldo,
                      SALDO_DIVISA_REAL = ISNULL(SALDO_DIVISA_REAL,0) + @Saldo_Divisa
                  WHERE cod_proveedor = @Cod_Proveedor",
                new { Saldo, Saldo_Divisa, Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar saldo por pagar del proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un asiento contable de factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del asiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaAsiento_Insertar(int CodEmpresa, AsientoFactura data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT INTO cxp_facturas_detalle(linea, cod_factura, cod_proveedor, cod_contabilidad, cod_cuenta, cod_unidad, cod_centro_costo, cod_divisa,
                                                   debeHaber, tipo_cambio, Monto)
                  values(@Linea, @Cod_Factura, @Cod_Proveedor, @Cod_Contabilidad,
                         @Cod_Cuenta, @Cod_Unidad, @Cod_Centro_Costo, @Cod_Divisa, @Debehaber, @Tipo_Cambio, @Monto)",
                new
                {
                    data.Linea,
                    data.Cod_Factura,
                    data.Cod_Proveedor,
                    data.Cod_Contabilidad,
                    data.Cod_Cuenta,
                    data.Cod_Unidad,
                    data.Cod_Centro_Costo,
                    data.Cod_Divisa,
                    data.Debehaber,
                    data.Tipo_Cambio,
                    data.Monto
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar asiento de factura.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un pago de contado para la factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del pago de contado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PagoContado_Insertar(int CodEmpresa, PagoContado data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT cxp_pagoProv(NPago, Cod_Proveedor, Cod_Factura, Fecha_Vencimiento, Monto, Frecuencia,
                                      Tipo_Transac, User_TrasLada, Fecha_Traslada, Tesoreria, Pago_Tercero, Apl_Cargo_Flotante,
                                      Pago_Anticipado, forma_pago, IMPORTE_DIVISA_REAL, TIPO_CAMBIO, COD_DIVISA)
                  values(@NPago, @Cod_Proveedor, @Cod_Factura, @Fecha_Vencimiento,
                         @Monto, @Frecuencia, @Tipo_Transac, @User_Traslada, @Fecha_Traslada,
                         @Tesoreria, @Pago_Tercero, @Apl_Cargo_Flotante, @Pago_Anticipado, @Forma_Pago, @Importe_Divisa_Real,
                         @Tipo_Cambio, @Cod_Divisa)",
                new
                {
                    data.NPago,
                    data.Cod_Proveedor,
                    data.Cod_Factura,
                    data.Fecha_Vencimiento,
                    data.Monto,
                    data.Frecuencia,
                    data.Tipo_Transac,
                    data.User_Traslada,
                    data.Fecha_Traslada,
                    data.Tesoreria,
                    data.Pago_Tercero,
                    data.Apl_Cargo_Flotante,
                    data.Pago_Anticipado,
                    data.Forma_Pago,
                    data.Importe_Divisa_Real,
                    data.Tipo_Cambio,
                    data.Cod_Divisa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar pago contado.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una nueva factura de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la factura.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Factura_Insertar(int CodEmpresa, FacturaDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT cxp_facturas(estado, cod_factura, cod_proveedor, fecha, total, cxp_estado,
                                      asiento_generado, plantilla, vence, creacion_fecha, creacion_user, notas, cod_forma_Pago,
                                      cod_divisa, tipo_cambio, importe_divisa_real, IMPUESTO_VENTAS)
                  values(@Estado, @Cod_Factura, @Cod_Proveedor, @Fecha,
                         @Total, @Cxp_Estado, @Asiento_Generado, @Plantilla, @Vence,
                         @Creacion_Fecha, @Creacion_User, @Notas, @Cod_Forma_Pago, @Cod_Divisa, @Tipo_Cambio,
                         @Importe_Divisa_Real, @Impuesto_Ventas)",
                new
                {
                    data.Estado,
                    data.Cod_Factura,
                    data.Cod_Proveedor,
                    data.Fecha,
                    data.Total,
                    data.Cxp_Estado,
                    data.Asiento_Generado,
                    data.Plantilla,
                    data.Vence,
                    Creacion_Fecha = DateTime.Now,
                    data.Creacion_User,
                    data.Notas,
                    data.Cod_Forma_Pago,
                    data.Cod_Divisa,
                    data.Tipo_Cambio,
                    data.Importe_Divisa_Real,
                    data.Impuesto_Ventas
                });

            if (result.Code == 0)
            {
                RegistrarBitacoraFactura(
                    CodEmpresa,
                    data.Creacion_User,
                    "CxP Factura: " + data.Cod_Factura + " Prov: " + data.Cod_Proveedor,
                    "REGISTRA - WEB");
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar factura.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra una factura y sus movimientos relacionados dentro de una única transacción.
        /// </summary>
        public ErrorDto FacturaCompleta_Insertar(int CodEmpresa, FacturaGuardarCompleta data)
        {
            FacturaDto factura = data.Factura;
            factura.Cod_Factura = factura.Cod_Factura.Trim();

            ErrorDto? validacion = ValidarFacturaCompleta(data, factura);
            if (validacion is not null)
                return validacion;

            using var connection = CreatePortalDb().CreateConnection(CodEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var proceso = connection.QuerySingle<FacturaProcesoResultado>(
                    "[spCxP_Factura_Add]",
                    new
                    {
                        CodFactura = factura.Cod_Factura,
                        CodProveedor = factura.Cod_Proveedor,
                        factura.Fecha,
                        Total = factura.Importe_Divisa_Real,
                        Tipo = factura.Cod_Forma_Pago,
                        factura.Plantilla,
                        factura.Vence,
                        Usuario = factura.Creacion_User,
                        factura.Notas,
                        CodDivisa = factura.Cod_Divisa,
                        TipoCambio = factura.Tipo_Cambio,
                        ImporteDivisaReal = factura.Total,
                        ImpuestoVentas = factura.Impuesto_Ventas,
                        CodIVA = factura.Cod_IVA,
                        SubTotal = factura.Sub_Total,
                        AplicaCargoFlotante = data.PagoContado?.Apl_Cargo_Flotante ?? 0
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                connection.Execute(
                    @"DELETE FROM cxp_facturas_detalle
                      WHERE cod_factura = @Cod_Factura AND cod_proveedor = @Cod_Proveedor",
                    new { factura.Cod_Factura, factura.Cod_Proveedor },
                    transaction);
                InsertarAsientosFactura(connection, transaction, factura, data.Asientos);
                GuardarDetalleIva(connection, transaction, factura, data);

                transaction.Commit();
                var actualizado = string.Equals(proceso.Accion, "UPDATE", StringComparison.OrdinalIgnoreCase);
                RegistrarBitacoraFactura(
                    CodEmpresa,
                    factura.Creacion_User,
                    "CxP Factura: " + factura.Cod_Factura + " Prov: " + factura.Cod_Proveedor,
                    actualizado ? "MODIFICA - WEB" : "REGISTRA - WEB");
                return DbHelper.OkResponse(actualizado ? "Factura actualizada correctamente." : "Factura creada correctamente.");
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); }
                catch
                {
                    // Ignorado: el rollback puede fallar si la transacción ya fue completada o descartada.
                }
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static ErrorDto? ValidarFacturaCompleta(FacturaGuardarCompleta data, FacturaDto factura)
        {
            var error = ValidarDatosBasicosFactura(data, factura);
            if (error is not null)
                return error;

            error = ValidarDetalleIvaFactura(data, factura);
            if (error is not null)
                return error;

            return ValidarAsientosFactura(data, factura);
        }

        private static ErrorDto? ValidarDatosBasicosFactura(FacturaGuardarCompleta data, FacturaDto factura)
        {
            if (factura.Cod_Proveedor <= 0 || string.IsNullOrWhiteSpace(factura.Cod_Factura) || factura.Total <= 0 || factura.Importe_Divisa_Real <= 0)
                return DbHelper.ErrorResponse("Factura, proveedor y monto son obligatorios.", -1);

            if (string.IsNullOrWhiteSpace(factura.Cod_IVA) || factura.Sub_Total is null || factura.Sub_Total < 0)
                return DbHelper.ErrorResponse("Seleccione un tipo de IVA y especifique el subtotal.", -1);

            return data.Asientos.Count < 2
                ? DbHelper.ErrorResponse("La factura debe contener al menos dos líneas de asiento.", -1)
                : null;
        }

        private static ErrorDto? ValidarDetalleIvaFactura(FacturaGuardarCompleta data, FacturaDto factura)
        {
            if (data.IvaDetallado != true)
                return null;

            var detalle = data.IvaDetalle.Where(item => item.Monto_Base > 0).ToList();
            if (detalle.Count == 0 || detalle.Select(item => item.Cod_IVA).Distinct().Count() != detalle.Count)
                return DbHelper.ErrorResponse("El detalle de IVA debe contener tipos distintos con monto base.", -1);

            return Math.Abs(detalle.Sum(item => item.Monto_Base) - factura.Sub_Total!.Value) >= 0.01m ||
                   Math.Abs(detalle.Sum(item => item.Monto_IVA) - factura.Impuesto_Ventas.GetValueOrDefault()) >= 0.01m
                ? DbHelper.ErrorResponse("El detalle de IVA no coincide con el subtotal y el impuesto.", -1)
                : null;
        }

        private static ErrorDto? ValidarAsientosFactura(FacturaGuardarCompleta data, FacturaDto factura)
        {
            if (data.Asientos.Any(asiento =>
                string.IsNullOrWhiteSpace(asiento.Cod_Cuenta) ||
                string.IsNullOrWhiteSpace(asiento.Cod_Unidad) ||
                (asiento.Debito <= 0 && asiento.Credito <= 0)))
                return DbHelper.ErrorResponse("Los asientos contienen líneas incompletas.", -1);

            decimal totalDebito = data.Asientos.Sum(asiento => asiento.Debito);
            decimal totalCredito = data.Asientos.Sum(asiento => asiento.Credito);
            if (Math.Abs(totalDebito - totalCredito) >= 0.005m)
                return DbHelper.ErrorResponse("El asiento de la factura no está balanceado.", -1);

            decimal montoPrimeraLinea = data.Asientos[0].Debito + data.Asientos[0].Credito;
            return Math.Abs(montoPrimeraLinea - factura.Importe_Divisa_Real.GetValueOrDefault()) >= 0.005m
                ? DbHelper.ErrorResponse("El monto de la primera línea no corresponde al monto local de la factura.", -1)
                : null;
        }

        private static void GuardarDetalleIva(IDbConnection connection, IDbTransaction transaction, FacturaDto factura, FacturaGuardarCompleta data)
        {
            var lineas = data.IvaDetallado == true
                ? data.IvaDetalle.Where(item => item.Monto_Base > 0).ToList()
                : new List<FacturaIvaDetalle>
                {
                    new()
                    {
                        Cod_IVA = factura.Cod_IVA,
                        Monto_Base = factura.Sub_Total.GetValueOrDefault(),
                        Monto_IVA = factura.Impuesto_Ventas.GetValueOrDefault(),
                        Porcentaje = 0
                    }
                };

            for (var index = 0; index < lineas.Count; index++)
            {
                var linea = lineas[index];
                var tipo = connection.QueryFirstOrDefault<TipoIvaFactura>(
                    "[spSys_IVA_TIPOS_Consulta]",
                    new { TipoIVA = linea.Cod_IVA },
                    transaction,
                    commandType: CommandType.StoredProcedure);
                if (tipo is null)
                    throw new InvalidOperationException("No se encontró el tipo de IVA " + linea.Cod_IVA + ".");

                connection.Execute(
                    "[spCXP_Factura_IVA_Add]",
                    new
                    {
                        Proveedor = factura.Cod_Proveedor,
                        Factura = factura.Cod_Factura,
                        CodigoIVA = linea.Cod_IVA,
                        MontoBase = linea.Monto_Base,
                        Porcentaje = tipo.Porcentaje,
                        Impuesto = linea.Monto_IVA,
                        Cuenta = tipo.Cod_Cuenta,
                        Usuario = factura.Creacion_User,
                        Inicializa = index == 0 ? 1 : 0
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        private static void InsertarAsientosFactura(
            IDbConnection connection,
            IDbTransaction transaction,
            FacturaDto factura,
            List<AsientoFactura> asientos)
        {
            for (int index = 0; index < asientos.Count; index++)
            {
                AsientoFactura asiento = asientos[index];
                int contabilidad = asiento.Cod_Contabilidad <= 0 ? 1 : asiento.Cod_Contabilidad;
                string? divisaCuenta = connection.QueryFirstOrDefault<string>(
                    @"SELECT RTRIM(cod_divisa)
                      FROM CntX_Cuentas
                      WHERE cod_cuenta = @Cod_Cuenta
                        AND cod_contabilidad = @Cod_Contabilidad",
                    new { asiento.Cod_Cuenta, Cod_Contabilidad = contabilidad },
                    transaction);
                if (string.IsNullOrWhiteSpace(divisaCuenta))
                    throw new InvalidOperationException(
                        $"No se encontró la divisa de la cuenta {asiento.Cod_Cuenta} en el catálogo contable.");

                decimal monto = asiento.Debehaber == "H" ? asiento.Credito : asiento.Debito;
                connection.Execute(
                    @"INSERT INTO cxp_facturas_detalle(linea, cod_factura, cod_proveedor, cod_contabilidad,
                                                       cod_cuenta, cod_unidad, cod_centro_costo, cod_divisa,
                                                       debehaber, tipo_cambio, monto)
                      values(@Linea, @Cod_Factura, @Cod_Proveedor, @Cod_Contabilidad, @Cod_Cuenta,
                             @Cod_Unidad, @Cod_Centro_Costo, @Cod_Divisa, @DebeHaber, @Tipo_Cambio, @Monto)",
                    new
                    {
                        Linea = index + 1,
                        factura.Cod_Factura,
                        factura.Cod_Proveedor,
                        Cod_Contabilidad = contabilidad,
                        asiento.Cod_Cuenta,
                        asiento.Cod_Unidad,
                        asiento.Cod_Centro_Costo,
                        Cod_Divisa = divisaCuenta,
                        DebeHaber = asiento.Debehaber,
                        asiento.Tipo_Cambio,
                        Monto = monto
                    },
                    transaction);
            }
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
