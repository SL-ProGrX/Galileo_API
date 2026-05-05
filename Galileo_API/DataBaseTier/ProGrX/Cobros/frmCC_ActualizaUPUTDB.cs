using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;

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

                await using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    await CC_ActualizaUpUt_EjecutarProceso(connection, transaction, lineas);

                    await transaction.CommitAsync();
                    response.Code = 1;
                    response.Description = "Información actualizada correctamente.";
                }
                catch
                {
                    await transaction.RollbackAsync();
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
            string? rawLine;

            while ((rawLine = await reader.ReadLineAsync()) is not null)
            {
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                string line = rawLine.TrimEnd('\r');

                if (line.Length < 61)
                {
                    throw new InvalidDataException(
                        $"La línea {numeroLinea} no cumple con la longitud mínima requerida.");
                }

                var item = new CcActualizaUpUtLinea
                {
                    cedula = CC_ActualizaUpUt_LimpiarCampo(line.Substring(0, 11)),
                    unidad_programatica = CC_ActualizaUpUt_LimpiarCampo(line.Substring(53, 4)),
                    unidad_trabajo = CC_ActualizaUpUt_LimpiarCampo(line.Substring(57, 4))
                };

                if (string.IsNullOrWhiteSpace(item.cedula))
                {
                    throw new InvalidDataException(
                        $"La línea {numeroLinea} no contiene una cédula válida.");
                }

                if (string.IsNullOrWhiteSpace(item.unidad_programatica))
                {
                    throw new InvalidDataException(
                        $"La línea {numeroLinea} no contiene una unidad programática válida.");
                }

                if (string.IsNullOrWhiteSpace(item.unidad_trabajo))
                {
                    throw new InvalidDataException(
                        $"La línea {numeroLinea} no contiene una unidad de trabajo válida.");
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
            DbTransaction transaction,
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
