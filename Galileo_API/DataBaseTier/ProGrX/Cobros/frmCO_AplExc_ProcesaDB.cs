using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaDb
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 4;

        public FrmCoAplExcProcesaDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCoAplExcProcesaDb(PortalDB portalDB, MSecurityMainDb mSecurity)
        {
            _portalDB = portalDB;
        }

        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            const string sql = @"exec spCBR_Excedente_Apl_Proceso_Carga_Informacion";

            return DbHelper.ExecuteListQuery<CoAplExcProcInformacionData>(
                _portalDB, codEmpresa, sql);
        }

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
                    new { Usuario = usuario }
                );

                int contadorValidos = 0;
                int totalCasos = seleccionados.Count;

                foreach (var item in seleccionados)
                {
                    var cedula = item.cedula.Trim();

                    const string aplicaSql =
                        @"exec spCBR_Excedente_Apl_Proceso_Aplicacion 
                    @Usuario, @Cedula, @AplicacionId;";

                    connection.Execute(aplicaSql, new
                    {
                        Usuario = usuario,
                        Cedula = cedula,
                        AplicacionId = idAplicacion
                    });

                    const string validaSql =
                        @"select dbo.fxCBR_Excedente_Apl_Proceso_Valida(
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