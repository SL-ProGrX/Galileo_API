using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        public ErrorDto<SimpleSuccessResult> Fnd_AhorroConsolidado_Procesar(
            int CodEmpresa,
            FndAhorroConsolidadoRequest request)
        {
            if (request is null)
            {
                return ErrorSimple(
                    "Los datos de ahorro consolidado son requeridos.",
                    -2);
            }

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        if (request.Socios is not null)
                        {
                            foreach (var socio in request.Socios)
                            {
                                ProcesarSocioAhorro(
                                    connection,
                                    transaction,
                                    request,
                                    socio);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return CrearRespuestaSimple(
                result,
                "Error al procesar ahorro consolidado.");
        }

        private static void ProcesarSocioAhorro(
            SqlConnection connection,
            SqlTransaction transaction,
            FndAhorroConsolidadoRequest request,
            FndAhorroConsolidadoSocio socio)
        {
            var movimiento = ObtenerMovimientoAhorro(
                request.Destino ?? string.Empty,
                socio.EstadoActual ?? string.Empty,
                socio.Existe ?? string.Empty);

            if (movimiento is null)
            {
                return;
            }

            var parametros = new
            {
                Cedula = NormalizarTexto(socio.Cedula),
                socio.Monto
            };

            connection.Execute(
                movimiento.Value.SqlConsolidado,
                parametros,
                transaction);

            connection.Execute(
                SqlAhorroDetalladoInsert,
                CrearParametrosAhorroDetalle(
                    request,
                    socio,
                    movimiento.Value.Tipo),
                transaction);
        }
    }
}