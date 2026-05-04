using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCCActualizaUpUtDb
    {
        private readonly PortalDB _portalDb;

        public FrmCCActualizaUpUtDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public async Task<ErrorDto> CC_ActualizaUpUt_ProcesarArchivo(
            int codEmpresa,
            string usuario,
            IFormFile file)
        {
            string stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            ErrorDto response = new ErrorDto();

            try
            {
                List<CcActualizaUpUtLinea> lineas;

                using (var stream = file.OpenReadStream())
                {
                    lineas = await CC_ActualizaUpUt_LeerArchivo(stream);
                }

                if (lineas.Count == 0)
                {
                    response.Code = -1;
                    response.Description = "El archivo no contiene registros válidos.";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                try
                {
                    await CC_ActualizaUpUt_EjecutarProceso(connection, transaction, usuario, lineas);

                    transaction.Commit();
                    response.Code = 1;
                    response.Description = "Información actualizada correctamente.";
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private async Task<List<CcActualizaUpUtLinea>> CC_ActualizaUpUt_LeerArchivo(Stream stream)
        {
            var resultado = new List<CcActualizaUpUtLinea>();

            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);

            int numeroLinea = 0;

            while (!reader.EndOfStream)
            {
                string? rawLine = await reader.ReadLineAsync();
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                string line = rawLine.TrimEnd('\r');

                if (line.Length < 61)
                {
                    throw new ArgumentException($"La línea {numeroLinea} no cumple con la longitud mínima requerida.", nameof(line));
                }

                var item = new CcActualizaUpUtLinea
                {
                    cedula = CC_ActualizaUpUt_LimpiarCampo(line.Substring(0, 11)),
                    unidad_programatica = CC_ActualizaUpUt_LimpiarCampo(line.Substring(53, 4)),
                    unidad_trabajo = CC_ActualizaUpUt_LimpiarCampo(line.Substring(57, 4))
                };

                if (string.IsNullOrWhiteSpace(item.cedula))
                {
                    throw new ArgumentException($"La línea {numeroLinea} no contiene una cédula válida.", nameof(item.cedula));
                }

                if (string.IsNullOrWhiteSpace(item.unidad_programatica))
                {
                    throw new ArgumentException($"La línea {numeroLinea} no contiene una unidad programática válida.", nameof(item.unidad_programatica));
                }

                if (string.IsNullOrWhiteSpace(item.unidad_trabajo))
                {
                    throw new ArgumentException($"La línea {numeroLinea} no contiene una unidad de trabajo válida.", nameof(item.unidad_trabajo));
                }

                resultado.Add(item);
            }

            return resultado;
        }

        private static string CC_ActualizaUpUt_LimpiarCampo(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private async Task CC_ActualizaUpUt_EjecutarProceso(
            SqlConnection connection,
            SqlTransaction transaction,
            string usuario,
            List<CcActualizaUpUtLinea> lineas)
        {
            for (int i = 0; i < lineas.Count; i++)
            {
                int accion = i == 0 ? 1 : 2;

                await connection.ExecuteAsync(
                    "spPrmProcAddUPUT_Actualiza_Manual",
                    new
                    {
                        Cedula = lineas[i].cedula,
                        UP = lineas[i].unidad_programatica,
                        UT = lineas[i].unidad_trabajo,
                        Paso = accion
                    },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            var ultimaLinea = lineas[^1];

            await connection.ExecuteAsync(
                "spPrmProcAddUPUT_Actualiza_Manual",
                new
                {
                    Cedula = ultimaLinea.cedula,
                    UP = ultimaLinea.unidad_programatica,
                    UT = ultimaLinea.unidad_trabajo,
                    Paso = 3
                },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }

    }
}
