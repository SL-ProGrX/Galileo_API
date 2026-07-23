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
            CxpAnticiposFiltros filtro = JsonConvert.DeserializeObject<CxpAnticiposFiltros>(filtros) ?? new CxpAnticiposFiltros();
            string cargoPrincipal = filtro.cargoCod.Trim();
            List<CargoAdicionalAnticipoDto> cargosAdicionales = filtro.cargosAdicionales
                .Where(cargo => cargo.monto > 0)
                .Select(cargo => new CargoAdicionalAnticipoDto { codCargo = cargo.codCargo.Trim(), monto = cargo.monto })
                .ToList();

            if (filtro.proveedor <= 0 || string.IsNullOrWhiteSpace(cargoPrincipal) || filtro.monto <= 0)
                return DbHelper.ErrorResponse("Proveedor, cargo y monto son obligatorios.", -1);

            if (cargosAdicionales.Any(cargo => string.IsNullOrWhiteSpace(cargo.codCargo)))
                return DbHelper.ErrorResponse("Todos los cargos adicionales deben tener un código válido.", -1);

            if (cargosAdicionales.Sum(cargo => cargo.monto) > filtro.monto)
                return DbHelper.ErrorResponse("Los cargos adicionales son mayores que el monto del anticipo.", -1);

            using var connection = CreatePortalDb().CreateConnection(CodCliente);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                if (!CargoExiste(connection, transaction, cargoPrincipal))
                    return RollbackError(transaction, $"El cargo '{cargoPrincipal}' no existe.", -1);

                CargoAdicionalAnticipoDto? cargoInvalido = cargosAdicionales
                    .FirstOrDefault(cargo => !CargoExiste(connection, transaction, cargo.codCargo));
                if (cargoInvalido != null)
                    return RollbackError(transaction, $"El cargo adicional '{cargoInvalido.codCargo}' no existe.", -1);

                int consecutivo = connection.ExecuteScalar<int>(
                    @"select isnull(max(IDX), 0) + 1
                      from CXP_ANTICIPOS with (updlock, holdlock)
                      where COD_PROVEEDOR = @Proveedor",
                    new { Proveedor = filtro.proveedor },
                    transaction);

                string anticipoEsperado = $"ANT.{filtro.proveedor}.{consecutivo:00000}";
                int registrosParciales = connection.ExecuteScalar<int>(
                    @"select case when
                         exists(select 1 from CXP_FACTURAS where COD_PROVEEDOR = @Proveedor and COD_FACTURA = @Anticipo)
                         or exists(select 1 from CXP_FACTURAS_DETALLE where COD_PROVEEDOR = @Proveedor and COD_FACTURA = @Anticipo)
                         or exists(select 1 from CXP_PAGOPROV where COD_PROVEEDOR = @Proveedor and COD_FACTURA = @Anticipo)
                       then 1 else 0 end",
                    new { Proveedor = filtro.proveedor, Anticipo = anticipoEsperado },
                    transaction);

                if (registrosParciales > 0)
                    return RollbackError(transaction, $"Existen registros parciales para el anticipo {anticipoEsperado}. Deben revisarse antes de volver a guardar.", -1);

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
                    transaction,
                    commandType: CommandType.StoredProcedure);

                string? anticipoRegistrado = connection.QuerySingleOrDefault<string>(
                    @"select ANTICIPOS
                      from CXP_ANTICIPOS
                      where COD_PROVEEDOR = @Proveedor and IDX = @Consecutivo",
                    new { Proveedor = filtro.proveedor, Consecutivo = consecutivo },
                    transaction);

                if (string.IsNullOrWhiteSpace(anticipoRegistrado))
                    return RollbackError(transaction, "El procedimiento no generó el registro del anticipo.", -1);

                foreach (CargoAdicionalAnticipoDto cargo in cargosAdicionales)
                {
                    connection.Execute(
                        @"insert CXP_PAGOPROVCARGOS
                            (NPAGO, COD_FACTURA, COD_PROVEEDOR, COD_CARGO, MONTO, REGISTRO_FECHA,
                             REGISTRO_USUARIO, COD_DIVISA, TIPO_CAMBIO, TIPO_CARGO, TIPO_PROCESO)
                          values
                            (1, @Anticipo, @Proveedor, @CodCargo, @Monto, dbo.MyGetdate(),
                             @Usuario, 'COL', 1, 'M', 'D')",
                        new
                        {
                            Anticipo = anticipoRegistrado,
                            Proveedor = filtro.proveedor,
                            CodCargo = cargo.codCargo,
                            cargo.monto,
                            Usuario = filtro.usuario
                        },
                        transaction);
                }

                transaction.Commit();
                return DbHelper.OkResponse("Los datos han sido guardados satisfactoriamente!");
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); } catch { }
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static bool CargoExiste(IDbConnection connection, IDbTransaction transaction, string codCargo)
        {
            return connection.ExecuteScalar<int>(
                "select count(1) from CXP_CARGOS where LTRIM(RTRIM(COD_CARGO)) = @CodCargo",
                new { CodCargo = codCargo },
                transaction) > 0;
        }

        private static ErrorDto RollbackError(IDbTransaction transaction, string mensaje, int codigo)
        {
            transaction.Rollback();
            return DbHelper.ErrorResponse(mensaje, codigo);
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
