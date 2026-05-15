using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {

        private const string SqlInsertPlanEstado = @"
                    IF NOT EXISTS
                    (
                        SELECT 1
                        FROM dbo.fnd_planes_estados
                        WHERE cod_plan = @cod_plan
                          AND cod_operadora = @cod_operadora
                          AND cod_estado = @cod_estado
                    )
                    BEGIN
                        INSERT INTO dbo.fnd_planes_estados
                        (
                            cod_plan,
                            cod_operadora,
                            cod_estado,
                            registro_usuario,
                            registro_fecha
                        )
                        VALUES
                        (
                            @cod_plan,
                            @cod_operadora,
                            @cod_estado,
                            @usuario,
                            dbo.MyGetDate()
                        );
                    END;";

        private const string SqlDeletePlanEstado = @"
                    DELETE FROM dbo.fnd_planes_estados
                    WHERE cod_plan = @cod_plan
                      AND cod_operadora = @cod_operadora
                      AND cod_estado = @cod_estado;";

        private const string SqlInsertPlanPlazo = @"
                    IF NOT EXISTS
                    (
                        SELECT 1
                        FROM dbo.fnd_planes_plazos
                        WHERE cod_plan = @cod_plan
                          AND cod_operadora = @cod_operadora
                          AND plazo = @plazo
                    )
                    BEGIN
                        INSERT INTO dbo.fnd_planes_plazos
                        (
                            cod_plan,
                            cod_operadora,
                            plazo,
                            registro_usuario,
                            registro_fecha
                        )
                        VALUES
                        (
                            @cod_plan,
                            @cod_operadora,
                            @plazo,
                            @usuario,
                            dbo.MyGetDate()
                        );
                    END;";

        private const string SqlDeletePlanPlazo = @"
                    DELETE FROM dbo.fnd_planes_plazos
                    WHERE cod_plan = @cod_plan
                      AND cod_operadora = @cod_operadora
                      AND plazo = @plazo;";

        #region Vencimientos

        /// <summary>
        /// Guarda los estados y plazos de vencimiento asociados a un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="dto">Datos de estados y plazos del plan.</param>
        /// <returns>Indicador de éxito de la operación.</returns>
        public ErrorDto<bool> Fnd_Planes_Vencimientos_Guardar(int CodEmpresa, string usuario, FndPlanesVencimientosGuardarDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de vencimientos son requeridos.", -2, false);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var estado in dto.estados ?? new List<EstadoAsignadoDto>())
                {
                    connection.Execute(
                        estado.asignado ? SqlInsertPlanEstado : SqlDeletePlanEstado,
                        CrearParametrosEstadoVencimiento(usuario, dto, estado));
                }

                foreach (var plazo in dto.plazos ?? new List<PlazoAsignadoDto>())
                {
                    connection.Execute(
                        plazo.asignado ? SqlInsertPlanPlazo : SqlDeletePlanPlazo,
                        CrearParametrosPlazoVencimiento(usuario, dto, plazo));
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al guardar vencimientos.", result.Code.GetValueOrDefault(-1), false);
        }

        /// <summary>
        /// Crea los parámetros seguros para insertar o eliminar estados de vencimiento.
        /// </summary>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="dto">Datos generales del plan.</param>
        /// <param name="estado">Estado a procesar.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosEstadoVencimiento(string usuario, FndPlanesVencimientosGuardarDto dto, EstadoAsignadoDto estado)
        {
            return new
            {
                cod_plan = NormalizarTexto(dto.cod_plan),
                cod_operadora = dto.cod_operadora,
                cod_estado = estado.cod_estado,
                usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Crea los parámetros seguros para insertar o eliminar plazos de vencimiento.
        /// </summary>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="dto">Datos generales del plan.</param>
        /// <param name="plazo">Plazo a procesar.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosPlazoVencimiento(string usuario, FndPlanesVencimientosGuardarDto dto, PlazoAsignadoDto plazo)
        {
            return new
            {
                cod_plan = NormalizarTexto(dto.cod_plan),
                cod_operadora = dto.cod_operadora,
                plazo = plazo.plazo,
                usuario = NormalizarTexto(usuario)
            };
        }

        #endregion

    }
}