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
        /// <returns>Listado paginado de proveedores.</returns>
        public ErrorDto<ProveedoresPagosLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? filtroQ)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = new ProveedoresPagosLista
                {
                    Total = 0,
                    Proveedores = new List<ProveedorPagos>()
                };

                var filtroAdicional = string.IsNullOrWhiteSpace(filtroQ) ? string.Empty : filtroQ;
                var filtroTexto = string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();
                var offset = pagina.GetValueOrDefault();
                var fetch = paginacion.GetValueOrDefault();

                var totalQuery = string.IsNullOrWhiteSpace(filtroAdicional)
                    ? "SELECT COUNT(*) FROM CXP_PROVEEDORES"
                    : "SELECT COUNT(*) FROM CXP_PROVEEDORES P inner join CntX_Divisas D ON P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1 " + filtroAdicional;

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery);

                var whereFiltro = string.IsNullOrWhiteSpace(filtroTexto)
                    ? string.Empty
                    : " AND (P.COD_PROVEEDOR LIKE @Filtro OR P.DESCRIPCION LIKE @Filtro) ";

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
                               inner join CntX_Divisas D ON P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1 "
                            + filtroAdicional
                            + whereFiltro
                            + @" ORDER BY COD_PROVEEDOR "
                            + paginaSql;

                respuesta.Proveedores = connection.Query<ProveedorPagos>(
                    query,
                    new
                    {
                        Filtro = filtroTexto is null ? null : $"%{filtroTexto}%",
                        Offset = offset,
                        Fetch = fetch
                    }).ToList();

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new ProveedoresPagosLista { Total = 0, Proveedores = new List<ProveedorPagos>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener proveedores.", result.Code.GetValueOrDefault(-1), new ProveedoresPagosLista { Total = 0, Proveedores = new List<ProveedorPagos>() });
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

            foreach (var item in result.Result ?? new List<FacturaPendientePago>())
            {
                item.Datakey = item.Npago + "-" + item.Cod_Proveedor + "-" + item.Cod_Factura;
            }

            return DbHelper.CreateOkResponse(result.Result ?? new List<FacturaPendientePago>());
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
                         p.ultimo_pago,
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
        public ErrorDto<ProveedorPagos> ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string Vence, string tipo)
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
                              inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                              where P.cod_proveedor in(
                                    select cod_proveedor
                                    from cxp_PagoProv
                                    where tesoreria Is Null
                                      and fecha_vencimiento <= @VenceFin)
                              order by cod_proveedor desc";
                    parametros = new { VenceFin = Vence + " 23:59:59" };
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
                              inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                              where P.cod_proveedor in(
                                    select cod_proveedor
                                    from cxp_PagoProv
                                    where tesoreria Is Null
                                      and fecha_vencimiento <= @VenceFin
                                      and cod_proveedor < @Cod_Proveedor
                                    group by cod_proveedor)
                              order by cod_proveedor desc";
                    parametros = new { VenceFin = Vence + " 23:59:59", Cod_Proveedor };
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
                          inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa and D.cod_contabilidad = 1
                          where P.cod_proveedor in(
                                select cod_proveedor
                                from cxp_PagoProv
                                where tesoreria Is Null
                                  and fecha_vencimiento <= @VenceFin
                                  and cod_proveedor > @Cod_Proveedor
                                group by cod_proveedor)
                          order by cod_proveedor asc";
                parametros = new { VenceFin = Vence + " 23:59:59", Cod_Proveedor };
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

        public ErrorDto Detalle_Insertar(int CodEmpresa, TesTransAsiento data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"INSERT Tes_Trans_Asiento(nsolicitud,cuenta_contable,monto,debehaber,linea,cod_unidad,cod_cc,cod_divisa,tipo_cambio) 
                        VALUES({data.NSolicitud},'{data.Cuenta_Contable}',{data.Monto}
                       ,'{data.DebeHaber}',{data.Linea},'{data.Cod_Unidad}','{data.Cod_Cc}','{data.Cod_Divisa}',{data.Tipo_Cambio})";

                resp.Code = connection.Query<int>(query).FirstOrDefault();
                resp.Description = "Registro agregado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<Anticipo> MontoAnticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Anticipo>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT isnull(Sum(Pc.monto),0) AS 'Cargos'
                                FROM CXP_CARGOSPER Cp INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR  AND  Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                Where Cp.COD_PROVEEDOR = {Cod_Proveedor}
                                AND Pf.user_traslada = 'xBITxTesx' ";
                response.Result = connection.Query<Anticipo>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto Tesoreria_Insertar(int CodEmpresa, TesTransacciones data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"INSERT Tes_Transacciones(id_banco,tipo,codigo,beneficiario,monto,fecha_solicitud,estado,estadoi
                                ,modulo,submodulo,cta_ahorros,detalle1,detalle2,referencia,op,genera,actualiza,cod_unidad
                                ,cod_concepto,user_solicita,autoriza,fecha_autorizacion,user_autoriza,TIPO_BENEFICIARIO,tipo_cambio,cod_divisa)  
                                VALUES({data.Id_Banco},'{data.Tipo}','{data.Codigo}','{data.Beneficiario}',{data.Monto},'{data.Fecha_Solicitud}','{data.Estado}',
                                '{data.Estadoi}','{data.Modulo}','{data.Submodulo}','{data.Cta_Ahorros}'),'{data.Detalle1}','{data.Detalle2}',{data.Referencia},
                                {data.Op},'{data.Genera}','{data.Actualiza}','{data.Cod_Unidad}','{data.Cod_Concepto}','{data.User_Solicita}','{data.Autoriza}',
                                '{data.Fecha_Autorizacion}','{data.User_Autoriza}',{data.Tipo_Beneficiario},{data.Tipo_Cambio}";

                resp.Code = connection.Query<int>(query).FirstOrDefault();
                resp.Description = "Registro agregado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<TesTransacciones> Tesoreria_Obtener(int CodEmpresa, int nSolicitud)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesTransacciones>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT * FROM Tes_Transacciones WHERE nsolicitud = {nSolicitud}";
                response.Result = connection.Query<TesTransacciones>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
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
                    Corte = $"{data.Fecha_Vencimiento.ToString("yyyy/MM/dd")} 23:59:59",
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

        public ErrorDto<DesembolsoNetos> DesembolsoNetos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<DesembolsoNetos>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"SELECT CEDJUR, cod_proveedor, SUM(monto - cargos) AS Neto, SUM(Divisa_Real_Neto) AS 'Divisa_Real_Neto'
                                FROM vCXP_Pagos 
                                WHERE cod_Proveedor = {Cod_Proveedor}
                                GROUP BY cod_proveedor, CEDJUR;
                                ";
                response.Result = connection.Query<DesembolsoNetos>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto Indicadores_Actualizar(int CodEmpresa, PagoProvUpdate data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE cxp_pagoprov 
                                    SET tesoreria = {data.Tesoreria},
                                        fecha_traslada = Getdate(),
                                        user_traslada = '{data.User_Traslada}',
                                        pago_tercero = '{(data.IsPagoTerceroChecked ? data.Pago_Tercero : string.Empty)}'
                                    WHERE user_traslada = 'xBITxTesx'
                                    AND cod_proveedor = {data.Cod_Proveedor}";

                resp.Code = connection.Query<int>(query).FirstOrDefault();
                resp.Description = "Registro actualizado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CancelacionCargos_Actualizar(int CodEmpresa, int Cod_Proveedor, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE cxp_pagoprov 
                                    SET tesoreria = 0,
                                        Tipo_Cancelacion = 'C',
                                        Tesoreria_Estado = 'E',
                                        fecha_traslada = Getdate(),
                                        user_traslada = '{Usuario}',
                                        pago_tercero = '',
                                        Tesoreria_Emision = Getdate()
                                    WHERE user_traslada = 'xBITxTesx'
                                    AND cod_proveedor = {Cod_Proveedor}";

                resp.Code = connection.Query<int>(query).FirstOrDefault();
                resp.Description = "Cargos actualizado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
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

        public ErrorDto<List<CargoPer>> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CargoPer>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"SELECT Cr.COD_CARGO, Cr.DESCRIPCION, Cr.COD_CUENTA, Pc.monto, Pc.COD_DIVISA
                                    FROM CXP_CARGOSPER Cp
                                    INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                    INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                    INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                    INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                                    WHERE Cp.COD_PROVEEDOR = {Cod_Proveedor} AND Pf.user_traslada = 'xBITxTesx'";
                response.Result = connection.Query<CargoPer>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<ProveedorInfoEjecucion> ProveedorTesoreria_Obtener(int CodEmpresa, int Cod_Proveedor, int cod_contabilidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ProveedorInfoEjecucion>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = $@"select P.CEDJUR, P.cod_proveedor, P.descripcion, P.cod_cuenta, P.cod_divisa,
                                       D.cod_cuenta as 'CtaDivDifIng', D.cod_cuenta_Gasto as 'CtaDivDifGst',
                                       dbo.fxCntXTipoCambio(1, P.COD_DIVISA, Getdate(), 'V') as 'TipoCambio',
                                       Getdate() as Fecha
                                from  Cxp_Proveedores P
                                inner join CntX_Divisas D on P.cod_divisa = D.cod_divisa
                                and D.cod_contabilidad = {cod_contabilidad}
                                where P.cod_proveedor = {Cod_Proveedor}
                                ";
                response.Result = connection.Query<ProveedorInfoEjecucion>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<Anticipo>> Anticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Anticipo>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"SELECT Cr.COD_CARGO, Cr.DESCRIPCION, Cr.COD_CUENTA, Pc.monto, Pc.COD_DIVISA
                                    FROM CXP_CARGOSPER Cp
                                    INNER JOIN CXP_ANTICIPOS Ca ON Cp.COD_PROVEEDOR = Ca.COD_PROVEEDOR AND Cp.COD_CARGO = Ca.COD_CARGO AND Cp.ID = Ca.ID_CARGO
                                    INNER JOIN cxp_pagoProv Pf ON Pf.COD_PROVEEDOR = Cp.COD_PROVEEDOR
                                    INNER JOIN CXP_PAGOPROVCARGOS Pc ON Pf.COD_PROVEEDOR = Pc.COD_PROVEEDOR AND Pf.COD_FACTURA = Pc.COD_FACTURA AND Pc.NPAGO = Pf.NPAGO AND Pc.ID = Cp.ID
                                    INNER JOIN CXP_CARGOS Cr ON Cp.COD_CARGO = Cr.COD_CARGO
                                    WHERE Cp.COD_PROVEEDOR = {Cod_Proveedor} AND Pf.user_traslada = 'xBITxTesx'";
                response.Result = connection.Query<Anticipo>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
        
        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}