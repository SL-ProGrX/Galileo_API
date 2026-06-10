using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPCargosAdicionalesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPCargosAdicionalesDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPCargosAdicionalesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de cargos adicionales y formatea la cuenta contable asociada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cargos adicionales.</returns>
        public ErrorDto<List<CargosAdicionalDto>> ObtenerCargosAdicionales(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<CargosAdicionalDto>(
                CreatePortalDb(),
                CodEmpresa,
                "select * from CXP_CARGOS order by COD_CARGO");

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener cargos adicionales.", result.Code.GetValueOrDefault(-1), new List<CargosAdicionalDto>());
            }

            MCntLinkDB obj = new MCntLinkDB(_config);
            foreach (var item in result.Result ?? new List<CargosAdicionalDto>())
            {
                item.Cod_Cuenta = obj.fxgCntCuentaFormato(CodEmpresa, true, item.Cod_Cuenta, 1);
            }

            return DbHelper.CreateOkResponse(result.Result ?? new List<CargosAdicionalDto>());
        }

        /// <summary>
        /// Verifica si existe un cargo adicional por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodCargo">Código del cargo.</param>
        /// <returns>Resultado con la cantidad encontrada en la propiedad <c>Code</c>.</returns>
        public ErrorDto ExisteCargoAdicional(int CodEmpresa, string CodCargo)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "select isnull(count(*),0) as Existe from CXP_CARGOS where COD_CARGO = @CodCargo",
                0,
                new { CodCargo });

            return result.Code == 0
                ? new ErrorDto { Code = result.Result, Description = string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al validar la existencia del cargo adicional.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un cargo adicional según su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodCargo">Código del cargo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto EliminarCargoAdicional(int CodEmpresa, string CodCargo)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "delete from CXP_CARGOS where COD_CARGO = @CodCargo",
                new { CodCargo });

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el cargo adicional.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo cargo adicional.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Info">Información del cargo adicional.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InsertarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "insert into CXP_CARGOS (COD_CARGO, DESCRIPCION, COD_CUENTA, ACTIVO) values (@COD_CARGO, @DESCRIPCION, @COD_CUENTA, @ACTIVO)",
                new
                {
                    COD_CARGO = Info.Cod_Cargo,
                    DESCRIPCION = Info.Descripcion,
                    COD_CUENTA = Info.Cod_Cuenta.Replace("-", ""),
                    ACTIVO = Info.Activo,
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo agregado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el cargo adicional.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza un cargo adicional existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Info">Información actualizada del cargo adicional.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ActualizarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "update CXP_CARGOS set DESCRIPCION = @DESCRIPCION, COD_CUENTA = @COD_CUENTA, ACTIVO = @ACTIVO where COD_CARGO = @COD_CARGO",
                new
                {
                    DESCRIPCION = Info.Descripcion,
                    COD_CUENTA = Info.Cod_Cuenta.Replace("-", ""),
                    ACTIVO = Info.Activo,
                    COD_CARGO = Info.Cod_Cargo,
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Cargo actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el cargo adicional.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}