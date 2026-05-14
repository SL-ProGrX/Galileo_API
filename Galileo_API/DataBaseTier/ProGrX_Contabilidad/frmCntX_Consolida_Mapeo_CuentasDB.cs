using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using System.Collections.Generic;
using Galileo.Models.Security;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConsolidaMapeoCuentasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmCntXConsolidaMapeoCuentasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle)
        {
            _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Aplica - Web",
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Obtiene la lista de unidades activas para una contabilidad, usando DropDownListaGenericaModel.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel?>> ConsolidaMapeoCuentas_ObtenerUnidades(int codEmpresa, int mContabilidad)
        {
            var sql = @"select Cod_Unidad as item, Descripcion as descripcion from CntX_Unidades where cod_Contabilidad = @mContabilidad and Activa = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel?>(_portalDb, codEmpresa, sql, new { mContabilidad });
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Importa_Cargado para importar mapeo de cuentas.
        /// </summary>
        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_ImportaCargado(int codEmpresa, ConsolidaMapeoImportaCargadoRequestDto request)
        {
            var sql = "spCntX_Consolida_Mapeo_Importa_Cargado";
            var parametros = new {
                request.Consolidadora,
                request.Unidad,
                request.Cuenta,
                request.CtaConsolida,
                request.Descripcion,
                request.Usuario,
                request.Linea
            };
            return DbHelper.ExecuteSingleQuery<ConsolidaMapeoImportaResultDto?>(_portalDb, codEmpresa, sql, default, parametros);
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Importa_Mapeo para registrar y validar mapeo de cuentas.
        /// </summary>
        public ErrorDto<bool> ConsolidaMapeoCuentas_ImportaMapeo(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
        {
            var sql = "spCntX_Consolida_Mapeo_Importa_Mapeo"; 
            var parametros = new { Consolidadora, Unidad, Usuario };
            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Execute(sql, parametros, commandType: System.Data.CommandType.StoredProcedure);
                return new ErrorDto<bool> { Result = true, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Importa_Resultados para consultar resultados del mapeo.
        /// </summary>
        public ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>> ConsolidaMapeoCuentas_ImportaResultados(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
        {
            var sql = "spCntX_Consolida_Mapeo_Importa_Resultados";
            var parametros = new { Consolidadora, Unidad, Usuario };
            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                var result = conn.Query<ConsolidaMapeoImportaResultadoDto?>(sql, parametros, commandType: System.Data.CommandType.StoredProcedure).AsList();
                return new ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>> { Result = null, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Importa_Valida para validar casos erróneos.
        /// </summary>
        public ErrorDto<ConsolidaMapeoImportaValidaDto?> ConsolidaMapeoCuentas_ImportaValida(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
        {
            var sql = "spCntX_Consolida_Mapeo_Importa_Valida";
            var parametros = new { Consolidadora, Unidad, Usuario };
            return DbHelper.ExecuteSingleQuery<ConsolidaMapeoImportaValidaDto?>(_portalDb, codEmpresa, sql, default, parametros);
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Importa para importar el catálogo consolidado.
        /// </summary>
        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_Importa(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
        {
            var sql = "spCntX_Consolida_Mapeo_Importa";
            var parametros = new { Consolidadora, Unidad, Usuario };
            var result = DbHelper.ExecuteSingleQuery<ConsolidaMapeoImportaResultDto?>(_portalDb, codEmpresa, sql, default, parametros);
            if (result.Result != null && result.Result.Pass == 1)
            {
                RegistrarBitacora(codEmpresa, Usuario, $"Importación del Mapeo de Cuentas de la Contabilidad Id: [{Consolidadora}]  Unidad: {Unidad}");
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Inicializa para inicializar el mapeo de la unidad.
        /// </summary>
        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_Inicializa(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
        {
            var sql = "spCntX_Consolida_Mapeo_Inicializa";
            var parametros = new { Consolidadora, Unidad, Usuario };
            var result = DbHelper.ExecuteSingleQuery<ConsolidaMapeoImportaResultDto?>(_portalDb, codEmpresa, sql, default, parametros);
            if (result.Result != null && result.Result.Pass == 1)
            {
                RegistrarBitacora(codEmpresa, Usuario, $"Inicialización del Mapeo de Cuentas de la Contabilidad Id: [{Consolidadora}]  , Unidad: {Unidad}");
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Mapeo_Actual para consultar el mapeo actual de la unidad.
        /// </summary>
        public ErrorDto<List<ConsolidaMapeoActualDto?>> ConsolidaMapeoCuentas_Actual(int codEmpresa, int Consolidadora, string Unidad)
        {
            var sql = "spCntX_Consolida_Mapeo_Actual";
            var parametros = new { Consolidadora, Unidad };
            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                var result = conn.Query<ConsolidaMapeoActualDto?>(sql, parametros, commandType: System.Data.CommandType.StoredProcedure).AsList();
                return new ErrorDto<List<ConsolidaMapeoActualDto?>> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<List<ConsolidaMapeoActualDto?>> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<List<ConsolidaMapeoActualDto?>> { Result = null, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Obtiene información de consolidación de la contabilidad.
        /// </summary>
        public ErrorDto<ConsolidaContabilidadDto?> ConsolidaMapeoCuentas_ContabilidadInfo(int codEmpresa, int mContabilidad)
        {
            var sql = @"select isnull(I_CONSOLIDADORA, 0) as Consolida_Ind, isnull(CONSOLIDA_CONTA_BASE, 0) as Consolida_Conta, isnull(CONSOLIDA_UNIDAD_BASE, '') as Consolida_Unidad from CntX_Contabilidades where cod_contabilidad = @mContabilidad";
            return DbHelper.ExecuteSingleQuery<ConsolidaContabilidadDto?>(_portalDb, codEmpresa, sql, default, new { mContabilidad });
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Consolida_Importa_Conta_Base_Mapeo para importar y mapear la contabilidad base.
        /// </summary>
        public ErrorDto<bool> ConsolidaMapeoCuentas_ImportaContaBaseMapeo(int codEmpresa, int Consolidadora, string Usuario)
        {
            var sql = "spCntX_Consolida_Importa_Conta_Base_Mapeo";
            var parametros = new { Consolidadora, Usuario };
            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Execute(sql, parametros, commandType: System.Data.CommandType.StoredProcedure);
                RegistrarBitacora(codEmpresa, Usuario, $"Importación del Mapeo de Cuentas de la Contabilidad Base de: {Consolidadora}");
                return new ErrorDto<bool> { Result = true, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
        }
    }
}
