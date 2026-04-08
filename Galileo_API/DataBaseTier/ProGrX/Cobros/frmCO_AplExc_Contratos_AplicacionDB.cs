using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcContratosAplicacionDb
    {
        private readonly PortalDB _portalDB;

        public FrmCoAplExcContratosAplicacionDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplExcContratosAplicacionDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        /// <summary>
        /// Obtiene la información de los contratos para aplicar excedentes a mora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {

            using var connection = _portalDB.CreateConnection(codEmpresa);

            var response = connection.Query<CoAplExcContrAplInformacionData> (
                sql: "exec spCBR_Excedente_Apl_Contratos_Proceso_Carga_Informacion",
                commandTimeout: 0
            ).ToList();

            return DbHelper.CreateOkResponse(response);
        }

        /// <summary>
        /// Aplica los excedentes a mora para los contratos itemsados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoAplExcContrAplicadosResult> CO_AplExcContrApl_Aplicar(int CodEmpresa, ExcContratosAplicarRequest request)
        {
            var response = new ErrorDto<CoAplExcContrAplicadosResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoAplExcContrAplicadosResult
                {
                    aplicados = 0, pendientes = 0
                }
            };

            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();
                var items = request.Seleccionados
                    ?.Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .ToList() ?? new List<CoAplExcContrAplInformacionData>();

                if (items.Count == 0)
                {
                    response.Code = -1;
                    response.Description = "No hay casos a procesar";
                    return response;
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaAplSql = "exec spCBR_Excedente_Apl_Contratos_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaAplSql,
                    new { Usuario = usuario }
                );

                int contValidos = 0;
                int totalCasos = items.Count;

                foreach (var cedula in items.Select(x => x.cedula.Trim()))
                {
                    const string aplicSql =
                        @"exec spCBR_Excedente_Apl_Contratos_Proceso_Aplicacion @Usuario, @Cedula, @AplicacionId;";

                    connection.Execute(aplicSql, new
                    {
                        Usuario = usuario,
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    const string validaSql =
                        @"select dbo.fxCBR_Excedente_Apl_Contratos_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

                    bool valido = connection.QuerySingle<bool>(validaSql, new
                    {
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    if (valido)
                    {
                        contValidos++;
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description =
                            $"El caso {cedula} no se proces&oacute; correctamente. " +
                            $"Aplicación: {idAplicacion}. " +
                            $"Procesados: {contValidos} de {totalCasos}.";
                        response.Result = new CoAplExcContrAplicadosResult
                        {
                            aplicados = contValidos,
                            pendientes = totalCasos - contValidos
                        };
                        return response;
                    }
                }

                response.Description = "Proceso concluido satisfactoriamente";

                response.Result = new CoAplExcContrAplicadosResult
                {
                    aplicados = contValidos,
                    pendientes = totalCasos - contValidos
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
