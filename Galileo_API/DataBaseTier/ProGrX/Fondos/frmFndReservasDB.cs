using Dapper;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndReservasDb
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private readonly int vModulo = 18;

        private const string SpReservaCuentasConsulta = "spFnd_Reserva_Cuentas_Consulta";
        private const string SpReservaContenidoConsulta = "spFnd_Reserva_Contenido_Consulta";
        private const string SpReservaCortes = "spFnd_Reserva_Cortes";
        private const string SpReservaCuentaSaldo = "spFnd_Reserva_Cuenta_Saldo";
        private const string SpReservaCuentasRegistro = "spFnd_Reserva_Cuentas_Registro";
        private const string SpReservaContenidoRegistro = "spFnd_Reserva_Contenido_Registro";

        private const string SqlReservasLista = @"
                    SELECT COUNT(1)
                    FROM dbo.vFND_RESERVAS
                    WHERE @hasFilter = 0 OR
                    (
                        COD_RESERVA LIKE @filtro OR
                        descripcion LIKE @filtro
                    );

                    SELECT
                        COD_RESERVA AS cod_reserva,
                        descripcion,
                        cta_reserva,
	                    cta_reserva_desc,
                        cta_transitoria,
	                    cta_transitoria_desc,
                        activa,
                        registro_usuario,
                        registro_fecha
                    FROM dbo.vFND_RESERVAS
                    WHERE @hasFilter = 0 OR
                    (
                        COD_RESERVA LIKE @filtro OR
                        descripcion LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN COD_RESERVA END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN COD_RESERVA END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        COD_RESERVA ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlReservasExporta = @"
                    SELECT
                        COD_RESERVA AS cod_reserva,
                        descripcion,
                        cta_reserva,
                        cta_transitoria,
                        activa,
                        registro_usuario,
                        registro_fecha,
                        modifica_usuario,
                        modifica_fecha
                    FROM dbo.vFND_RESERVAS
                    WHERE @hasFilter = 0 OR
                    (
                        COD_RESERVA LIKE @filtro OR
                        descripcion LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN COD_RESERVA END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN COD_RESERVA END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        COD_RESERVA ASC;";

        private const string SqlCatalogoReservas = @"
                    SELECT
                        COD_RESERVA AS item,
                        descripcion
                    FROM dbo.FND_RESERVAS
                    ORDER BY descripcion;";

        private const string SqlCatalogoOperadoras = @"
                    SELECT
                        RTRIM(cod_Operadora) AS item,
                        descripcion
                    FROM dbo.fnd_Operadoras
                    ORDER BY descripcion;";

        private const string SqlExisteReserva = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.FND_RESERVAS
                    WHERE COD_RESERVA = @CodReserva;";

        private const string SqlInsertReserva = @"
                    INSERT INTO dbo.FND_RESERVAS
                    (
                        COD_RESERVA,
                        descripcion,
                        cod_cuenta,
                        cod_cuenta_Tra,
                        activa
                    )
                    VALUES
                    (
                        @CodReserva,
                        @Descripcion,
                        @CodCuenta,
                        @CodCuentaTra,
                        @Activa
                    );";

        private const string SqlUpdateReserva = @"
                    UPDATE dbo.FND_RESERVAS
                    SET descripcion = @Descripcion,
                        cod_cuenta = @CodCuenta,
                        cod_cuenta_tra = @CodCuentaTra,
                        activa = @Activa
                    WHERE COD_RESERVA = @CodReserva;";

        private const string SqlDeleteReservaCuentas = @"
                    DELETE FROM dbo.FND_RESERVAS_CTAS
                    WHERE COD_RESERVA = @Reserva;";

        private const string SqlDeleteReserva = @"
                    DELETE FROM dbo.FND_RESERVAS
                    WHERE COD_RESERVA = @Reserva;";

        private static readonly IReadOnlyDictionary<string, int> ReservasSortMap = new Dictionary<string, int>
        {
            ["COD_RESERVA"] = 1,
            ["cod_reserva"] = 1,
            ["descripcion"] = 2
        };

        public FrmFndReservasDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtener las reservas de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Exporta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_Reservas_Obtener(int CodEmpresa, bool Exporta, FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndReservasDto>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, ReservasSortMap, "COD_RESERVA");
                var queryResult = Exporta
                    ? ObtenerReservasExportacion(CodEmpresa, spec)
                    : ObtenerReservasPaginadas(CodEmpresa, spec);

                if (queryResult.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        queryResult.Description ?? "Error al consultar reservas de fondos.",
                        queryResult.Code.GetValueOrDefault(-1),
                        new TablasListaGenericaModel { total = 0, lista = new List<FndReservasDto>() });
                }

                result.Result = queryResult.Result ?? new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndReservasDto>()
                };
            }
            catch (Exception ex)
            {
                result = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new TablasListaGenericaModel { total = 0, lista = new List<FndReservasDto>() });
            }

            return result;
        }

        /// <summary>
        /// Obtener los catálogos para las reservas de fondos mediante el TabIndex
        /// 1 para Reservas
        /// 2 para Operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TabIndex"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Reservas_Catalogo_Obtener(int CodEmpresa, int TabIndex)
        {
            var query = TabIndex switch
            {
                0 => SqlCatalogoReservas,
                1 => SqlCatalogoOperadoras,
                _ => string.Empty
            };

            return string.IsNullOrWhiteSpace(query)
                ? DbHelper.CreateErrorResponse("Opción inválida.", -2, new List<DropDownListaGenericaModel>())
                : DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Obtener las cuentas asociadas a una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <returns></returns>
        public ErrorDto<List<FndReservaCuentaDto>> Fnd_Reservas_Cuentas_Obtener(int CodEmpresa, string Reserva)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<FndReservaCuentaDto>(
                    SpReservaCuentasConsulta,
                    new { Reserva = NormalizarTexto(Reserva) },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndReservaCuentaDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndReservaCuentaDto>()
            };
        }

        /// <summary>
        /// Obtener el contenido de una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <returns></returns>
        public ErrorDto<List<FndReservaContenidoDto>> Fnd_Reservas_Contenido_Obtener( int CodEmpresa, string Reserva)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<FndReservaContenidoDto>(
                    SpReservaContenidoConsulta,
                    new { Reserva = NormalizarTexto(Reserva) },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndReservaContenidoDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndReservaContenidoDto>()
            };
        }

        /// <summary>
        /// Obtener los cortes de una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndReservaCorteDto>> Fnd_Reservas_Cortes_Obtener(int CodEmpresa, FndReservaCorteFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de cortes son requeridos.",
                    -2,
                    new List<FndReservaCorteDto>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<FndReservaCorteDto>(
                    SpReservaCortes,
                    CrearParametrosCortes(filtros),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            var cortes = result.Result ?? new List<FndReservaCorteDto>();
            if (result.Code == 0 && string.Equals(NormalizarTexto(filtros.tipo), "R", StringComparison.OrdinalIgnoreCase))
            {
                CalcularPendientes(cortes);
            }

            return new ErrorDto<List<FndReservaCorteDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = cortes
            };
        }

        /// <summary>
        /// Obtener el saldo de una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <param name="FechaInicio"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Fnd_Reservas_Saldo_Obtener(int CodEmpresa, string Reserva, string FechaInicio)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<decimal?>(
                    SpReservaCuentaSaldo,
                    new
                    {
                        Reserva = NormalizarTexto(Reserva),
                        Fecha = NormalizarTexto(FechaInicio)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener saldo contable.", result.Code.GetValueOrDefault(-1), 0m);
            }

            return result.Result.HasValue
                ? DbHelper.CreateOkResponse(result.Result.Value)
                : DbHelper.CreateErrorResponse("No se pudo obtener el saldo contable.", -2, 0m);
        }

        /// <summary>
        /// Guardar una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Reservas_Guardar(int CodEmpresa, FndReservasDto data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos de la reserva son requeridos.", -2);
            }

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteReserva,
                0,
                new { CodReserva = NormalizarTexto(data.cod_reserva) });

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar reserva.", existe.Code.GetValueOrDefault(-1));
            }

            var esNuevo = existe.Result == 0;
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                esNuevo ? SqlInsertReserva : SqlUpdateReserva,
                CrearParametrosReserva(data));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                data.registro_usuario,
                $"Reserva de Fondos: {NormalizarTexto(data.cod_reserva)}",
                esNuevo ? "Registra - WEB" : "Modifica - WEB");

            return result;
        }

        /// <summary>
        /// Eliminar una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Reservas_Eliminar(int CodEmpresa, string Reserva, string Usuario)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var reserva = NormalizarTexto(Reserva);
                connection.Execute(SqlDeleteReservaCuentas, new { Reserva = reserva });
                connection.Execute(SqlDeleteReserva, new { Reserva = reserva });
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar reserva.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                Usuario,
                $"Reserva de Fondos: {NormalizarTexto(Reserva)}",
                "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Registrar las cuentas asociadas a una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <param name="CodCuenta"></param>
        /// <param name="Usuario"></param>
        /// <param name="Accion"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Reservas_Cuentas_Registro(int CodEmpresa, string Reserva, string CodCuenta, string Usuario, string Accion)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpReservaCuentasRegistro,
                    new
                    {
                        Reserva = NormalizarTexto(Reserva),
                        Cuenta = NormalizarCuenta(CodCuenta),
                        Usuario = NormalizarTexto(Usuario).ToUpperInvariant(),
                        Accion = NormalizarTexto(Accion)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar cuenta de reserva.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registrar movimientos en el contenido de una reserva de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Reserva"></param>
        /// <param name="Usuario"></param>
        /// <param name="Accion"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Reservas_Mov_Registro(int CodEmpresa, string Reserva, string Usuario, int Accion, FndReservaContenidoDto filtros)
        {
            if (filtros is null)
            {
                return DbHelper.ErrorResponse("Los datos del movimiento son requeridos.", -2);
            }

            var accion = Accion == 0 ? "A" : "E";
            var movimiento = Accion == 0 ? "Registra - WEB" : "Elimina - WEB";

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpReservaContenidoRegistro,
                    CrearParametrosMovimiento(Reserva, Usuario, accion, filtros),
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al registrar movimiento de reserva.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                Usuario,
                $"Reserva de Fondos, Linea: {filtros.linea_id}, Reserva: {NormalizarTexto(Reserva)}, Plan: {NormalizarTexto(filtros.cod_plan)}, Porcentaje: {filtros.porcentaje}",
                movimiento);

            return DbHelper.OkResponse("Ok");
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerReservasPaginadas(int codEmpresa, LazyLoadSpec spec)
        {
            return DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
            {
                using var multi = connection.QueryMultiple(SqlReservasLista, spec.Params);
                return new TablasListaGenericaModel
                {
                    total = multi.ReadFirstOrDefault<int>(),
                    lista = multi.Read<FndReservasDto>().ToList()
                };
            });
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerReservasExportacion(int codEmpresa, LazyLoadSpec spec)
        {
            return DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
            {
                var lista = connection.Query<FndReservasDto>(SqlReservasExporta, spec.Params).ToList();
                return new TablasListaGenericaModel
                {
                    total = lista.Count,
                    lista = lista
                };
            });
        }

        private static object CrearParametrosCortes(FndReservaCorteFiltros filtros)
        {
            return new
            {
                Reserva = NormalizarTexto(filtros.cod_reserva),
                Inicio = filtros.fecha_inicio,
                Corte = filtros.fecha_corte,
                Tipo = NormalizarTexto(filtros.tipo)
            };
        }

        private static void CalcularPendientes(List<FndReservaCorteDto> cortes)
        {
            foreach (var item in cortes)
            {
                item.pendiente = item.saldo_contable - item.monto_reserva;
            }
        }

        private static object CrearParametrosReserva(FndReservasDto data)
        {
            return new
            {
                CodReserva = NormalizarTexto(data.cod_reserva),
                Descripcion = NormalizarTexto(data.descripcion),
                CodCuenta = NormalizarCuenta(data.cta_reserva),
                CodCuentaTra = NormalizarCuenta(data.cta_transitoria),
                Activa = data.activa ? 1 : 0
            };
        }

        private static object CrearParametrosMovimiento(string reserva, string usuario, string accion, FndReservaContenidoDto filtros)
        {
            return new
            {
                Reserva = NormalizarTexto(reserva),
                Linea = filtros.linea_id,
                ChkPatrimonio = filtros.patrimonio ? 1 : 0,
                Operadora = filtros.cod_operadora,
                Plan = NormalizarTexto(filtros.cod_plan),
                Porcentaje = filtros.porcentaje,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                Accion = NormalizarTexto(accion)
            };
        }

        private void RegistrarBitacora(int codEmpresa, string? usuario, string detalleMovimiento, string movimiento)
        {
            _mSecurity.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarCuenta(string? valor) => NormalizarTexto(valor).Replace("-", string.Empty);

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}
