using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPPlantillasDB
    {

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPPlantillasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPPlantillasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de plantillas de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de plantillas.</returns>
        public ErrorDto<List<PlantillaDto>> Plantillas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<PlantillaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM CxP_Plantillas");
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
                "SELECT cod_unidad, descripcion FROM CntX_unidades WHERE Activa = 1 and cod_contabilidad = 1");
        }

        /// <summary>
        /// Obtiene los centros de costo asociados a una unidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Unidad">Código de la unidad.</param>
        /// <returns>Listado de centros de costo.</returns>
        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return DbHelper.ExecuteListQuery<CentroCosto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT C.COD_CENTRO_COSTO,
                         C.descripcion
                  FROM CNTX_CENTRO_COSTOS C
                  INNER JOIN CNTX_UNIDADES_CC A ON C.COD_CENTRO_COSTO = A.COD_CENTRO_COSTO
                                               AND C.cod_contabilidad = A.cod_Contabilidad
                  WHERE A.cod_unidad = @Cod_Unidad
                    AND C.cod_contabilidad = 1",
                new { Cod_Unidad });
        }

        /// <summary>
        /// Obtiene el detalle de una plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Plantilla">Código de la plantilla.</param>
        /// <returns>Detalle de la plantilla.</returns>
        public ErrorDto<PlantillaDto> PlantillaDetalle_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            var result = DbHelper.ExecuteSingleQuery<PlantillaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM CxP_Plantillas WHERE cod_plantilla = @Cod_Plantilla",
                null,
                new { Cod_Plantilla });

            if (result.Code != 0)
            {
                return new ErrorDto<PlantillaDto>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener el detalle de la plantilla.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<PlantillaDto>
                {
                    Code = -2,
                    Description = "No se encontró la plantilla.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene la siguiente o anterior plantilla según la dirección indicada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scroll">Dirección del desplazamiento.</param>
        /// <param name="Cod_Plantilla">Código actual de la plantilla.</param>
        /// <returns>Plantilla encontrada según el desplazamiento.</returns>
        public ErrorDto<PlantillaDto> PlantillaDetalle_Scroll(int CodEmpresa, int scroll, string Cod_Plantilla)
        {
            string query = scroll == 1
                ? "SELECT top 1 * FROM CxP_Plantillas WHERE cod_plantilla > @Cod_Plantilla ORDER BY cod_plantilla asc"
                : "SELECT top 1 * FROM CxP_Plantillas WHERE cod_plantilla < @Cod_Plantilla ORDER BY cod_plantilla desc";

            var result = DbHelper.ExecuteSingleQuery<PlantillaDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                new { Cod_Plantilla });

            if (result.Code != 0)
            {
                return new ErrorDto<PlantillaDto>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener desplazamiento de plantilla.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<PlantillaDto>
                {
                    Code = -2,
                    Description = "No se encontró plantilla para el desplazamiento solicitado.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene los asientos contables asociados a una plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Plantilla">Código de la plantilla.</param>
        /// <returns>Listado de asientos de la plantilla.</returns>
        public ErrorDto<List<PlantillaAsientoDto>> PlantillaAsientos_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            return DbHelper.ExecuteListQuery<PlantillaAsientoDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT A.linea,
                         A.cod_cuenta,
                         B.descripcion AS Desc_Cuenta,
                         B.cod_cuenta_mask,
                         A.cod_unidad,
                         A.cod_Centro_Costo,
                         A.porcentaje,
                         A.cod_plantilla,
                         A.cod_divisa
                  FROM CxP_Plantillas_Asiento A
                  INNER JOIN CntX_cuentas B ON A.cod_cuenta = B.cod_cuenta
                                           AND A.cod_contabilidad = B.cod_contabilidad
                  WHERE B.cod_contabilidad = 1
                    AND A.cod_plantilla = @Cod_Plantilla
                  ORDER BY Linea",
                new { Cod_Plantilla });
        }

        /// <summary>
        /// Actualiza una plantilla y elimina sus asientos para permitir su recreación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la plantilla.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Plantilla_Actualizar(int CodEmpresa, PlantillaDto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"UPDATE CxP_Plantillas
                      SET descripcion = @Descripcion,
                          notas = @Notas,
                          activo = @Activo
                      WHERE cod_plantilla = @Cod_Plantilla",
                    new
                    {
                        Descripcion = data.Descripcion,
                        Notas = data.Notas,
                        Activo = Convert.ToInt32(data.Activo),
                        Cod_Plantilla = data.Cod_Plantilla
                    });

                connection.Execute(
                    "DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = @Cod_Plantilla",
                    new { Cod_Plantilla = data.Cod_Plantilla });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla actualizada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una nueva plantilla y limpia asientos previos asociados al mismo código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la plantilla.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Plantilla_Insertar(int CodEmpresa, PlantillaDto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"INSERT INTO CxP_Plantillas(cod_plantilla, descripcion, notas, Registro_Usuario, registro_fecha, activo)
                      VALUES(@Cod_Plantilla, @Descripcion, @Notas, @Registro_Usuario, @Registro_Fecha, @Activo)",
                    new
                    {
                        Cod_Plantilla = data.Cod_Plantilla,
                        Descripcion = data.Descripcion,
                        Notas = data.Notas,
                        Registro_Usuario = data.Registro_Usuario,
                        Registro_Fecha = DateTime.Now,
                        Activo = Convert.ToInt32(data.Activo)
                    });

                connection.Execute(
                    "DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = @Cod_Plantilla",
                    new { Cod_Plantilla = data.Cod_Plantilla });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla agregada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una plantilla y sus asientos asociados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Plantilla">Código de la plantilla.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Plantilla_Borrar(int CodEmpresa, string Cod_Plantilla)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = @Cod_Plantilla",
                    new { Cod_Plantilla });

                connection.Execute(
                    "DELETE CxP_Plantillas WHERE cod_plantilla = @Cod_Plantilla",
                    new { Cod_Plantilla });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla eliminada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un asiento en una plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del asiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PlantillaAsiento_Insertar(int CodEmpresa, PlantillaAsientoDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT INTO CxP_Plantillas_Asiento(Linea, cod_plantilla, cod_cuenta, cod_contabilidad, cod_divisa, cod_unidad, cod_centro_costo, porcentaje)
                  VALUES(@Linea, @Cod_Plantilla, @Cod_Cuenta, @Cod_Contabilidad, @Cod_Divisa, @Cod_Unidad, @Cod_Centro_Costo, @Porcentaje)",
                new
                {
                    data.Linea,
                    data.Cod_Plantilla,
                    data.Cod_Cuenta,
                    data.Cod_Contabilidad,
                    data.Cod_Divisa,
                    data.Cod_Unidad,
                    data.Cod_Centro_Costo,
                    data.Porcentaje
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla asiento agregada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar asiento de plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza un asiento de plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del asiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PlantillaAsiento_Actualizar(int CodEmpresa, PlantillaAsientoDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE CxP_Plantillas_Asiento
                  SET cod_cuenta = @Cod_Cuenta,
                      cod_divisa = @Cod_Divisa,
                      cod_unidad = @Cod_Unidad,
                      cod_centro_costo = @Cod_Centro_Costo,
                      porcentaje = @Porcentaje
                  WHERE linea = @Linea
                    AND cod_plantilla = @Cod_Plantilla",
                new
                {
                    data.Cod_Cuenta,
                    data.Cod_Divisa,
                    data.Cod_Unidad,
                    data.Cod_Centro_Costo,
                    data.Porcentaje,
                    data.Linea,
                    data.Cod_Plantilla
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla asiento actualizada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar asiento de plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un asiento de plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del asiento a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PlantillaAsiento_Borrar(int CodEmpresa, PlantillaAsientoDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = @Cod_Plantilla AND linea = @Linea",
                new
                {
                    data.Cod_Plantilla,
                    data.Linea
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Plantilla asiento eliminada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar asiento de plantilla.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}