using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Data.Common;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasMonitoreoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCFacturasMonitoreoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de personas para monitoreo de facturas.
        /// Permite ordenar por cédula o nombre según parámetro.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="ordenarPor">Valores esperados: "cedula" o "nombre".</param>
        /// <param name="esPagador">Indica si se filtra solo cédulas que existen en cxc_contratos_pagadores.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoPersonas_Obtener(
            int codEmpresa,
            string ordenarPor,
            bool esPagador)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var orden = (ordenarPor ?? string.Empty).Trim().ToLowerInvariant();
                var sortCode = orden == "nombre" ? 2 : 1;

                const string sql = @"
                    SELECT
                        RTRIM(Cedula) AS item,
                        RTRIM(Nombre) AS descripcion
                    FROM CxC_Personas
                    WHERE
                        @esPagador = 0
                        OR Cedula IN (
                            SELECT Cedula
                            FROM cxc_contratos_pagadores
                        )
                    ORDER BY
                        CASE WHEN @sortCode = 1 THEN Cedula END,
                        CASE WHEN @sortCode = 2 THEN Nombre END;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { sortCode, esPagador }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, []);
            }
        }

        /// <summary>
        /// Obtiene el catálogo de conceptos con proceso de descuento activo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoConceptos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_CONCEPTO) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CXC_CONCEPTOS
                WHERE PROCESO_DESCUENTO = 1
                ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene estados de factura activos por proceso.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCFacturasMonitoreoEstadoProcesoDto>> CxCFacturasMonitoreoEstadosPorProceso_Obtener(
            int codEmpresa,
            string proceso)
        {
            const string sql = @"
                SELECT
                    RTRIM(FACTURA_ESTADO) AS Factura_Estado
                FROM CXC_FACTURAS_ESTADOS
                WHERE PROCESO = @Proceso
                  AND ACTIVO = 1
                ORDER BY FACTURA_ESTADO;";

            return DbHelper.ExecuteListQuery<CxCFacturasMonitoreoEstadoProcesoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Proceso = (proceso ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Obtiene el catálogo de contratos activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoContratos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_CONTRATO) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CXC_CONTRATOS
                WHERE ACTIVO = 1
                ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el catálogo de estados de factura.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoEstados_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(FACTURA_ESTADO) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM CXC_FACTURAS_ESTADOS;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el monitoreo de facturas según filtros dinámicos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCFacturasMonitoreoItemDto>> CxCFacturasMonitoreoFacturas_Obtener(
            int codEmpresa,
            CxCFacturasMonitoreoFiltroDto filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                filtro ??= new CxCFacturasMonitoreoFiltroDto
                {
                    Adelantadas = false
                };

                var sql = new StringBuilder(@"
                    SELECT
                        '' AS Seleccion,
                        Operacion AS Operacion,
                        cod_Factura AS Cod_Factura,
                        Monto AS Monto,
                        Factura_Estado_Desc AS Factura_Estado_Desc,
                        Fecha_Emision AS Fecha_Emision,
                        Cod_Divisa AS Cod_Divisa,
                        Tipo_Cambio AS Tipo_Cambio,
                        Adelanto_Porc AS Adelanto_Porc,
                        Adelanto_Monto AS Adelanto_Monto,
                        Pendiente AS Pendiente,
                        RTRIM(Cedula) + ' - ' + Cliente_Nombre AS Cliente,
                        RTRIM(Cedula_Pagador) + ' - ' + Pagador_Nombre AS Pagador,
                        Cancela_Fecha AS Cancela_Fecha,
                        '[' + ISNULL(CONVERT(VARCHAR(30), Pago_Principal_Remesa), '') + '] '
                            + ISNULL(CONVERT(VARCHAR(30), Pago_Principal_Fecha), '') AS Remesa_I,
                        '[' + ISNULL(CONVERT(VARCHAR(30), Pago_Segundo_Remesa), '') + '] '
                            + ISNULL(CONVERT(VARCHAR(30), Pago_Segundo_Fecha), '') AS Remesa_II
                    FROM vCxC_Facturas_Control
                    WHERE cod_Factura LIKE '%' + @Cod_Factura + '%'");

                var parameters = new DynamicParameters();
                parameters.Add("Cod_Factura", (filtro.Cod_Factura ?? string.Empty).Trim());

                if (!string.IsNullOrWhiteSpace(filtro.Cliente_Id))
                {
                    sql.Append(" AND Cedula LIKE '%' + @Cliente_Id + '%' ");
                    parameters.Add("Cliente_Id", filtro.Cliente_Id.Trim());
                }

                if (!string.IsNullOrWhiteSpace(filtro.Cliente_Nombre))
                {
                    sql.Append(" AND Cliente_Nombre LIKE '%' + @Cliente_Nombre + '%' ");
                    parameters.Add("Cliente_Nombre", filtro.Cliente_Nombre.Trim());
                }

                if (!string.IsNullOrWhiteSpace(filtro.Pagador_Id))
                {
                    sql.Append(" AND Cedula_Pagador LIKE '%' + @Pagador_Id + '%' ");
                    parameters.Add("Pagador_Id", filtro.Pagador_Id.Trim());
                }

                if (!string.IsNullOrWhiteSpace(filtro.Pagador_Nombre))
                {
                    sql.Append(" AND Pagador_Nombre LIKE '%' + @Pagador_Nombre + '%' ");
                    parameters.Add("Pagador_Nombre", filtro.Pagador_Nombre.Trim());
                }

                var contratos = (filtro.Contratos ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (contratos.Count > 0)
                {
                    sql.Append(" AND Cod_Contrato IN @contratos ");
                    parameters.Add("contratos", contratos);
                }

                var conceptos = (filtro.Conceptos ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (conceptos.Count > 0)
                {
                    sql.Append(" AND Cod_Concepto IN @conceptos ");
                    parameters.Add("conceptos", conceptos);
                }

                if (filtro.Fecha_Inicio.HasValue && filtro.Fecha_Corte.HasValue)
                {
                    var columnaFecha = ObtenerColumnaFechaMonitoreo(filtro.Tipo_Fecha);
                    if (!string.IsNullOrEmpty(columnaFecha))
                    {
                        sql.Append($" AND {columnaFecha} BETWEEN @fecha_inicio AND @fecha_corte ");
                        parameters.Add("fecha_inicio", filtro.Fecha_Inicio.Value.Date);
                        parameters.Add("fecha_corte", filtro.Fecha_Corte.Value.Date.AddDays(1).AddTicks(-1));
                    }
                }

                if (!string.IsNullOrWhiteSpace(filtro.Estado) &&
                    !string.Equals(filtro.Estado.Trim(), "TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(" AND Factura_Estado = @estado ");
                    parameters.Add("estado", filtro.Estado.Trim());
                }

                if (filtro.Adelantadas)
                {
                    sql.Append(" AND ADELANTO_INDICA = 1 ");
                }

                var lista = conn.Query<CxCFacturasMonitoreoItemDto>(sql.ToString(), parameters).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CxCFacturasMonitoreoItemDto>>(ex.Message, -1, []);
            }
        }

        private static string? ObtenerColumnaFechaMonitoreo(string? tipoFecha)
        {
            return (tipoFecha ?? string.Empty).Trim() switch
            {
                "Registro" => "Registro_Fecha",
                "Emisión" => "Fecha_Emision",
                "Emision" => "Fecha_Emision",
                "Pago" => "Fecha_Pago",
                "Libera" => "Liberado_Fecha",
                "Cancela" => "Cancela_Fecha",
                "Activación" => "Activa_Fecha",
                "Activacion" => "Activa_Fecha",
                "Desembolso 1" => "Pago_Principal_Fecha",
                "Desembolso 2" => "Pago_Secundario_Fecha",
                _ => null
            };
        }

        /// <summary>
        /// Consulta detalle de factura por operación/factura según tipo de consulta del SP.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CxCFacturasMonitoreoDetalle_Obtener(
            int codEmpresa,
            CxCFacturasMonitoreoDetalleRequestDto request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                request ??= new CxCFacturasMonitoreoDetalleRequestDto();

                var consulta = string.IsNullOrWhiteSpace(request.Consulta)
                    ? "G"
                    : request.Consulta.Trim().ToUpperInvariant();

                var lista = conn.Query<dynamic>(
                    "spCxC_Operacion_Factura_Detalle",
                    new
                    {
                        request.Operacion,
                        request.Factura,
                        Consulta = consulta
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(new TablasListaGenericaModel
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(
                    ex.Message,
                    -1,
                    new TablasListaGenericaModel
                    {
                        total = 0,
                        lista = (object)new List<dynamic>()
                    });
            }
        }

        /// <summary>
        /// Cambia el estado de una factura de una operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CxCFacturasMonitoreoEstado_Actualizar(
            int codEmpresa,
            CxCFacturasMonitoreoEstadoRequestDto request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                request ??= new CxCFacturasMonitoreoEstadoRequestDto();

                conn.Execute(
                    "spCxC_Operacion_Factura_Estado",
                    new
                    {
                        request.Operacion,
                        Factura = (request.Factura ?? string.Empty).Trim(),
                        Estado = (request.Estado_Confirmacion ?? string.Empty).Trim(),
                        Usuario = (request.Usuario ?? string.Empty).Trim(),
                        Actualiza = 1,
                        Resultado = 0
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message, -1, false);
            }
        }
    }
}
