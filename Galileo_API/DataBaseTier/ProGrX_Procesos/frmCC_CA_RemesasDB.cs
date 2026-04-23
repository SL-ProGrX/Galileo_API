using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcCaRemesasDB
    {
        private readonly PortalDB _portalDB;

        public FrmCcCaRemesasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los catálogos base de la pantalla de remesas:
        /// líneas, entidades, procesos, cuotas y tipos de filtro.
        /// </summary>
        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var response = new CcCaRemesasCatalogosResponse();

            try
            {
                const string sqlLineas = @"
                    SELECT
                        COD_REMESA AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM PRM_CA_TIPOS_REMESA
                    WHERE activo = 1
                    ORDER BY COD_REMESA;";

                const string sqlEntidades = @"
                    SELECT
                        RTRIM(cod_entidad) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM PRM_CA_ENTIDAD
                    WHERE activo = 1
                    ORDER BY cod_entidad;";

                response.lineas = conn.Query<DropDownListaGenericaModel>(sqlLineas).ToList();
                response.entidades = conn.Query<DropDownListaGenericaModel>(sqlEntidades).ToList();

                for (var i = 1; i <= 5; i++)
                {
                    response.cuotas.Add(new DropDownListaGenericaModel
                    {
                        item = i.ToString(),
                        descripcion = i.ToString()
                    });
                }

                response.filtros.Add(new DropDownListaGenericaModel { item = "C", descripcion = "Cédula" });
                response.filtros.Add(new DropDownListaGenericaModel { item = "N", descripcion = "Nombre" });
                response.filtros.Add(new DropDownListaGenericaModel { item = "O", descripcion = "Operación" });

                var periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                for (var i = 0; i <= 6; i++)
                {
                    var valor = periodo.AddMonths(i).ToString("yyyyMM");
                    response.procesos.Add(new DropDownListaGenericaModel
                    {
                        item = valor,
                        descripcion = valor
                    });
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasCatalogosResponse>(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Consulta los casos candidatos a enviar en remesa de cargos automáticos.
        /// </summary>
        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int codEmpresa,
            CcCaRemesasEnvioConsultaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<CcCaRemesasEnvioConsultaData>(
                    "spPrm_CA_Remesa_Envia_Consulta",
                    new
                    {
                        RemesaTipo = request.cod_remesa,
                        Entidad = request.cod_entidad.Trim(),
                        Fecha = request.fecha_vence.Date,
                        NCuotas = request.cuotas
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las remesas pendientes de recibir/aplicar.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<DropDownListaGenericaModel>(
                    "spPrm_CA_Remesa_Envia_Pendiente",
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de una remesa pendiente o procesada para la pestaña recibe/aplica.
        /// </summary>
        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int codEmpresa, long remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<CcCaRemesasRecibeDetalleData>(
                    "spPrm_CA_Remesa_Consultas",
                    new { remesa },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasRecibeDetalleData>>(ex.Message);
            }
        }
    }
}
