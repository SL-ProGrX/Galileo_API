using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaMacHogarDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmCrPolizaMacHogarDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Método para obtener la lista de pólizas MAC Hogar.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista para llenar el combo de pólizas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizaMacHogar_Polizas_Lista(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"EXEC spPolizas_MAC_Incendio_List";

                var result = conn.Query<dynamic>(query).ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return lista;
            });
        }

        /// <summary>
        /// Método para obtener la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Fecha actual del servidor.</returns>
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _proGrxMain.fxFechaServidor(codEmpresa, 0);
        }

        #region Envio

        /// <summary>
        /// Método para consultar el corte del tab Envío.
        /// </summary>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Envio_Consulta(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            if (codEmpresa <= 0)
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Empresa inválida.");

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Usuario inválido.");

            if (request == null)
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Datos inválidos.");

            if (string.IsNullOrWhiteSpace(request.Poliza))
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("La póliza es requerida.");

            if (string.IsNullOrWhiteSpace(request.TipoMovimiento))
                request.TipoMovimiento = "T";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Poliza", request.Poliza, DbType.String, size: 10);
                parameters.Add("@Corte", request.Corte, DbType.DateTime);
                parameters.Add("@Beneficiarios", request.Beneficiarios, DbType.Int16);
                parameters.Add("@Usuario", usuario, DbType.String, size: 30);
                parameters.Add("@Movimiento", request.TipoMovimiento, DbType.String, size: 5);

                var data = conn.Query<CrPolizaMacHogarEnvioRow>(
                    "spPoliza_Incendio_Cierre",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return data;
            });
        }

        #endregion

        #region Recepcion
        /// <summary>
        /// Método para validar la información cargada en recepción de MAC Hogar.
        /// Valida estructura mínima y devuelve la lista normalizada para mostrar en grid.
        /// </summary>
        public ErrorDto<List<CrPolizaMacHogarRecepcionRowDto>> Cr_PolizaMacHogar_Recepcion_Validar(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarRecepcionValidarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarRecepcionRowDto>>(
                    "Request inválido.");
            }

            if (string.IsNullOrWhiteSpace(request.Poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarRecepcionRowDto>>(
                    "Debe seleccionar la póliza.");
            }

            if (request.Filas == null || request.Filas.Count == 0)
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarRecepcionRowDto>>(
                    "No se recibieron filas para validar.");
            }

            var filasNormalizadas = new List<CrPolizaMacHogarRecepcionRowDto>();

            for (int i = 0; i < request.Filas.Count; i++)
            {
                var fila = request.Filas[i];

                if (string.IsNullOrWhiteSpace(fila.cedula))
                {
                    return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarRecepcionRowDto>>(
                        $"La fila {fila.numero_linea} no contiene identificación.");
                }

                if (string.IsNullOrWhiteSpace(fila.nombre))
                {
                    return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarRecepcionRowDto>>(
                        $"La fila {fila.numero_linea} no contiene nombre.");
                }

                filasNormalizadas.Add(new CrPolizaMacHogarRecepcionRowDto
                {
                    numero_linea = fila.numero_linea,
                    documento = fila.documento?.Trim() ?? string.Empty,
                    fecha_proceso = fila.fecha_proceso,
                    cod_operadora = fila.cod_operadora,
                    cod_plan = fila.cod_plan?.Trim() ?? string.Empty,
                    cod_contrato = fila.cod_contrato,
                    cedula = fila.cedula.Trim(),
                    nombre = fila.nombre.Trim(),
                    fondos = fila.fondos,
                    cod_institucion = fila.cod_institucion,
                    existe_persona = fila.existe_persona,
                    existe_contrato = fila.existe_contrato,
                    procesado = fila.procesado
                });
            }

            return DbHelper.CreateOkResponse(filasNormalizadas);
        }

        /// <summary>
        /// Método para procesar la recepción de MAC Hogar.
        /// El proceso final no está definido en VB6, por lo que se responde de forma controlada.
        /// </summary>
        public static ErrorDto Cr_PolizaMacHogar_Recepcion_Procesar(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarRecepcionProcesarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Request inválido.");
            }

            if (string.IsNullOrWhiteSpace(request.Poliza))
            {
                return DbHelper.ErrorResponse("Debe seleccionar la póliza.");
            }

            if (request.Filas == null || request.Filas.Count == 0)
            {
                return DbHelper.ErrorResponse("No existen filas para procesar.");
            }

            return DbHelper.ErrorResponse(
                "El proceso de recepción MAC Hogar aún no está definido funcionalmente en ProGrX.");
        }
        #endregion

        #region Consulta
        /// <summary>
        /// Método para consultar información del tab Consultas.
        /// </summary>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Consulta_Obtener(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Request inválido.");
            }

            if (string.IsNullOrWhiteSpace(request.Poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Debe seleccionar la póliza.");
            }

            var movimiento = (request.TipoMovimiento ?? "T").Trim().ToUpperInvariant();
            var movimientosValidos = new HashSet<string> { "T", "I", "E", "M", "SC" };

            if (!movimientosValidos.Contains(movimiento))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Movimiento inválido.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Poliza", request.Poliza, DbType.String, size: 10);
                parameters.Add("@Corte", request.Corte, DbType.DateTime);
                parameters.Add("@Beneficiarios", 1, DbType.Int16);
                parameters.Add("@Usuario", usuario, DbType.String, size: 30);
                parameters.Add("@Movimiento", movimiento, DbType.String, size: 5);

                var data = conn.Query<CrPolizaMacHogarEnvioRow>(
                    "spPoliza_Incendio_Cierre",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return data;
            });
        }

        #endregion

        #region Beneficiarios
        /// <summary>
        /// Método para obtener la lista de beneficiarios por póliza.
        /// </summary>
        public ErrorDto<List<CrPolizaMacHogarBeneficiariosRowDto>> Cr_PolizaMacHogar_Beneficiarios_Lista(
            int codEmpresa,
            string usuario,
            string poliza)
        {
            if (string.IsNullOrWhiteSpace(poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarBeneficiariosRowDto>>(
                    "Debe indicar la póliza.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"
                    EXEC dbo.spPoliza_Beneficiarios_Lista 'PC'";

                var parametros = new
                {
                    Poliza = poliza.Trim()
                };

                var data = conn.Query<CrPolizaMacHogarBeneficiariosRowDto>(sql, parametros).ToList();
                return data;
            });
        }
        #endregion
    }
}
