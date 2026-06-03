using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAfAfiliacionTramiteDb
    {
        private readonly IConfiguration _config;

        public FrmAfAfiliacionTramiteDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_AfiliacionTramite_Instituciones_Obtener(int CodEmpresa)
        {
            const string query = @"select COD_INSTITUCION as item,  '[' + COD_DIVISA + ']  ' + DESCRIPCION as descripcion
                        from INSTITUCIONES where ACTIVA = 1 
                        order by COD_INSTITUCION";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtener listado de afiliaciones en trámite
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfAfiliacionTramiteDto>> AF_AfiliacionTramite_Obtener(int CodEmpresa, AfAfiliacionTramiteFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de afiliación en trámite son requeridos.", -2, new List<AfAfiliacionTramiteDto>());
            }

            var fechaInicio = filtros.inicio.Date;
            var fechaCorte = filtros.corte.Date.AddDays(1).AddTicks(-1);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var sql = @"select S.* , isnull(Pe.Descripcion,'No Localizado') as EstadoPersona 
                        from Socios S left join AFI_ESTADOS_PERSONA Pe on S.EstadoActual = Pe.COD_ESTADO 
                        where S.EstadoActual = 'T'";

                var parameters = new DynamicParameters();
                parameters.Add("Inicio", fechaInicio);
                parameters.Add("Corte", fechaCorte);

                if (!string.Equals(filtros.institucion, "TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND S.COD_INSTITUCION = @CodInstitucion";
                    parameters.Add("CodInstitucion", filtros.codInstitucion);
                }

                if (!string.IsNullOrWhiteSpace(filtros.cedula))
                {
                    sql += " AND S.CEDULA like @Cedula";
                    parameters.Add("Cedula", $"%{filtros.cedula.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(filtros.idAlterna))
                {
                    sql += " AND S.CEDULAR like @IdAlterna";
                    parameters.Add("IdAlterna", $"%{filtros.idAlterna.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(filtros.nombre))
                {
                    sql += " AND S.NOMBRE like @Nombre";
                    parameters.Add("Nombre", $"%{filtros.nombre.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(filtros.usuario))
                {
                    sql += " AND S.REG_USER like @Usuario";
                    parameters.Add("Usuario", $"%{filtros.usuario.Trim()}%");
                }

                sql += " AND S.FECHAINGRESO between @Inicio AND @Corte order by S.CEDULA";

                return connection.Query<AfAfiliacionTramiteDto>(sql, parameters).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfAfiliacionTramiteDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener afiliaciones en trámite.", result.Code.GetValueOrDefault(-1), new List<AfAfiliacionTramiteDto>());
        }

        /// <summary>
        /// Aprobar afiliaciones en trámite seleccionadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Lista"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_AfiliacionTramite_Aprobar(int CodEmpresa, List<AfAfiliacionTramiteDto> Lista, string Usuario)
        {
            if (Lista is null)
            {
                return DbHelper.ErrorResponse("La lista de afiliaciones es requerida.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var afectados = 0;
                foreach (var item in Lista)
                {
                    connection.Execute(
                        "spAFI_Afiliacion_EnTramite_Resolucion",
                        new { Cedula = item.cedula, Usuario },
                        commandType: System.Data.CommandType.StoredProcedure);
                    afectados++;
                }

                return afectados;
            });

            return result.Code == 0
                ? DbHelper.OkResponse($"Casos Afectados({result.Result})")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aprobar afiliaciones en trámite.", result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
