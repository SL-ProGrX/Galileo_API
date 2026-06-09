using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPProvCargoPerDB
    {

        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        #region Helpers privados

        /// <summary>
        /// Crea una lista vacía para el resultado paginado de cargos periódicos.
        /// </summary>
        /// <returns>Lista vacía inicializada.</returns>
        private static CargoPerDtoList CrearCargoPerDtoListVacia() => new()
        {
            Total = 0,
            Cargoper = new List<CargoPerDto>()
        };

        /// <summary>
        /// Crea una lista vacía para el resultado paginado de pagos asociados a cargos.
        /// </summary>
        /// <returns>Lista vacía inicializada.</returns>
        private static PagoProvCargosDtoList CrearPagoProvCargosDtoListVacia() => new()
        {
            Total = 0,
            Pagos = new List<PagoProvCargosDto>()
        };

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

        /// <summary>
        /// Agrega un filtro LIKE a dos consultas y a la colección de parámetros.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        /// <param name="totalBuilder">Consulta de total.</param>
        /// <param name="detalleBuilder">Consulta de detalle.</param>
        /// <param name="condicionSql">Condición SQL a agregar.</param>
        private static void AgregarFiltroLikeComun(string? filtro, DynamicParameters parametros, StringBuilder totalBuilder, StringBuilder detalleBuilder, string condicionSql)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            totalBuilder.Append(condicionSql);
            detalleBuilder.Append(condicionSql);
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a una consulta y a la colección de parámetros.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="detalleBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, StringBuilder detalleBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            detalleBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        /// <summary>
        /// Crea los parámetros comunes para proveedor e identificador.
        /// </summary>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Id">Identificador del cargo periódico.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosProveedorId(int Cod_Proveedor, int Id) => new
        {
            Cod_Proveedor,
            Id
        };

        /// <summary>
        /// Crea los parámetros comunes para proveedor y filtro de búsqueda paginada.
        /// </summary>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Parámetros Dapper inicializados.</returns>
        private static DynamicParameters CrearParametrosProveedor(int Cod_Proveedor)
        {
            var parametros = new DynamicParameters();
            parametros.Add("Cod_Proveedor", Cod_Proveedor);
            return parametros;
        }

        /// <summary>
        /// Actualiza el saldo del proveedor con el valor y tipo de cambio indicados.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="Valor">Monto base.</param>
        /// <param name="Tipo_Cambio">Tipo de cambio aplicado.</param>
        /// <param name="sumar">Indica si se suma o se resta al saldo.</param>
        private static void ActualizarSaldoProveedor(IDbConnection connection, int Cod_Proveedor, decimal Valor, decimal Tipo_Cambio, bool sumar)
        {
            var parametros = new
            {
                Cod_Proveedor,
                Valor,
                Importe_Divisa_Real = Valor / Tipo_Cambio
            };

            if (sumar)
            {
                connection.Execute(
                    @"UPDATE cxp_proveedores
                       SET saldo = isnull(saldo, 0) + @Valor,
                           SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) + @Importe_Divisa_Real
                       WHERE cod_proveedor = @Cod_Proveedor",
                    parametros);

                return;
            }

            connection.Execute(
                @"UPDATE cxp_proveedores
                   SET saldo = isnull(saldo, 0) - @Valor,
                       SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) - @Importe_Divisa_Real
                   WHERE cod_proveedor = @Cod_Proveedor",
                parametros);
        }

        /// <summary>
        /// Registra la bitácora de cargos periódicos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <param name="detalleMovimiento">Detalle del movimiento.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        private void RegistrarBitacoraCargo(int CodEmpresa, string usuario, string detalleMovimiento, string movimiento)
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
        /// Ejecuta una consulta paginada usando los builders y parámetros indicados.
        /// </summary>
        /// <typeparam name="T">Tipo del detalle a retornar.</typeparam>
        /// <typeparam name="TLista">Tipo del contenedor paginado.</typeparam>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        /// <param name="totalBuilder">Consulta de total.</param>
        /// <param name="detalleBuilder">Consulta de detalle.</param>
        /// <param name="crearListaVacia">Función que crea la respuesta vacía.</param>
        /// <param name="asignarTotal">Acción para asignar el total.</param>
        /// <param name="asignarDetalle">Acción para asignar el detalle.</param>
        /// <returns>Respuesta paginada llena.</returns>
        private static TLista EjecutarConsultaPaginada<T, TLista>(
            IDbConnection connection,
            DynamicParameters parametros,
            StringBuilder totalBuilder,
            StringBuilder detalleBuilder,
            Func<TLista> crearListaVacia,
            Action<TLista, int> asignarTotal,
            Action<TLista, List<T>> asignarDetalle)
        {
            var respuesta = crearListaVacia();
            var total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);
            var detalle = connection.Query<T>(detalleBuilder.ToString(), parametros).ToList();
            asignarTotal(respuesta, total);
            asignarDetalle(respuesta, detalle);
            return respuesta;
        }

        #endregion

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
                CrearParametrosProveedorId(Cod_Proveedor, Id));

            return CrearRespuestaSingle(
                result,
                "Error al obtener detalle del cargo periódico.",
                "No se encontró el cargo periódico.");
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

            return CrearRespuestaSingle(
                result,
                "Error al obtener detalle del proveedor.",
                "No se encontró el proveedor.");
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
                var parametros = CrearParametrosProveedor(Cod_Proveedor);

                var totalBuilder = new StringBuilder(@"SELECT COUNT(C.cod_proveedor)
                                                      FROM cxp_cargosper C
                                                      INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
                                                      WHERE C.cod_proveedor = @Cod_Proveedor");

                var detalleBuilder = new StringBuilder(@"SELECT C.*, D.descripcion AS Cargo_Desc
                                                        FROM cxp_cargosper C
                                                        INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
                                                        WHERE C.cod_proveedor = @Cod_Proveedor");

                AgregarFiltroLikeComun(
                    filtro,
                    parametros,
                    totalBuilder,
                    detalleBuilder,
                    " AND (C.cod_Cargo LIKE @Filtro OR D.descripcion LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");

                detalleBuilder.Append(" ORDER BY C.ID desc");
                AgregarPaginacion(pagina, paginacion, detalleBuilder, parametros);

                return EjecutarConsultaPaginada<CargoPerDto, CargoPerDtoList>(
                    connection,
                    parametros,
                    totalBuilder,
                    detalleBuilder,
                    CrearCargoPerDtoListVacia,
                    (lista, total) => lista.Total = total,
                    (lista, detalle) => lista.Cargoper = detalle);
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearCargoPerDtoListVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener cargos periódicos.", result.Code.GetValueOrDefault(-1), CrearCargoPerDtoListVacia());
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
                var parametros = CrearParametrosProveedor(Cod_Proveedor);
                parametros.Add("Id", Id);

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

                AgregarFiltroLikeComun(
                    filtro,
                    parametros,
                    totalBuilder,
                    detalleBuilder,
                    " AND (C.cod_Cargo LIKE @Filtro OR CAST(C.IDX_CONSEC AS varchar(50)) LIKE @Filtro OR C.concepto LIKE @Filtro OR CAST(C.id AS varchar(50)) LIKE @Filtro)");

                detalleBuilder.Append(" ORDER BY C.id desc");
                AgregarPaginacion(pagina, paginacion, detalleBuilder, parametros);

                return EjecutarConsultaPaginada<PagoProvCargosDto, PagoProvCargosDtoList>(
                    connection,
                    parametros,
                    totalBuilder,
                    detalleBuilder,
                    CrearPagoProvCargosDtoListVacia,
                    (lista, total) => lista.Total = total,
                    (lista, detalle) => lista.Pagos = detalle);
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearPagoProvCargosDtoListVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener pagos del cargo periódico.", result.Code.GetValueOrDefault(-1), CrearPagoProvCargosDtoListVacia());
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
                RegistrarBitacoraCargo(
                    CodEmpresa,
                    data.Usuario_Sesion,
                    "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id,
                    "MODIFICA - WEB");
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

                ActualizarSaldoProveedor(connection, data.Cod_Proveedor, data.Valor, data.Tipo_Cambio, false);

                return siguiente;
            });

            if (result.Code == 0)
            {
                RegistrarBitacoraCargo(
                    CodEmpresa,
                    data.Registro_Usuario,
                    "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + result.Result,
                    "REGISTRA - WEB");
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
                    ActualizarSaldoProveedor(connection, data.Cod_Proveedor, data.Valor, data.Tipo_Cambio, true);
                }

                return filas;
            });

            if (result.Code == 0 && result.Result > 0)
            {
                RegistrarBitacoraCargo(
                    CodEmpresa,
                    data.Usuario_Sesion,
                    "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id + "..Mnt..:" + data.Valor,
                    "ELIMINA - WEB");
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