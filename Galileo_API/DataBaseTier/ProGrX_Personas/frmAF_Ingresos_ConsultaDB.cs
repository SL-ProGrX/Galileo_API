using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFIngresosConsultaDB
    {
        private readonly IConfiguration _config;

        public FrmAFIngresosConsultaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ErrorDto<IngresosConsultaLista> AF_Ingresos_Consulta(int CodEmpresa, IngresosConsultaFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de ingresos son requeridos.", -2, new IngresosConsultaLista
                {
                    Lista = new List<IngresosConsultaData>()
                });
            }

            var resultadoVacio = new IngresosConsultaLista
            {
                Lista = new List<IngresosConsultaData>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lista = connection.Query<IngresosConsultaData>(
                    "spAFI_Afiliaciones_Consulta",
                    new
                    {
                        Cedula = filtro.Cedula,
                        Nombre = filtro.Nombre,
                        Inicio = filtro.Inicio,
                        Corte = filtro.Corte,
                        Usuario = filtro.Usuario,
                        Promotor = filtro.Promotor
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                return new IngresosConsultaLista
                {
                    Lista = lista
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar ingresos.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
