using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.DataBaseTier
{
    
    public class FrmCajasRoeAnularDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasRoeAnularDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Consulta los ROE de cajas para anular
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CajasRoeAnularLista> CajasRoeAnular_Obtener(int CodEmpresa, FiltrosCajasRoeAnularData filtros)
        {
            var result = new ErrorDto<CajasRoeAnularLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasRoeAnularLista
                {
                    total = 0,
                    lista = new List<CajasRoeAnularData>()
                }
            };

            try
            {
                filtros ??= new FiltrosCajasRoeAnularData();
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var totalQuery = "select COUNT(1) from CAJAS_ROE";
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                var sortField = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? "ID_ROE"
                    : filtros.sortField;

                var query = @"select ID_ROE, TIPOROE, rtrim(CEDULA_ASO) as 'CEDULA_ASO', IDENTIFICACION_DEPO, NOMBRE_DEPO, FECHA, USUARIO, MONTO_LOCAL, MONTO_DOL,TIPO_CAMBIO
                                , REGISTRO_FECHA, REGISTRO_USUARIO, ACTUALIZA_FECHA, ACTUALIZA_USUARIO, USUARIO_ANULACION, FECHA_ANULACION, OBSERV_ANULACION, IMPRIME_FECHA, IMPRIME_USUARIO
                                , ISNULL(ID_SESION,'') AS 'ID_SESION', ESTADO
                                From CAJAS_ROE WHERE ESTADO = 'A'  ";

                if (!filtros.rango_fechas)
                {
                    query += @" and Fecha between @fecha_inicio and @fecha_fin ";
                }

                if (!string.IsNullOrWhiteSpace(filtros.IDENTIFICACION_DEPO))
                {
                    query += @" and ( Cedula_Aso like '%'+@cedula+'%' or IDENTIFICACION_DEPO like '%'+@cedula+'%' )";
                }

                if (!string.IsNullOrWhiteSpace(filtros.NOMBRE_DEPO))
                {
                    query += @" and NOMBRE_DEPO like '%'+@nombre+'%'";
                }

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    query += @" and (
                            CAST(ID_ROE AS varchar(50)) like '%'+@filtro+'%'
                            or TIPOROE like '%'+@filtro+'%'
                            or CEDULA_ASO like '%'+@filtro+'%'
                            or IDENTIFICACION_DEPO like '%'+@filtro+'%'
                            or NOMBRE_DEPO like '%'+@filtro+'%') ";
                }

                query += $@" order by {sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                             OFFSET {filtros.pagina} ROWS
                             FETCH NEXT {filtros.paginacion} ROWS ONLY";

                result.Result.lista = connection.Query<CajasRoeAnularData>(query, new
                {
                    fecha_inicio = filtros.fecha_desde,
                    fecha_fin = filtros.fecha_hasta,
                    cedula = filtros.IDENTIFICACION_DEPO,
                    nombre = filtros.NOMBRE_DEPO,
                    filtro = filtros.filtro
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<CajasRoeAnularData>();
            }

            return result;
        }

        /// <summary>
        /// Anula un ROE de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="roe"></param>
        /// <param name="notas"></param>
        /// <returns></returns>
        public ErrorDto CajasRoeAnular_Anular(int CodEmpresa, string usuario, string roe, string notas)
        {
            const string sql = "spCajas_ROE_Anula";
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = DbHelper.ExecuteStoredProcedureSingle<int>(
                connectionString,
                sql,
                0,
                new
                {
                    roe,
                    notas,
                    usuario
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(result.Description ?? "Error desconocido", result.Code ?? -1);
        }
    }
}