using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndContratosAplicacionDb
    {
        private readonly PortalDB _portalDB;

        public FrmCoAplFndContratosAplicacionDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplFndContratosAplicacionDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        /// <summary>
        /// Obtiene la información de los contratos con mora para aplicar fondos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            const string sql = @"exec spCBR_Fondos_Apl_Contratos_Proceso_Carga_Informacion";

            return DbHelper.ExecuteListQuery<CoAplFndContrAplInformacionData>(
                _portalDB, codEmpresa, sql);
        }

        /// <summary>
        /// Aplica los fondos a los contratos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoAplExcProcesadosResult> CO_AplFndContrApl_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            var response = new ErrorDto<CoAplExcProcesadosResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoAplExcProcesadosResult
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
                    .ToList() ?? new List<CoAplExcProcInformacionData>();

                if (seleccionados.Count == 0)
                {
                    response.Code = -1;
                    response.Description = "No hay casos a procesar";
                    return response;
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaSql = "exec spCBR_Fondos_Apl_Contratos_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaSql,
                    new { Usuario = usuario }
                );

                int contadorValidos = 0;
                int totalCasos = seleccionados.Count;

                foreach (var cedula in seleccionados.Select(x => x.cedula.Trim()))
                {
                    const string aplicaSql =
                        @"exec spCBR_Fondos_Apl_Contratos_Proceso_Aplicacion @Usuario, @Cedula, @AplicacionId;";

                    connection.Execute(aplicaSql, new
                    {
                        Usuario = usuario,
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    const string validaSql =
                        @"select dbo.fxCBR_Fondos_Apl_Contratos_Proceso_Valida(
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
