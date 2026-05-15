using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using System.Collections.Generic;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConsolidaMapeoCuentasBL
    {
        private readonly FrmCntXConsolidaMapeoCuentasDB _db;

        public FrmCntXConsolidaMapeoCuentasBL(IConfiguration config)
        {
            _db = new FrmCntXConsolidaMapeoCuentasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel?>> ConsolidaMapeoCuentas_ObtenerUnidades(int codEmpresa, int mContabilidad)
            => _db.ConsolidaMapeoCuentas_ObtenerUnidades(codEmpresa, mContabilidad);

        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_ImportaCargado(int codEmpresa, ConsolidaMapeoImportaCargadoRequestDto request)
            => _db.ConsolidaMapeoCuentas_ImportaCargado(codEmpresa, request);

        public ErrorDto<bool> ConsolidaMapeoCuentas_ImportaMapeo(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
            => _db.ConsolidaMapeoCuentas_ImportaMapeo(codEmpresa, Consolidadora, Unidad, Usuario);

        public ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>> ConsolidaMapeoCuentas_ImportaResultados(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
            => _db.ConsolidaMapeoCuentas_ImportaResultados(codEmpresa, Consolidadora, Unidad, Usuario);

        public ErrorDto<ConsolidaMapeoImportaValidaDto?> ConsolidaMapeoCuentas_ImportaValida(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
            => _db.ConsolidaMapeoCuentas_ImportaValida(codEmpresa, Consolidadora, Unidad, Usuario);

        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_Importa(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
            => _db.ConsolidaMapeoCuentas_Importa(codEmpresa, Consolidadora, Unidad, Usuario);

        public ErrorDto<ConsolidaMapeoImportaResultDto?> ConsolidaMapeoCuentas_Inicializa(int codEmpresa, int Consolidadora, string Unidad, string Usuario)
            => _db.ConsolidaMapeoCuentas_Inicializa(codEmpresa, Consolidadora, Unidad, Usuario);

        public ErrorDto<List<ConsolidaMapeoActualDto?>> ConsolidaMapeoCuentas_Actual(int codEmpresa, int Consolidadora, string Unidad)
            => _db.ConsolidaMapeoCuentas_Actual(codEmpresa, Consolidadora, Unidad);

        public ErrorDto<ConsolidaContabilidadDto?> ConsolidaMapeoCuentas_ContabilidadInfo(int codEmpresa, int mContabilidad)
            => _db.ConsolidaMapeoCuentas_ContabilidadInfo(codEmpresa, mContabilidad);

        public ErrorDto<bool> ConsolidaMapeoCuentas_ImportaContaBaseMapeo(int codEmpresa, int Consolidadora, string Usuario)
            => _db.ConsolidaMapeoCuentas_ImportaContaBaseMapeo(codEmpresa, Consolidadora, Usuario);
    }
}
