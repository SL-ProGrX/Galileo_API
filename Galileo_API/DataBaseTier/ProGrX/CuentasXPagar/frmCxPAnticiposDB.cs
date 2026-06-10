using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

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

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "spCxP_Anticipos",
                new
                {
                    Proveedor = filtro.proveedor,
                    CargoCod = filtro.cargoCod,
                    Monto = filtro.monto,
                    Divisa = filtro.divisa,
                    Documento = filtro.documento,
                    Notas = filtro.notas,
                    Usuario = filtro.usuario,
                    FechaCargo = filtro.fechaCargo
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Los datos han sido guardados satisfactoriamente!")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar el anticipo.", result.Code.GetValueOrDefault(-1));
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