using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data.Common;

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
    }
}
