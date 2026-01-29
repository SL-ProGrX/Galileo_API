using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasxCobrar; 

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCConceptosDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCConceptosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta todos los conceptos de cuentas por cobrar.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de conceptos.</returns>
        public ErrorDto<List<CxcConceptoDto>> CxcConceptos_Lista(int codEmpresa)
        {
            var query = @"SELECT COD_CONCEPTO as Cod_Concepto, DESCRIPCION, COD_CUENTA as Cod_Cuenta, COD_CUENTA_SALIDA as Cod_Cuenta_Salida,
                                 REQUIERE_CONTRATO as Requiere_Contrato, REQUIERE_DOCUMENTO as Requiere_Documento, GENERA_DESEMBOLSO as Genera_Desembolso,
                                 PROCESO_DESCUENTO as Proceso_Descuento, ACTIVO, ADELANTO_INFORMATIVO, REGISTRO_FECHA, REGISTRO_USUARIO, PAGADOR_DEFAULT,
                                 MONTO_MAX, COD_UNIDAD as Cod_Unidad, COD_CENTRO_COSTO as Cod_Centro_Costo, I_INDICADOR as I_Indicador,
                                 I_CTA_DETERIORO as I_Cta_Deterioro, I_CTA_ESTIMACION as I_Cta_Estimacion, I_CTA_ORDEN_DEBE as I_Cta_Orden_Debe,
                                 I_CTA_ORDEN_HABER as I_Cta_Orden_Haber, I_CTA_INGRESO as I_Cta_Ingreso, MODIFICA_USUARIO, MODIFICA_FECHA
                          FROM CxC_Conceptos
                          ORDER BY COD_CONCEPTO";
            return DbHelper.ExecuteListQuery<CxcConceptoDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Verifica si existe un concepto por código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codigo">Código del concepto.</param>
        /// <returns>Resultado con la cantidad encontrada.</returns>
        public ErrorDto<CxcConceptoExisteResult?> CxcConceptos_Existe(int codEmpresa, string codigo)
        {
            var query = @"SELECT ISNULL(COUNT(*),0) as Existe FROM CxC_Conceptos WHERE COD_CONCEPTO = @Codigo";
            return DbHelper.ExecuteSingleQuery<CxcConceptoExisteResult>(_portalDb, codEmpresa, query, default, new { Codigo = codigo });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un concepto de cuentas por cobrar.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del concepto.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_Guardar(int codEmpresa, CxcConceptoSaveParams param)
        {
            var existe = CxcConceptos_Existe(codEmpresa, param.Cod_Concepto).Result?.Existe ?? 0;
            return existe == 0
                ? CxcConceptos_Insertar(codEmpresa, param)
                : CxcConceptos_Actualizar(codEmpresa, param);
        }

        /// <summary>
        /// Inserta un nuevo concepto.
        /// </summary>
        private ErrorDto<bool> CxcConceptos_Insertar(int codEmpresa, CxcConceptoSaveParams param)
        {
            const string sql = @"
                INSERT INTO CxC_Conceptos (
                    COD_CONCEPTO, DESCRIPCION, COD_CUENTA, COD_CUENTA_SALIDA,
                    REQUIERE_CONTRATO, REQUIERE_DOCUMENTO, GENERA_DESEMBOLSO,
                    PROCESO_DESCUENTO, MONTO_MAX, ACTIVO, ADELANTO_INFORMATIVO,
                    REGISTRO_FECHA, REGISTRO_USUARIO
                )
                VALUES (
                    @Cod_Concepto, @Descripcion, @Cod_Cuenta, @Cod_Cuenta_Salida,
                    @Requiere_Contrato, @Requiere_Documento, @Genera_Desembolso,
                    @Proceso_Descuento, @Monto_Max, @Activo, 0, GETDATE(), @Usuario
                );";

            var parameters = new
            {
                param.Cod_Concepto,
                param.Descripcion,
                param.Cod_Cuenta,
                param.Cod_Cuenta_Salida,
                param.Requiere_Contrato,
                param.Requiere_Documento,
                param.Genera_Desembolso,
                param.Proceso_Descuento,
                param.Monto_Max,
                param.Activo,
                param.Usuario
            };

            // Usa el helper transaccional para mejor manejo de errores y conexión
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, parameters);
                return rows > 0;
            });
        }

        /// <summary>
        /// Actualiza un concepto existente.
        /// </summary>
        private ErrorDto<bool> CxcConceptos_Actualizar(int codEmpresa, CxcConceptoSaveParams param)
        {
            const string sql = @"
                UPDATE CxC_Conceptos
                SET DESCRIPCION = @Descripcion,
                    COD_CUENTA = @Cod_Cuenta,
                    COD_CUENTA_SALIDA = @Cod_Cuenta_Salida,
                    REQUIERE_CONTRATO = @Requiere_Contrato,
                    REQUIERE_DOCUMENTO = @Requiere_Documento,
                    GENERA_DESEMBOLSO = @Genera_Desembolso,
                    PROCESO_DESCUENTO = @Proceso_Descuento,
                    MONTO_MAX = @Monto_Max,
                    ACTIVO = @Activo,
                    MODIFICA_USUARIO = @Usuario,
                    MODIFICA_FECHA = GETDATE()
                WHERE COD_CONCEPTO = @Cod_Concepto;";

            var parameters = new
            {
                param.Cod_Concepto,
                param.Descripcion,
                param.Cod_Cuenta,
                param.Cod_Cuenta_Salida,
                param.Requiere_Contrato,
                param.Requiere_Documento,
                param.Genera_Desembolso,
                param.Proceso_Descuento,
                param.Monto_Max,
                param.Activo,
                param.Usuario
            };

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, parameters);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina un concepto por código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con el código del concepto.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_Eliminar(int codEmpresa, CxcConceptoDeleteParams param)
        {
            var query = @"DELETE FROM CxC_Conceptos WHERE COD_CONCEPTO = @Cod_Concepto";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(query, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Consulta todos los conceptos de cuentas por cobrar como lista desplegable (formato genérico).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de conceptos en formato de lista desplegable.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxcConceptos_ListaGenerica(int codEmpresa)
        {
            var query = @"
                SELECT rtrim(COD_CONCEPTO) AS item,
                       rtrim(DESCRIPCION) AS descripcion
                FROM CxC_Conceptos
                ORDER BY COD_CONCEPTO";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Consulta los contratos asignados a un concepto específico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <returns>Lista de contratos asignados.</returns>
        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_ContratosAsignados(int codEmpresa, string codConcepto)
        {
            var query = @"
                SELECT Cnt.Cod_Contrato AS Codigo,
                       Cnt.Descripcion,
                       Asg.registro_Fecha,
                       Asg.Registro_Usuario
                FROM CxC_Contratos Cnt
                LEFT JOIN CXC_CONCEPTOS_CONTRATOS Asg
                  ON Cnt.cod_contrato = Asg.cod_contrato
                 AND Asg.cod_Concepto = @CodConcepto
                ORDER BY Cnt.Cod_Contrato";
            return DbHelper.ExecuteListQuery<CxcConceptoAsignacionDto>(_portalDb, codEmpresa, query, new { CodConcepto = codConcepto });
        }

        /// <summary>
        /// Consulta los estados de factura asignados a un concepto específico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <returns>Lista de estados de factura asignados.</returns>
        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_FacturaEstadosAsignados(int codEmpresa, string codConcepto)
        {
            var query = @"
                SELECT Cnt.FACTURA_ESTADO AS Codigo,
                       Cnt.Descripcion,
                       Asg.registro_Fecha,
                       Asg.Registro_Usuario
                FROM CXC_FACTURAS_ESTADOS Cnt
                LEFT JOIN CXC_CONCEPTOS_FACTURA_ESTADO Asg
                  ON Cnt.FACTURA_ESTADO = Asg.FACTURA_ESTADO
                 AND Asg.cod_Concepto = @CodConcepto
                ORDER BY Cnt.FACTURA_ESTADO";
            return DbHelper.ExecuteListQuery<CxcConceptoAsignacionDto>(_portalDb, codEmpresa, query, new { CodConcepto = codConcepto });
        }

        /// <summary>
        /// Inserta una relación entre un concepto y un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la relación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_Contrato_Insertar(int codEmpresa, CxcConceptoContratoParams param)
        {
            var sql = @"
                INSERT INTO CxC_Conceptos_Contratos
                    (cod_contrato, cod_concepto, registro_usuario, registro_fecha)
                VALUES
                    (@Cod_Contrato, @Cod_Concepto, @Usuario, dbo.MyGetdate())";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina una relación entre un concepto y un contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la relación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_Contrato_Eliminar(int codEmpresa, CxcConceptoContratoParams param)
        {
            var sql = @"
                DELETE FROM CxC_Conceptos_Contratos
                WHERE cod_contrato = @Cod_Contrato
                  AND cod_concepto = @Cod_Concepto";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Inserta una relación entre un concepto y un estado de factura.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la relación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_FacturaEstado_Insertar(int codEmpresa, CxcConceptoFacturaEstadoParams param)
        {
            var sql = @"
                INSERT INTO CXC_CONCEPTOS_FACTURA_ESTADO
                    (factura_estado, cod_concepto, registro_usuario, registro_fecha)
                VALUES
                    (@Factura_Estado, @Cod_Concepto, @Usuario, dbo.MyGetdate())";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina una relación entre un concepto y un estado de factura.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la relación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_FacturaEstado_Eliminar(int codEmpresa, CxcConceptoFacturaEstadoParams param)
        {
            var sql = @"
                DELETE FROM CXC_CONCEPTOS_FACTURA_ESTADO
                WHERE factura_estado = @Factura_Estado
                  AND cod_concepto = @Cod_Concepto";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Consulta los pagadores disponibles en el sistema.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de pagadores.</returns>
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Pagadores(int codEmpresa)
        {
            var query = @"SELECT cedula AS Cedula, nombre AS Nombre
                          FROM CxC_Personas
                          WHERE Rol_Pagador = 1
                          ORDER BY cedula";
            return DbHelper.ExecuteListQuery<CxcPersonaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Actualiza el pagador por defecto de un concepto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con el nuevo pagador por defecto.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_ActualizarPagadorDefault(int codEmpresa, CxcConceptoPagadorDefaultParams param)
        {
            const string sql = @"
                UPDATE CxC_Conceptos
                SET Pagador_Default = @Pagador_Default,
                    Modifica_Usuario = @Usuario,
                    Modifica_Fecha = GETDATE()
                WHERE Cod_Concepto = @Cod_Concepto";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Consulta las unidades asignadas a una contabilidad específica.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de unidades asignadas.</returns>
        public ErrorDto<List<UnidadDto>> Unidades_ListaPorContabilidad(int codEmpresa, string codContabilidad)
        {
            var query = @"
                SELECT cod_unidad AS Unidad, Descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = @CodContabilidad
                ORDER BY cod_unidad";
            return DbHelper.ExecuteListQuery<UnidadDto>(_portalDb, codEmpresa, query, new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Consulta los centros de costo asignados a una contabilidad específica.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de centros de costo asignados.</returns>
        public ErrorDto<List<CentrosCostoDto>> CentrosCosto_ListaPorContabilidad(int codEmpresa, string codContabilidad)
        {
            var query = @"
                SELECT cod_centro_Costo AS Centro, Descripcion
                FROM cntx_centro_costos
                WHERE cod_contabilidad = @CodContabilidad
                  AND Activo = 1
                ORDER BY cod_centro_Costo";
            return DbHelper.ExecuteListQuery<CentrosCostoDto>(_portalDb, codEmpresa, query, new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Marca un concepto como incobrable o rehabilita un concepto cobrable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para la actualización.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcConceptos_Incobrable(int codEmpresa, CxcConceptoIncobrableParams param)
        {
            const string sp = "spCxC_Concepto_Incobrable";
            var parameters = new
            {
                Concepto = param.Cod_Concepto,
                Indicador = param.Indicador,
                Usuario = param.Usuario,
                Unidad = param.Cod_Unidad,
                CentroCosto = param.Cod_Centro_Costo,
                CtaDeterioro = param.Cta_Deterioro,
                CtaEstimacion = param.Cta_Estimacion,
                CtaIngreso = param.Cta_Ingreso,
                CtaOrdenDebe = param.Cta_Orden_Debe,
                CtaOrdenHaber = param.Cta_Orden_Haber
            };

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sp, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }
    }
}
