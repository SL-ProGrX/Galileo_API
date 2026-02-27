using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizaProcPrevistaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }


        /// <summary>
        /// Metodo para obtener pólizas facturables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spPoliza_Facturables_Lista";

                var result = conn.Query<dynamic>(query).ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,      
                    descripcion = x.ItmX  
                }).ToList();

                return lista;
            });
        }

        #region Generacion

        /// <summary>
        /// Método para cargar detalle de prevista (spPoliza_Prevista_Corte_Detalle_Cargar)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Cargar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleAddRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request is null.");

                if (string.IsNullOrWhiteSpace(request.cod_poliza))
                    return DbHelper.ErrorResponse("cod_poliza es requerido.");

                if (request.lineas == null || request.lineas.Count == 0)
                    return DbHelper.ErrorResponse("No existen líneas para procesar.");

                const string query = @"
                    EXEC spPoliza_Prevista_Corte_Detalle_Add
                        @IdLinea,
                        @CodPoliza,
                        @Corte,
                        @Factura,
                        @Cedula,
                        @Nombre,
                        @NumPoliza,
                        @MontoAsegurado,
                        @Prima,
                        @Operacion,
                        @Usuario,
                        @Inicializa,
                        @MuestraResultado;";

                for (int i = 0; i < request.lineas.Count; i++)
                {
                    var x = request.lineas[i];

                    var param = new
                    {
                        IdLinea = i + 1, // backend controla el orden
                        CodPoliza = request.cod_poliza,
                        Corte = request.corte,
                        Factura = request.factura ?? string.Empty,
                        Cedula = x.cedula ?? string.Empty,
                        Nombre = x.nombre ?? string.Empty,
                        NumPoliza = x.n_poliza ?? string.Empty,
                        MontoAsegurado = x.monto_asegurado,
                        Prima = x.prima,
                        Operacion = x.operacion,
                        Usuario = usuario,
                        Inicializa = (i == 0) ? 1 : 0,
                        MuestraResultado = 0
                    };

                    connection.Execute(query, param);
                }


                return DbHelper.OkResponse("Información Cargada Satisfactoriamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para eliminar registros seleccionados (spPoliza_Prevista_Corte_Detalle_Elimina)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Eliminar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleEliminarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request is null.");

                if (request.id_registros == null || request.id_registros.Count == 0)
                    return DbHelper.ErrorResponse("No existen registros seleccionados para eliminar.");

                var motivo = string.IsNullOrWhiteSpace(request.motivo)
                    ? "Poliza Descartada por Cancelacion"
                    : request.motivo.Trim();

                const string query = @"
                    EXEC spPoliza_Prevista_Corte_Detalle_Elimina
                        @IdRegistro,
                        @Usuario,
                        @Notas ;";

                foreach (var param in request.id_registros
                    .Distinct()
                    .Where(id => id > 0)
                    .Select(id => new
                    {
                        IdRegistro = id,
                        Usuario = usuario,
                        Notas = motivo
                    }))
                {
                    connection.Execute(query, param);
                }

                return DbHelper.OkResponse("Registros eliminados satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para consultar detalle de prevista (spPoliza_Prevista_Corte_Detalle_Consulta)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codPoliza"></param>
        /// <param name="corte"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolProcprevistaDetalleDto>> Cr_PolProcPrevista_Corte_Detalle_Consulta(
            int CodEmpresa,
            string codPoliza,
            DateTime corte)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spPoliza_Prevista_Corte_Detalle_Consulta @Corte, @CodPoliza";

                var param = new
                {
                    Corte = corte,
                    CodPoliza = codPoliza
                };

                return conn.Query<CrPolProcprevistaDetalleDto>(query, param).ToList();
            });
        }
        #endregion

        #region Consulta

        /// <summary>
        /// Método para consultar conciliación de prevista (spPoliza_Prevista_Corte_Concilia)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolProcPrevistaConciliaDto>> Cr_PolProcPrevista_Corte_Concilia_Consulta(
            int CodEmpresa,
            CrPolProcPrevistaConciliaConsultaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<List<CrPolProcPrevistaConciliaDto>>("Request is null.");

            if (request.corte == default)
                return DbHelper.CreateErrorResponse<List<CrPolProcPrevistaConciliaDto>>("corte es requerido.");

            if (string.IsNullOrWhiteSpace(request.cod_poliza))
                return DbHelper.CreateErrorResponse<List<CrPolProcPrevistaConciliaDto>>("cod_poliza es requerido.");

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spPoliza_Prevista_Corte_Concilia @Corte, @CodPoliza;";

                var param = new
                {
                    Corte = request.corte,
                    CodPoliza = request.cod_poliza
                };

                return conn.Query<CrPolProcPrevistaConciliaDto>(query, param).ToList();
            });
        }

        #endregion
    }
}
