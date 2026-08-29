using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaDb
    {
        private readonly PortalDB _portalDB;

        public FrmCoAplExcProcesaDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplExcProcesaDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        /// <summary>
        /// Obtener información de excedentes a procesar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            using var connection = _portalDB.CreateConnection(codEmpresa);

            var response = connection.Query<CoAplExcProcInformacionData>(
                sql: "exec spCBR_Excedente_Apl_Proceso_Carga_Informacion",
                commandTimeout: 0
            ).ToList();

            return DbHelper.CreateOkResponse(response);

        }

        /// <summary>
        /// Aplicar excedentes seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            var response = new ErrorDto<CoAplExcProcesadosResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoAplExcProcesadosResult {
                    aplicados = 0,
                    pendientes = 0
                }
            };

            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();
                var seleccionados = request.Seleccionados
                    ?.Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .ToList() ?? new List<CoAplExcProcInformacionData>();

                if (seleccionados.Count == 0)
                {
                    response.Code = -1;
                    response.Description = "No hay casos a procesar";
                    return response;
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaSql = "exec spCBR_Excedente_Apl_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaSql,
                    new { Usuario = usuario },
                    commandTimeout: 0
                );

                int contadorValidos = 0;
                int totalCasos = seleccionados.Count;

                foreach (var cedula in seleccionados.Select(x => x.cedula.Trim()))
                {
                    const string aplicaSql =
                        @"exec spCBR_Excedente_Apl_Proceso_Aplicacion 
                    @Usuario, @Cedula, @AplicacionId;";

                    connection.Execute(aplicaSql, new
                    {
                        Usuario = usuario,
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    }, commandTimeout: 0);

                    const string validaSql =
                        @"select dbo.fxCBR_Excedente_Apl_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

                    bool valido = connection.QuerySingle<bool>(validaSql, new
                    {
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    }, commandTimeout: 0);

                    if (valido)
                    {
                        contadorValidos++;
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description =
                            $"El caso {cedula} no se proces&oacute; correctamente. " +
                            $"Aplicación: {idAplicacion}. " +
                            $"Procesados: {contadorValidos} de {totalCasos}.";
                        response.Result = new CoAplExcProcesadosResult
                        {
                            aplicados = contadorValidos,
                            pendientes = totalCasos - contadorValidos
                        };
                        return response;
                    }
                }

                response.Description = "Proceso concluido satisfactoriamente";

                response.Result = new CoAplExcProcesadosResult
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