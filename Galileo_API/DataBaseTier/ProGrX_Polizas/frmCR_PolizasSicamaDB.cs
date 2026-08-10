using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasSicamaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmCrPolizasSicamaDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Metodo para obtener pólizas SICAMA (MAC Vida)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasSicama_Polizas_Lista(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spPolizas_MAC_Vida_List";

                var result = conn.Query<dynamic>(query).ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IDX,        // según alias del SP
                    descripcion = x.ITMX
                }).ToList();

                return lista;
            });
        }

        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _proGrxMain.fxFechaServidor(codEmpresa, 0);
        }

        #region Envio

        /// <summary>
        /// Metodo para consultar el corte (tab Envío) - equivalente a btnCorte en VB6.
        /// Ejecuta: spPoliza_Sicama(@Poliza, @Corte, @Beneficiarios, @Usuario, @TipoMovimiento)
        /// </summary>
        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Envio_Consulta(
            int CodEmpresa,
            string Usuario,
            CrPolizasSicamaEnvioConsultaRequest request)
        {
            if (CodEmpresa <= 0)
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Empresa inválida.");

            if (string.IsNullOrWhiteSpace(Usuario))
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Usuario inválido.");

            if (request == null)
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Datos inválidos.");

            if (string.IsNullOrWhiteSpace(request.Poliza))
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("La póliza es requerida.");

            if (string.IsNullOrWhiteSpace(request.TipoMovimiento))
                request.TipoMovimiento = "T";

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Poliza", request.Poliza, DbType.String, size: 10);
                parameters.Add("@Corte", request.Corte, DbType.DateTime);
                parameters.Add("@Beneficiarios", request.Beneficiarios, DbType.Int16);
                parameters.Add("@Usuario", Usuario, DbType.String, size: 30);
                parameters.Add("@Movimiento", request.TipoMovimiento, DbType.String, size: 5);

                var data = conn.Query<CrPolizasSicamaEnvioRow>(
                    "spPoliza_Sicama",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return data;
            });
        }

        #endregion

        #region Genera

        /// <summary>
        /// Genera el corte SICAMA para la fecha indicada.
        /// Equivalente a sbCorte_Genera del VB6 que ejecuta spPolizas_Sicama_Genera.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cr_PolizasSicama_Genera(int codEmpresa, DateTime fechaCorte, string usuario)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "OK",
                Result = true
            };

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "dbo.spPolizas_Sicama_Genera",
                    new
                    {
                        pFecha = fechaCorte,
                        pUsuario = usuario
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        #endregion

        #region Consulta

        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Consulta_Obtener(
            int CodEmpresa,
            string Usuario,
            CrPolizasSicamaEnvioConsultaRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Request inválido.");
            }

            if (string.IsNullOrWhiteSpace(request.Poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Debe seleccionar la póliza.");
            }

            var movimiento = (request.TipoMovimiento ?? "T").Trim().ToUpperInvariant();
            var movimientosValidos = new HashSet<string> { "T", "I", "M", "SC", "E" };
            if (!movimientosValidos.Contains(movimiento))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaEnvioRow>>("Movimiento inválido.");
            }

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {

                const string sql = @"
                        EXEC dbo.spPoliza_Sicama
                            @Poliza,
                            @Corte,
                            @Beneficiarios,
                            @Usuario,
                            @Movimiento;
                        ";

                var parametros = new
                {
                    Poliza = request.Poliza,
                    Corte = request.Corte,
                    Beneficiarios = 1,
                    Usuario,
                    Movimiento = movimiento
                };

                var data = conn.Query<CrPolizasSicamaEnvioRow>(sql, parametros).ToList();
                return data;
            });
        }

        #endregion

        #region Beneficiario

        public ErrorDto<List<CrPolizasSicamaBeneficiariosRowDto>>
                Cr_PolizasSicama_Beneficiarios_Lista(
                int CodEmpresa,
                string Usuario,
                string poliza)
        {
            if (string.IsNullOrWhiteSpace(poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizasSicamaBeneficiariosRowDto>>(
                    "Debe indicar la póliza.");
            }

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
              
                const string sql = @"
                    EXEC dbo.spPoliza_Beneficiarios_Lista
                        @Poliza;
                    ";

                var parametros = new
                {
                    Poliza = poliza.Trim()
                };

                var data = conn
                    .Query<CrPolizasSicamaBeneficiariosRowDto>(sql, parametros)
                    .ToList();

                return data;
            });
        }

        #endregion

        #region Recepcion

        public ErrorDto Cr_FndPlanillaDirecta_Sube(
            int CodEmpresa,
            string Usuario,
            CrFndPlanillaDirectaSubeRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            if (request == null)
            {
                return DbHelper.ErrorResponse("Request inválido.");
            }

            if (request.institucion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la institución.");
            }

            if (request.operadora <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la operadora.");
            }

            if (string.IsNullOrWhiteSpace(request.plan))
            {
                return DbHelper.ErrorResponse("Debe indicar el plan.");
            }

            if (string.IsNullOrWhiteSpace(request.documento))
            {
                return DbHelper.ErrorResponse("Debe indicar el documento.");
            }

            if (request.proceso <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar el proceso.");
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (string.IsNullOrWhiteSpace(request.nombre))
            {
                return DbHelper.ErrorResponse("Debe indicar el nombre.");
            }

            const string sql = @"
                        EXEC dbo.spFndPlanillaDirecta_Sube
                            @Institucion,
                            @Operadora,
                            @Plan,
                            @Documento,
                            @Proceso,
                            @Cedula,
                            @Nombre,
                            @Fondos,
                            @Linea,
                            @Inicializa;
";

            var parametros = new
            {
                Institucion = request.institucion,
                Operadora = request.operadora,
                Plan = request.plan.Trim(),
                Documento = request.documento.Trim(),
                Proceso = request.proceso,
                Cedula = request.cedula.Trim(),
                Nombre = request.nombre.Trim(),
                Fondos = request.fondos,
                Linea = request.linea,
                Inicializa = request.inicializa
            };

            var row = conn.Execute(sql, parametros);

            return row > 0
                ? DbHelper.OkResponse("Datos subidos correctamente.")
                : DbHelper.ErrorResponse("No se pudo subir la información.");
        }

        public ErrorDto<List<CrFndPlanillaDirectaConsultaRowDto>>
            Cr_FndPlanillaDirecta_Consulta(
            int CodEmpresa,
            string Usuario,
            CrFndPlanillaDirectaConsultaRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<
                    List<CrFndPlanillaDirectaConsultaRowDto>>(
                    "Request inválido.");
            }

            if (request.operadora <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    List<CrFndPlanillaDirectaConsultaRowDto>>(
                    "Debe indicar la operadora.");
            }

            if (string.IsNullOrWhiteSpace(request.plan))
            {
                return DbHelper.CreateErrorResponse<
                    List<CrFndPlanillaDirectaConsultaRowDto>>(
                    "Debe indicar el plan.");
            }

            if (string.IsNullOrWhiteSpace(request.documento))
            {
                return DbHelper.CreateErrorResponse<
                    List<CrFndPlanillaDirectaConsultaRowDto>>(
                    "Debe indicar el documento.");
            }
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
               

                const string sql = @"
                    EXEC dbo.spFndPlanillaDirecta_Consulta
                        @Operadora,
                        @Plan,
                        @Documento,
                        @Revisar;
";

                var parametros = new
                {
                    Operadora = request.operadora,
                    Plan = request.plan.Trim(),
                    Documento = request.documento.Trim(),
                    Revisar = request.revisar
                };

                var data = conn
                    .Query<CrFndPlanillaDirectaConsultaRowDto>(sql, parametros)
                    .ToList();

                return data;
            });
        }

        #endregion
    }
}
