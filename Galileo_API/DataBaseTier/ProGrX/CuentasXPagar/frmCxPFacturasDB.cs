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

            if (result.Code != 0)
            {
                return new ErrorDto<DivisaLocal>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener la divisa local.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<DivisaLocal>
                {
                    Code = -2,
                    Description = "No se encontró la divisa local.",
                    Result = null
                };
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
                var respuesta = new FacturaLista
                {
                    Total = 0,
                    Facturas = new List<Factura>()
                };

                var parametros = new DynamicParameters();
                var totalBuilder = new System.Text.StringBuilder("SELECT COUNT(*) from cxp_facturas");
                var detalleBuilder = new System.Text.StringBuilder("select cod_factura, cod_proveedor, TOTAL AS TOTAL_FACTURA, notas from cxp_facturas");
                var condiciones = new List<string>();

                if (Cod_Proveedor > 0)
                {
                    condiciones.Add("cod_proveedor = @Cod_Proveedor");
                    parametros.Add("Cod_Proveedor", Cod_Proveedor);
                }

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(COD_FACTURA LIKE @Filtro OR NOTAS LIKE @Filtro OR CAST(COD_PROVEEDOR AS varchar(50)) LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                if (condiciones.Count > 0)
                {
                    var where = " WHERE " + string.Join(" AND ", condiciones);
                    totalBuilder.Append(where);
                    detalleBuilder.Append(where);
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" order by cod_factura");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Facturas = connection.Query<Factura>(detalleBuilder.ToString(), parametros).ToList();
                foreach (Factura ft in respuesta.Facturas)
                {
                    ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                }

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new FacturaLista { Total = 0, Facturas = new List<Factura>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener facturas.", result.Code.GetValueOrDefault(-1), new FacturaLista { Total = 0, Facturas = new List<Factura>() });
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
            var query = @"SELECT F.*, P.descripcion AS Proveedor, P.cod_Divisa AS DivisaProv,
                                  dbo.fxCxPSaldoFacturaCorte(F.cod_Proveedor, F.cod_Factura, Getdate()) AS Saldo,
                                  RTRIM(D.descripcion) AS DivisaFactura
                           FROM cxp_facturas F
                           INNER JOIN cxp_proveedores P ON F.cod_proveedor = P.cod_proveedor
                           INNER JOIN CntX_Divisas D ON D.cod_contabilidad = 1 AND D.cod_divisa = F.cod_divisa
                           WHERE F.cod_factura = @Cod_Factura";

            object parametros;
            if (Cod_Proveedor > 0)
            {
                query += " AND F.cod_proveedor = @Cod_Proveedor";
                parametros = new { Cod_Factura, Cod_Proveedor };
            }
            else
            {
                parametros = new { Cod_Factura };
            }

            var result = DbHelper.ExecuteSingleQuery<FacturaDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                parametros);

            if (result.Code != 0)
            {
                return new ErrorDto<FacturaDto>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle de la factura.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<FacturaDto>
                {
                    Code = -2,
                    Description = "No se encontró la factura.",
                    Result = null
                };
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
                @"SELECT C.cod_Cuenta_Mask,
                         C.cod_cuenta,
                         C.descripcion as Cuenta,
                         D.debehaber,
                         D.monto,
                         D.cod_unidad,
                         U.descripcion as Unidad,
                         D.cod_centro_costo,
                         X.descripcion as CentroCosto,
                         D.cod_proveedor,
                         D.cod_factura,
                         Div.Cod_Divisa,
                         Div.Descripcion as Divisa,
                         D.Tipo_Cambio
                  FROM CXP_FACTURAS_DETALLE D
                  INNER JOIN CXP_FACTURAS Ch ON D.cod_factura = Ch.cod_factura AND D.cod_proveedor = Ch.Cod_Proveedor
                  INNER JOIN CntX_Cuentas C ON D.cod_cuenta = C.cod_cuenta AND D.cod_contabilidad = C.cod_Contabilidad
                  INNER JOIN CntX_Divisas Div ON D.cod_divisa = Div.cod_Divisa AND D.cod_contabilidad = Div.cod_Contabilidad
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
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Usuario,
                    DetalleMovimiento = "Cambio Factura: " + data.Cod_Factura + " --> " + data.Cod_FacturaNew + " Prov.Id: " + data.Cod_Proveedor,
                    Movimiento = "APLICA - WEB",
                    Modulo = 30
                });
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
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_facturas
                  SET IMPUESTO_VENTAS = @Impuesto_Ventas
                  WHERE cod_proveedor = @Cod_Proveedor
                    AND cod_factura = @Cod_Factura",
                new
                {
                    data.Impuesto_Ventas,
                    data.Cod_Proveedor,
                    data.Cod_Factura
                });

            if (result.Code == 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Usuario,
                    DetalleMovimiento = "CxP-Factura: " + data.Cod_Factura + " ...Prov: " + data.Cod_Proveedor + " IV: " + data.Impuesto_Ventas,
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 30
                });
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
            var result = DbHelper.ExecuteSingleQuery<ProveedorFactura>(
                CreatePortalDb(),
                CodEmpresa,
                @"select distinct P.cod_proveedor,
                                        P.descripcion,
                                        P.cod_divisa,
                                        c.cod_cuenta,
                                        C.COD_CUENTA_MASK,
                                        c.DESCRIPCION as Desc_Cuenta,
                                        rtrim(D.descripcion) as Divisa_Local
                  from Cxp_Proveedores P
                  inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa
                  inner join CNTX_CUENTAS C on p.COD_CUENTA = c.COD_CUENTA
                  where D.cod_contabilidad = 1
                    and P.cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorFactura>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener proveedor de factura.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorFactura>
                {
                    Code = -2,
                    Description = "No se encontró el proveedor de la factura.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene la factura anterior o siguiente según la dirección indicada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código actual de la factura.</param>
        /// <param name="tipo">Dirección del desplazamiento: asc o desc.</param>
        /// <returns>Factura encontrada para el desplazamiento.</returns>
        public ErrorDto<FacturaAntSig> ConsultaAscDesc(int CodEmpresa, string Cod_Factura, string tipo)
        {
            string query;
            object parametros;

            if (tipo == "desc")
            {
                if (Cod_Factura == "0")
                {
                    query = @"select Top 1 cod_factura
                              from cxp_facturas
                              order by cod_factura desc";
                    parametros = new { };
                }
                else
                {
                    query = @"select Top 1 cod_factura
                              from cxp_facturas
                              where cod_factura < @Cod_Factura
                              order by cod_factura desc";
                    parametros = new { Cod_Factura };
                }
            }
            else
            {
                query = @"select Top 1 cod_factura
                          from cxp_facturas
                          where cod_factura > @Cod_Factura
                          order by cod_factura asc";
                parametros = new { Cod_Factura };
            }

            var result = DbHelper.ExecuteSingleQuery<FacturaAntSig>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                parametros);

            if (result.Code != 0)
            {
                return new ErrorDto<FacturaAntSig>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al consultar factura anterior o siguiente.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<FacturaAntSig>
                {
                    Code = -2,
                    Description = "No se encontró una factura para el desplazamiento solicitado.",
                    Result = null
                };
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
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Usuario,
                    DetalleMovimiento = "Cambio Factura: " + data.Cod_Factura + " Prov.Id: " + data.Cod_Proveedor,
                    Movimiento = "APLICA - WEB",
                    Modulo = 30
                });
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
                var respuesta = new FacturaPlantillaLista
                {
                    Total = 0,
                    Plantillas = new List<FacturaPlantilla>()
                };

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
                ? DbHelper.CreateOkResponse(result.Result ?? new FacturaPlantillaLista { Total = 0, Plantillas = new List<FacturaPlantilla>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plantillas de factura.", result.Code.GetValueOrDefault(-1), new FacturaPlantillaLista { Total = 0, Plantillas = new List<FacturaPlantilla>() });
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
                @"select Cta.COD_CUENTA_MASK,
                         Cta.DESCRIPCION,
                         P.COD_UNIDAD,
                         P.COD_CENTRO_COSTO,
                         Cta.COD_DIVISA,
                         dbo.fxCntXTipoCambio(P.COD_CONTABILIDAD, Cta.COD_DIVISA, @fecha, 'V') as Tipo_Cambio,
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

            foreach (Factura ft in result.Result ?? new List<Factura>())
            {
                ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
            }

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

            if (result.Code != 0)
            {
                return new ErrorDto<CuentaProveedor>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener cuenta del proveedor.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<CuentaProveedor>
                {
                    Code = -2,
                    Description = "No se encontró la cuenta del proveedor.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el tipo de cambio de una divisa para una fecha determinada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_Divisa">Código de divisa.</param>
        /// <param name="Fecha">Fecha de consulta.</param>
        /// <returns>Tipo de cambio encontrado.</returns>
        public int TipoCambio_Obtener(int CodEmpresa, string cod_Divisa, string Fecha)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "select dbo.fxCntXTipoCambio(1, @cod_Divisa, @Fecha, 'V')",
                0,
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
                new { Cod_Factura, Cod_Proveedor });

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
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Creacion_User,
                    DetalleMovimiento = "CxP Factura: " + data.Cod_Factura + " Prov: " + data.Cod_Proveedor,
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 30
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar factura.", result.Code.GetValueOrDefault(-1));
        }
        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}