using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPProvCargoPerDB
    {

        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPProvCargoPerDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPProvCargoPerDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            DBBitacora = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene las últimas secuencias de cargos periódicos de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de secuencias registradas.</returns>
        public ErrorDto<List<Secuencia>> Secuencias_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<Secuencia>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT TOP 200 [ID], registro_fecha, registro_usuario, saldo
                  FROM cxp_cargosPer
                  WHERE COD_PROVEEDOR = @Cod_Proveedor
                  ORDER BY [id] DESC",
                new { Cod_Proveedor });
        }

        /// <summary>
        /// Obtiene el catálogo de cargos disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cargos.</returns>
        public ErrorDto<List<Cargo>> Cargos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<Cargo>(
                CreatePortalDb(),
                CodEmpresa,
                "Select trim(cod_cargo) as cod_cargo, descripcion from cxp_cargos");
        }

        /// <summary>
        /// Obtiene el detalle de un cargo periódico específico.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Id">Identificador del cargo periódico.</param>
        /// <returns>Detalle del cargo periódico.</returns>
        public ErrorDto<CargoPerDto> CargoDetalle_Obtener(int CodEmpresa, int Cod_Proveedor, int Id)
        {
            var result = DbHelper.ExecuteSingleQuery<CargoPerDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select C.*, isnull(C.Fecha_Cobro_Cargo, C.registro_Fecha) as FechaInicioCobro,
                         P.descripcion as Proveedor,
                         D.descripcion as Cargo_Desc
                  from cxp_proveedores P
                  inner join cxp_cargosper C on P.cod_proveedor = C.cod_proveedor
                  inner join cxp_cargos D on C.cod_cargo = D.cod_cargo
                  where C.ID = @Id and C.cod_proveedor = @Cod_Proveedor",
                null,
                new { Id, Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<CargoPerDto>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle del cargo periódico.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<CargoPerDto>
                {
                    Code = -2,
                    Description = "No se encontró el cargo periódico.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el detalle general del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Información del proveedor.</returns>
        public ErrorDto<ProveedorInfo> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<ProveedorInfo>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Top 1 cod_proveedor,
                         descripcion,
                         cod_divisa,
                         saldo,
                         dbo.fxCntXTipoCambio(1, COD_DIVISA, Getdate(), 'V') as Tipo_Cambio
                  from cxp_proveedores
                  where cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<ProveedorInfo>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle del proveedor.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<ProveedorInfo>
                {
                    Code = -2,
                    Description = "No se encontró el proveedor.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el listado paginado de cargos periódicos de un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por cargo, descripción, concepto o secuencia.</param>
        /// <returns>Listado paginado de cargos periódicos.</returns>
        public ErrorDto<CargoPerDtoList> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new CargoPerDtoList
                {
                    Total = 0,
                    Cargoper = new List<CargoPerDto>()
                };

                var parametros = new DynamicParameters();
                parametros.Add("Cod_Proveedor", Cod_Proveedor);

                var totalBuilder = new StringBuilder(@"SELECT COUNT(C.cod_proveedor)
                                                      FROM cxp_cargosper C
                                                      INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
                                                      WHERE C.cod_proveedor = @Cod_Proveedor");

                var detalleBuilder = new StringBuilder(@"SELECT C.*, D.descripcion AS Cargo_Desc
                                                        FROM cxp_cargosper C
                                                        INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
                                                        WHERE C.cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" AND (C.cod_Cargo LIKE @Filtro OR D.descripcion LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");
                    detalleBuilder.Append(" AND (C.cod_Cargo LIKE @Filtro OR D.descripcion LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" ORDER BY C.ID desc");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Cargoper = connection.Query<CargoPerDto>(detalleBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CargoPerDtoList { Total = 0, Cargoper = new List<CargoPerDto>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener cargos periódicos.", result.Code.GetValueOrDefault(-1), new CargoPerDtoList { Total = 0, Cargoper = new List<CargoPerDto>() });
        }

        /// <summary>
        /// Obtiene el listado paginado de pagos aplicados a un cargo periódico.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Id">Identificador del cargo periódico.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por cargo, secuencia, concepto o id.</param>
        /// <returns>Listado paginado de pagos aplicados.</returns>
        public ErrorDto<PagoProvCargosDtoList> Pagos_Obtener(int CodEmpresa, int Cod_Proveedor, int Id, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new PagoProvCargosDtoList
                {
                    Total = 0,
                    Pagos = new List<PagoProvCargosDto>()
                };

                var parametros = new DynamicParameters();
                parametros.Add("Id", Id);
                parametros.Add("Cod_Proveedor", Cod_Proveedor);

                var totalBuilder = new StringBuilder(@"SELECT COUNT(C.IDX_CONSEC)
                                                      FROM cxp_pagoprov P
                                                      INNER JOIN cxp_pagoprovcargos C
                                                         ON P.npago = C.npago
                                                        AND P.cod_proveedor = C.cod_proveedor
                                                        AND P.cod_factura = C.cod_factura
                                                        AND P.tesoreria IS NOT NULL
                                                      WHERE C.id = @Id AND C.cod_proveedor = @Cod_Proveedor");

                var detalleBuilder = new StringBuilder(@"SELECT C.*, P.fecha_Traslada, P.tesoreria
                                                        FROM cxp_pagoprov P
                                                        INNER JOIN cxp_pagoprovcargos C
                                                           ON P.npago = C.npago
                                                          AND P.cod_proveedor = C.cod_proveedor
                                                          AND P.cod_factura = C.cod_factura
                                                          AND P.tesoreria IS NOT NULL
                                                        WHERE C.id = @Id AND C.cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" AND (C.cod_Cargo LIKE @Filtro OR CAST(C.IDX_CONSEC AS varchar(50)) LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");
                    detalleBuilder.Append(" AND (C.cod_Cargo LIKE @Filtro OR CAST(C.IDX_CONSEC AS varchar(50)) LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);

                detalleBuilder.Append(" ORDER BY C.id desc");
                if (pagina.HasValue && paginacion.HasValue)
                {
                    detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Pagos = connection.Query<PagoProvCargosDto>(detalleBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new PagoProvCargosDtoList { Total = 0, Pagos = new List<PagoProvCargosDto>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener pagos del cargo periódico.", result.Code.GetValueOrDefault(-1), new PagoProvCargosDtoList { Total = 0, Pagos = new List<PagoProvCargosDto>() });
        }

        /// <summary>
        /// Actualiza la información editable de un cargo periódico.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del cargo a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Cargo_Actualizar(int CodEmpresa, CargoPerDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_cargosper
                  SET detalle = @Detalle,
                      concepto = @Concepto,
                      Fecha_Cobro_Cargo = @FechaInicioCobro,
                      Vence = @Vence
                  WHERE id = @Id AND cod_proveedor = @Cod_Proveedor",
                new
                {
                    data.Detalle,
                    data.Concepto,
                    FechaInicioCobro = data.FechaInicioCobro,
                    data.Vence,
                    data.Id,
                    data.Cod_Proveedor
                });

            if (result.Code == 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Usuario_Sesion,
                    DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id,
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 30
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar cargo periódico.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo cargo periódico y actualiza el saldo del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del cargo a insertar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Cargo_Insertar(int CodEmpresa, CargoPerDto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var siguiente = connection.QueryFirstOrDefault<int>(
                    "SELECT ISNULL(MAX(id),0) + 1 FROM cxp_cargosper WHERE cod_proveedor = @Cod_Proveedor",
                    new { data.Cod_Proveedor });

                connection.Execute(
                    @"INSERT cxp_cargosper(id, cod_proveedor, cod_cargo, tipo, valor, vence, saldo, concepto, detalle, recaudado,
                                           importe_divisa_real, registro_fecha, registro_usuario, cod_divisa, tipo_cambio, fecha_cobro_cargo)
                      VALUES(@Id, @Cod_Proveedor, @Cod_Cargo, @Tipo, @Valor, @Vence, @Saldo, @Concepto, @Detalle, @Recaudado,
                             @Importe_Divisa_Real, @Registro_Fecha, @Registro_Usuario, @Cod_Divisa, @Tipo_Cambio, @Fecha_Cobro_Cargo)",
                    new
                    {
                        Id = siguiente,
                        data.Cod_Proveedor,
                        data.Cod_Cargo,
                        data.Tipo,
                        data.Valor,
                        data.Vence,
                        Saldo = data.Valor,
                        data.Concepto,
                        data.Detalle,
                        Recaudado = 0,
                        Importe_Divisa_Real = data.Valor / data.Tipo_Cambio,
                        Registro_Fecha = DateTime.Now,
                        data.Registro_Usuario,
                        data.Cod_Divisa,
                        data.Tipo_Cambio,
                        Fecha_Cobro_Cargo = data.Fecha_Cobro_Cargo
                    });

                connection.Execute(
                    @"UPDATE cxp_proveedores
                      SET saldo = isnull(saldo, 0) - @Valor,
                          SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) - @Importe_Divisa_Real
                      WHERE cod_proveedor = @Cod_Proveedor",
                    new
                    {
                        data.Cod_Proveedor,
                        data.Valor,
                        Importe_Divisa_Real = data.Valor / data.Tipo_Cambio
                    });

                return siguiente;
            });

            if (result.Code == 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Registro_Usuario,
                    DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + result.Result,
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 30
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo agregado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar cargo periódico.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un cargo periódico no recaudado y revierte el saldo del proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del cargo a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Cargo_Eliminar(int CodEmpresa, CargoPerDto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filas = connection.Execute(
                    @"DELETE cxp_cargosper
                      WHERE cod_proveedor = @Cod_Proveedor
                        AND id = @Id
                        AND recaudado = 0",
                    new
                    {
                        data.Cod_Proveedor,
                        data.Id
                    });

                if (filas > 0)
                {
                    connection.Execute(
                        @"UPDATE cxp_proveedores
                          SET saldo = isnull(saldo, 0) + @Valor,
                              SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) + @Importe_Divisa_Real
                          WHERE cod_proveedor = @Cod_Proveedor",
                        new
                        {
                            data.Cod_Proveedor,
                            data.Valor,
                            Importe_Divisa_Real = data.Valor / data.Tipo_Cambio
                        });
                }

                return filas;
            });

            if (result.Code == 0 && result.Result > 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Usuario_Sesion,
                    DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id + "..Mnt..:" + data.Valor,
                    Movimiento = "ELIMINA - WEB",
                    Modulo = 30
                });
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar cargo periódico.", result.Code.GetValueOrDefault(-1));
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
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}