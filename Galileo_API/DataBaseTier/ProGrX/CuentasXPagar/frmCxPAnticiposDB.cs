using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCxPAnticiposDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPAnticiposDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPAnticiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Ejecuta el registro de un anticipo de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="filtros">Filtros serializados en formato JSON.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ExeAnticipos(int CodCliente, string filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
                return DbHelper.ErrorResponse("Debe indicar los datos del anticipo.", -1);

            CxpAnticiposFiltros filtro;
            try
            {
                filtro = JsonConvert.DeserializeObject<CxpAnticiposFiltros>(filtros)
                    ?? new CxpAnticiposFiltros();
            }
            catch (JsonException)
            {
                return DbHelper.ErrorResponse("Los datos del anticipo no tienen un formato válido.", -1);
            }

            var cargoPrincipal = filtro.cargoCod?.Trim() ?? string.Empty;
            var cargosAdicionales = (filtro.cargosAdicionales ?? new List<CargoAdicionalAnticipoDto>())
                .Where(cargo => cargo is not null && cargo.monto > 0)
                .Select(cargo => new CargoAdicionalAnticipoDto
                {
                    codCargo = cargo.codCargo?.Trim() ?? string.Empty,
                    monto = cargo.monto
                })
                .ToList();

            if (filtro.proveedor <= 0 || string.IsNullOrWhiteSpace(cargoPrincipal) || filtro.monto <= 0)
                return DbHelper.ErrorResponse("Proveedor, cargo y monto son obligatorios.", -1);

            if (cargoPrincipal.Length > 10 || cargosAdicionales.Any(cargo =>
                    string.IsNullOrWhiteSpace(cargo.codCargo) || cargo.codCargo.Length > 10))
                return DbHelper.ErrorResponse("Los códigos de cargo deben tener entre 1 y 10 caracteres.", -1);

            if (filtro.monto > 99999999999999.99m || decimal.Round(filtro.monto, 2) != filtro.monto ||
                cargosAdicionales.Any(cargo => cargo.monto > 99999999999999.99m ||
                    decimal.Round(cargo.monto, 2) != cargo.monto))
                return DbHelper.ErrorResponse("Los montos deben tener como máximo dos decimales.", -1);

            decimal totalCargos;
            try
            {
                totalCargos = cargosAdicionales.Sum(cargo => cargo.monto);
            }
            catch (OverflowException)
            {
                return DbHelper.ErrorResponse("El total de cargos adicionales es demasiado grande.", -1);
            }
            if (totalCargos > filtro.monto)
                return DbHelper.ErrorResponse("Los cargos adicionales son mayores que el monto del anticipo.", -1);

            if (string.IsNullOrWhiteSpace(filtro.usuario) || filtro.usuario.Length > 30 ||
                (filtro.documento?.Length ?? 0) > 30 || (filtro.notas?.Length ?? 0) > 500)
                return DbHelper.ErrorResponse("Usuario, documento o notas inválidos.", -1);

            var spIniciado = false;
            var spFinalizado = false;
            var cargosCompletados = 0;
            string? anticipoEsperado = null;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
                connection.Open();

                if (!CargoExiste(connection, cargoPrincipal))
                    return DbHelper.ErrorResponse($"El cargo '{cargoPrincipal}' no existe.", -1);

                var cargoInvalido = cargosAdicionales
                    .FirstOrDefault(cargo => !CargoExiste(connection, cargo.codCargo));
                if (cargoInvalido is not null)
                    return DbHelper.ErrorResponse($"El cargo adicional '{cargoInvalido.codCargo}' no existe.", -1);

                var consecutivo = connection.ExecuteScalar<int>(
                    @"SELECT ISNULL(MAX(IDX), 0) + 1
                      FROM CXP_ANTICIPOS
                      WHERE COD_PROVEEDOR = @Proveedor;",
                    new { Proveedor = filtro.proveedor });

                anticipoEsperado = $"ANT.{filtro.proveedor}.{consecutivo:00000}";
                var registrosParciales = connection.ExecuteScalar<int>(
                    @"SELECT CASE WHEN
                         EXISTS(SELECT 1 FROM CXP_FACTURAS WHERE COD_PROVEEDOR = @Proveedor AND COD_FACTURA = @Anticipo)
                         OR EXISTS(SELECT 1 FROM CXP_FACTURAS_DETALLE WHERE COD_PROVEEDOR = @Proveedor AND COD_FACTURA = @Anticipo)
                         OR EXISTS(SELECT 1 FROM CXP_PAGOPROV WHERE COD_PROVEEDOR = @Proveedor AND COD_FACTURA = @Anticipo)
                       THEN 1 ELSE 0 END;",
                    new { Proveedor = filtro.proveedor, Anticipo = anticipoEsperado });

                if (registrosParciales > 0)
                    return DbHelper.ErrorResponse(
                        $"Existen registros parciales para el anticipo {anticipoEsperado}. Revise esos registros antes de volver a guardar.", -1);

                spIniciado = true;
                connection.Execute(
                    "spCxP_Anticipos",
                    new
                    {
                        Proveedor = filtro.proveedor,
                        CargoCod = cargoPrincipal,
                        Monto = filtro.monto,
                        Divisa = "COL",
                        Documento = filtro.documento,
                        Notas = filtro.notas,
                        Usuario = filtro.usuario,
                        FechaCargo = filtro.fechaCargo
                    },
                    commandType: CommandType.StoredProcedure);
                spFinalizado = true;

                var anticipoRegistrado = connection.QuerySingleOrDefault<string>(
                    @"SELECT ANTICIPOS
                      FROM CXP_ANTICIPOS
                      WHERE COD_PROVEEDOR = @Proveedor AND IDX = @Consecutivo
                        AND USUARIO = @Usuario AND MONTO = @Monto
                        AND ISNULL(DOCUMENTO, '') = ISNULL(@Documento, '');",
                    new { Proveedor = filtro.proveedor, Consecutivo = consecutivo, Usuario = filtro.usuario, Monto = filtro.monto, Documento = filtro.documento });

                if (string.IsNullOrWhiteSpace(anticipoRegistrado) ||
                    !string.Equals(anticipoRegistrado, anticipoEsperado, StringComparison.OrdinalIgnoreCase))
                    return DbHelper.ErrorResponse(
                        "El procedimiento terminó, pero no se pudo identificar con seguridad el anticipo generado. Revise cargos, factura y pago antes de reintentar.", -1);

                foreach (var cargo in cargosAdicionales)
                {
                    connection.Execute(
                        @"INSERT CXP_PAGOPROVCARGOS
                            (NPAGO, COD_FACTURA, COD_PROVEEDOR, COD_CARGO, MONTO, REGISTRO_FECHA,
                             REGISTRO_USUARIO, COD_DIVISA, TIPO_CAMBIO, TIPO_CARGO, TIPO_PROCESO)
                          VALUES
                            (1, @Anticipo, @Proveedor, @CodCargo, @Monto, dbo.MyGetdate(),
                             @Usuario, 'COL', 1, 'M', 'D');",
                        new
                        {
                            Anticipo = anticipoRegistrado,
                            Proveedor = filtro.proveedor,
                            CodCargo = cargo.codCargo,
                            cargo.monto,
                            Usuario = filtro.usuario
                        });
                    cargosCompletados++;
                }

                return DbHelper.OkResponse("Los datos han sido guardados satisfactoriamente!");
            }
            catch (Exception)
            {
                if (!spIniciado)
                    return DbHelper.ErrorResponse("No se pudo iniciar el registro del anticipo. No se ejecutó el procedimiento.", -1);

                var referencia = anticipoEsperado is null ? string.Empty : $" Referencia prevista: {anticipoEsperado}.";
                var etapa = spFinalizado
                    ? $"El procedimiento terminó y se completaron {cargosCompletados} cargo(s) adicional(es)."
                    : "El procedimiento pudo haber registrado una parte del anticipo.";
                return DbHelper.ErrorResponse(
                    $"No se completó el registro del anticipo.{referencia} {etapa} " +
                    "Revise el cargo flotante, anticipo, factura, detalle y pago antes de reintentar.", -1);
            }
        }

        /// <summary>Comprueba que el cargo exista antes de registrar el anticipo.</summary>
        private static bool CargoExiste(IDbConnection connection, string codCargo)
        {
            return connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM CXP_CARGOS WHERE LTRIM(RTRIM(COD_CARGO)) = @CodCargo;",
                new { CodCargo = codCargo }) > 0;
        }

        /// <summary>
        /// Obtiene el listado de cargos activos para anticipos.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Listado de cargos disponibles.</returns>
        public ErrorDto<List<CargoDto>> ObtenerCargos(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<CargoDto>(
                CreatePortalDb(),
                CodCliente,
                "select COD_CARGO, DESCRIPCION, 0 as MONTO from CXP_CARGOS where ACTIVO = 1");
        }

        /// <summary>
        /// Obtiene los adelantos registrados de un proveedor.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="CodProveedor">Código del proveedor.</param>
        /// <returns>Listado de adelantos registrados.</returns>
        public ErrorDto<List<AdelantoRegistradoDto>> ObtenerAdelantosRegistrados(int CodCliente, int CodProveedor)
        {
            return DbHelper.ExecuteListQuery<AdelantoRegistradoDto>(
                CreatePortalDb(),
                CodCliente,
                @"select A.*,P.tesoreria,P.fecha_vencimiento,C.descripcion as Cargo,
                         dbo.fxCxP_CargoFlotanteSaldoCorte(A.cod_Proveedor,A.ID_Cargo, Getdate()) as Saldo
                  from cxp_anticipos A
                  left join cxp_pagoProv P on A.cod_proveedor = P.cod_proveedor and A.Anticipos = P.cod_factura
                  inner join CxP_Cargos C on A.cod_cargo = C.cod_cargo
                  where A.cod_proveedor = @CodProveedor
                  order by Fecha desc",
                new { CodProveedor });
        }

        /// <summary>
        /// Obtiene el historial de pagos de un anticipo específico.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="CodProveedor">Código del proveedor.</param>
        /// <param name="Anticipos">Identificador del anticipo.</param>
        /// <returns>Listado del historial de pagos.</returns>
        public ErrorDto<List<HistorialPagoDto>> ObtenerHistorialDePagos(int CodCliente, int CodProveedor, string Anticipos)
        {
            return DbHelper.ExecuteListQuery<HistorialPagoDto>(
                CreatePortalDb(),
                CodCliente,
                @"select A.ANTICIPOS, P.COD_PROVEEDOR, P.COD_FACTURA, P.REGISTRO_FECHA, REGISTRO_USUARIO, P.MONTO, P.COD_DIVISA, P.TIPO_CAMBIO, P.NPAGO
                  from CXP_ANTICIPOS A
                  inner join CXP_PAGOPROVCARGOS P on A.ID_CARGO = P.[ID] AND A.COD_PROVEEDOR = P.COD_PROVEEDOR
                  where A.COD_PROVEEDOR = @CodProveedor and A.ANTICIPOS = @Anticipos",
                new
                {
                    CodProveedor,
                    Anticipos
                });
        }

        /// <summary>
        /// Obtiene el listado de proveedores disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de proveedores.</returns>
        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<Proveedor>(
                CreatePortalDb(),
                CodEmpresa,
                "select * from pv_parametros_mod");
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo de anticipo para un proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Proveedor">Código del proveedor.</param>
        /// <returns>Consecutivo disponible en la descripción del resultado.</returns>
        public ErrorDto ConsecutivoAdelanto(int CodEmpresa, int Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "select (isnull(max(IDX),0) + 1) as Consecutivo from cxp_anticipos where cod_proveedor = @Proveedor",
                0,
                new { Proveedor });

            return result.Code == 0
                ? new ErrorDto { Code = result.Result, Description = string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al obtener el consecutivo del anticipo.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

    }//end class
}//end namespace