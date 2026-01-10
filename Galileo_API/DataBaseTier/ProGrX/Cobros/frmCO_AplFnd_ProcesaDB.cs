using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndProcesaDb
    {
        private readonly PortalDB _portalDB;

        public FrmCoAplFndProcesaDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplFndProcesaDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        /// <summary>
        /// Obtener información de fondos a procesar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
        {
            const string sql = @"exec spCBR_Fondos_Apl_Proceso_Carga_Informacion";

            return DbHelper.ExecuteListQuery<CoAplFndProcInformacionData>(
                _portalDB, codEmpresa, sql);
        }

        /// <summary>
        /// Aplicar fondos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoAplFndProcesadosResult> CO_AplFnd_Procesa_Aplicar(int CodEmpresa, FondosAplicarRequest request)
        {
            var response = new ErrorDto<CoAplFndProcesadosResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoAplFndProcesadosResult
                {
                    aplicados = 0,
                    pendientes = 0
                }
            };

            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();

                var seleccionados = request.Seleccionados
                    ?.Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .Select(x => x.cedula.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    ?? new List<string>();

                if (seleccionados.Count == 0)
                {
                    response.Code = -1;
                    response.Description = "No hay casos a procesar";
                    return response;
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaSql =
                    @"exec spCBR_Fondos_Apl_Proceso_Guia_Aplicacion @Usuario;";

                int idAplicacion = connection.QuerySingle<int>(guiaSql, new { Usuario = usuario });

                int contadorValidos = 0;
                int totalCasos = seleccionados.Count;

                foreach (var cedula in seleccionados)
                {
                    const string aplicaSql =
                        @"exec spCBR_Fondos_Apl_Proceso_Aplicacion 
                    @Usuario, @Cedula, @AplicacionId;";

                    connection.Execute(aplicaSql, new
                    {
                        Usuario = usuario,
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    const string validaSql =
                        @"select dbo.fxCBR_Fondos_Apl_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

                    bool valido = connection.QuerySingle<bool>(validaSql, new
                    {
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    if (valido)
                    {
                        contadorValidos++;

                        response.Result = new CoAplFndProcesadosResult
                        {
                            aplicados = contadorValidos,
                            pendientes = totalCasos - contadorValidos
                        };
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description =
                            $"El caso {cedula} no se procesó correctamente. " +
                            $"Aplicación: {idAplicacion}. " +
                            $"Procesados: {contadorValidos} de {totalCasos}.";
                        response.Result = new CoAplFndProcesadosResult
                        {
                            aplicados = contadorValidos,
                            pendientes = totalCasos - contadorValidos
                        };
                        return response;
                    }
                }

                response.Description = "Proceso concluido satisfactoriamente";
                response.Result = new CoAplFndProcesadosResult
                {
                    aplicados = contadorValidos,
                    pendientes = totalCasos - contadorValidos
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                return response;
            }
        }
    }
}
