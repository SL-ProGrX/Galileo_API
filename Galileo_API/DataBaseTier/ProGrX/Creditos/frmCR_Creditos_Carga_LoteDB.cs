using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCreditosCargaLoteDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrCreditosCargaLoteDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT
                        RTRIM(codigo) AS item,
                        RTRIM(descripcion) + '  [' + RTRIM(codigo) + ']' AS descripcion
                    FROM catalogo
                    WHERE retencion = 'N'
                      AND activo = 1
                      AND codigo NOT IN (
                          SELECT codigo_ase
                          FROM fnd_planes
                      )
                    ORDER BY codigo;";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de destinos asociados a una línea de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Destinos_Obtener(int CodEmpresa, string codigo)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    select
                        rtrim(D.cod_Destino) as item,
                        rtrim(D.descripcion) as descripcion
                    from catalogo_destinos D
                    inner join catalogo_destinosASG C
                        on D.cod_destino = C.cod_destino
                    WHERE C.codigo = @Codigo    
                    order by D.prioridad asc;";

                return conn.Query<DropDownListaGenericaModel>(
                    sqlQuery,
                    new { Codigo = (codigo ?? string.Empty).Trim() }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de conceptos de desembolso activos que retienen.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ConceptosDesembolso_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT
                        COD_CONDEB AS item,
                        DESCRIPCION AS descripcion
                    FROM CONCEPTO_DESEMB
                    WHERE ACTIVO = 1
                      AND RETIENE = 1;";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ObtenerDeductoras(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT 
                        COD_INSTITUCION AS item,
                        DESCRIPCION AS descripcion
                    FROM INSTITUCIONES
                    WHERE ACTIVA = 1
                      AND DEDUCCION_PLANILLA = 1";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrecuenciaReductora>> CrCreditosCargaLote_ObtenerFrecuenciaDeductora(int CodEmpresa, string CodInstitucion)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT 
                        RTRIM(descripcion) AS Descripcion,
                        ISNULL(Frecuencia,'M') AS Frecuencia_Id
                    FROM instituciones
                    WHERE cod_institucion = @CodInstitucion";

                return conn.Query<FrecuenciaReductora>(
                    sqlQuery,
                    new { CodInstitucion = (CodInstitucion ?? string.Empty).Trim() }).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spCrd_SGT_Bancos @Usuario";

                var param = new { Usuario = usuario };

                var result = conn.Query<dynamic>(query, param).ToList();                

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return lista;
            });
        }

        /// <summary>
        /// Elimina registros cargados por cliente y proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Codigo"></param>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        public ErrorDto CrCreditosCargaLote_Cargado_Eliminar(int CodEmpresa, string Codigo, long Proceso)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    DELETE CRD_CREDITOS_CARGADO_H
                    WHERE codigo = @Codigo
                      AND PROCESO = @Proceso";

                conn.Execute(
                    sqlQuery,
                    new
                    {
                        Codigo = (Codigo ?? string.Empty).Trim(),
                        Proceso
                    });

                return DbHelper.CreateOkResponse();
            }).Result ?? DbHelper.ErrorResponse("Error al eliminar registros cargados.");
        }

        /// <summary>
        /// Inserta un registro en créditos cargados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCreditosCargaLote_Cargado_Insertar(int CodEmpresa, CrCreditosCargaLoteCargadoInsertarRequest request)
        {
            var response = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    INSERT INTO CRD_CREDITOS_CARGADO_H (
                        LINEA,
                        CODIGO,
                        COD_REFERENCIA,
                        PROCESO,
                        CEDULA,
                        MONTO,
                        NOMBRE,
                        TIPO,
                        PLAZO,
                        TASA,
                        CUOTA,
                        COMISION,
                        DOCUMENTO,
                        NOTAS
                    )
                    VALUES (
                        @Linea,
                        @Codigo,
                        @Cod_Referencia,
                        @Proceso,
                        @Cedula,
                        @Monto,
                        @Nombre,
                        'D',
                        @Plazo,
                        0,
                        0,
                        @Comision,
                        @Documento,
                        @Notas
                    )";

                conn.Execute(
                    sqlQuery,
                    new
                    {
                        request.Linea,
                        Codigo = (request.Codigo ?? string.Empty).Trim(),
                        Cod_Referencia = (request.Cod_Referencia ?? string.Empty).Trim(),
                        request.Proceso,
                        Cedula = (request.Cedula ?? string.Empty).Trim(),
                        request.Monto,
                        Nombre = (request.Nombre ?? string.Empty).Trim(),
                        request.Plazo,
                        request.Comision,
                        Documento = (request.Documento ?? string.Empty).Trim(),
                        Notas = (request.Notas ?? string.Empty).Trim()
                    });

                return DbHelper.CreateOkResponse();
            });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(response.Description ?? "Error al insertar registro cargado.", response.Code ?? -1);
            }

            return response.Result ?? DbHelper.ErrorResponse("Error al insertar registro cargado.");
        }

        /// <summary>
        /// Revisa y compara listado cargado con la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCreditosCargaLoteCargadoRevisadoResponse>> CrCreditosCargaLote_Cargado_Revisado(int CodEmpresa, CrCreditosCargaLoteCargadoRevisadoRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"EXEC spCrd_Creditos_Lote_Cargado_Revisado @ClienteId, @Referencia, @Proceso, @Banco, @Destino";

                return conn.Query<CrCreditosCargaLoteCargadoRevisadoResponse>(
                    sqlQuery,
                    new
                    {
                        ClienteId = (request.ClienteId ?? string.Empty).Trim(),
                        Referencia = (request.Referencia ?? string.Empty).Trim(),
                        request.Proceso,
                        request.Banco,
                        Destino = string.IsNullOrWhiteSpace(request.Destino) ? null : request.Destino.Trim()
                    }).ToList();
            });
        }

        /// <summary>
        /// Obtiene el listado de proveedores de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorCxpModel>> CrCreditosCargaLote_ProveedorCxp_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT
                        cod_proveedor AS Cod_Proveedor,
                        cedjur AS CedJur,
                        descripcion AS Descripcion
                    FROM cxp_proveedores order by cod_proveedor asc";

                return conn.Query<ProveedorCxpModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Procesa lote de créditos cargados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCreditosCargaLote_Procesa(int CodEmpresa, CrCreditosCargaLoteProcesaRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    EXEC spCrd_Creditos_Lote_Procesa
                        @CodigoLinea,
                        @Proceso,
                        @TipoDocumento,
                        @PriDeduc,
                        @Banco,
                        @Proveedor,
                        @ComisionRef,
                        @Usuario,
                        @CodDestino,
                        @Aplicacion";

                conn.Execute(
                    sqlQuery,
                    new
                    {
                        CodigoLinea = (request.Codigo_Linea ?? string.Empty).Trim(),
                        request.Proceso,
                        TipoDocumento = (request.Tipo_Documento ?? string.Empty).Trim(),
                        PriDeduc = request.Pri_Deduc,
                        request.Banco,
                        request.Proveedor,
                        ComisionRef = (request.Comision_Ref ?? string.Empty).Trim(),
                        Usuario = (request.Usuario ?? string.Empty).Trim(),
                        CodDestino = (request.Cod_Destino ?? string.Empty).Trim(),
                        Aplicacion = string.IsNullOrWhiteSpace(request.Aplicacion) ? "ProGrX" : request.Aplicacion.Trim()
                    });

                return DbHelper.CreateOkResponse();
            }).Result ?? DbHelper.ErrorResponse("Error al procesar lote de créditos.");
        }
    }
}
