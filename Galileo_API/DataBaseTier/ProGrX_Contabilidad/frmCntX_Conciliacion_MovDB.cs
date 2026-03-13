using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConciliacionMovDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXConciliacionMovDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmCntXConciliacionMovDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public ErrorDto<CntXConciliacionResult> CntXConciliacionMov_Conciliar(int codEmpresa, CntXConciliacionMovRequest request)
        {
            try
            {
                var init = CntXConciliacionMov_Inicializar(codEmpresa, request);
                if (init.Code == -1)
                    return ErrorResult(init.Description);

                var proceso = CntXConciliacionMov_Procesar(codEmpresa, request, 1);
                if (proceso.Code == -1 || proceso.Result == null)
                    return ErrorResult(proceso.Description ?? "No se obtuvo respuesta del proceso inicial.");

                int intentos = 0;
                const int maxIntentos = 10000;

                while (proceso.Result.pendientes > 0)
                {
                    if (intentos++ >= maxIntentos)
                        return ErrorResult("El proceso de conciliación excedió el número máximo de iteraciones.");

                    proceso = CntXConciliacionMov_Procesar(codEmpresa, request, 20);

                    if (proceso.Code == -1 || proceso.Result == null)
                        return ErrorResult(proceso.Description ?? "No se obtuvo respuesta válida del proceso.");
                }

                var debitos = CntXConciliacionResultados_Obtener(codEmpresa, request, "DB");
                if (debitos.Code == -1)
                    return ErrorResult(debitos.Description);

                var creditos = CntXConciliacionResultados_Obtener(codEmpresa, request, "CR");
                if (creditos.Code == -1)
                    return ErrorResult(creditos.Description);

                var conciliados = CntXConciliacionResultados_Obtener(codEmpresa, request, "CON");
                if (conciliados.Code == -1)
                    return ErrorResult(conciliados.Description);

                return new ErrorDto<CntXConciliacionResult>
                {
                    Code = 0,
                    Description = "Proceso concluido correctamente.",
                    Result = new CntXConciliacionResult
                    {
                        debitos = debitos.Result ?? new List<CntXConciliacionMovData>(),
                        creditos = creditos.Result ?? new List<CntXConciliacionMovData>(),
                        conciliados = conciliados.Result ?? new List<CntXConciliacionMovData>()
                    }
                };
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
        }

        public ErrorDto CntXConciliacionMov_Inicializar(int codEmpresa, CntXConciliacionMovRequest request)
        {
            string query = @"exec spCntX_Concilia_Inicializa @Usuario, @CodConta, @Cuenta, @FechaInicio, @FechaCorte";
            return DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, query, new
                {
                    Usuario = request.usuario,
                    CodConta = request.cod_contabilidad,
                    Cuenta = request.cuenta,
                    FechaInicio = request.fecha_inicio.Date,
                    FechaCorte = request.fecha_corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                });
        }

        public ErrorDto<CntXConciliacionProcesoData?> CntXConciliacionMov_Procesar(int codEmpresa, CntXConciliacionMovRequest request, int top)
        {
            string query = @"exec spCntX_Concilia_Procesa @Usuario, @CodConta, @Cuenta, @Top";
            return DbHelper.ExecuteSingleQuery<CntXConciliacionProcesoData>(
                _portalDb, codEmpresa, query, null, new
                {
                    Usuario = request.usuario,
                    CodConta = request.cod_contabilidad,
                    Cuenta = request.cuenta,
                    Top = top
                });
        }

        public ErrorDto<List<CntXConciliacionMovData>> CntXConciliacionResultados_Obtener(int codEmpresa, CntXConciliacionMovRequest request, string tipo)
        {
            string query = @"exec spCntX_Concilia_Resultados @Usuario, @CodConta, @Cuenta, @Tipo";
            return DbHelper.ExecuteListQuery<CntXConciliacionMovData>(
                _portalDb, codEmpresa, query, new
                {
                    Usuario = request.usuario,
                    CodConta = request.cod_contabilidad,
                    Cuenta = request.cuenta,
                    Tipo = tipo
                });
        }

        private ErrorDto<CntXConciliacionResult> ErrorResult(string? description)
        {
            return new ErrorDto<CntXConciliacionResult>
            {
                Code = -1,
                Description = description,
                Result = null
            };
        }
    }
}