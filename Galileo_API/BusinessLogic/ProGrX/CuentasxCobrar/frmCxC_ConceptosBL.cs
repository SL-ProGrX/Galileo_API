using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCConceptosBL
    {
        private readonly FrmCxCConceptosDB _db;

        public FrmCxCConceptosBL(IConfiguration config)
        {
            _db = new FrmCxCConceptosDB(config);
        }

        public ErrorDto<List<CxcConceptoDto>> CxcConceptos_Lista(int codEmpresa)
            => _db.CxcConceptos_Lista(codEmpresa);

        public ErrorDto<CxcConceptoExisteResult?> CxcConceptos_Existe(int codEmpresa, string codigo)
            => _db.CxcConceptos_Existe(codEmpresa, codigo);

        public ErrorDto<bool> CxcConceptos_Guardar(int codEmpresa, CxcConceptoSaveParams param)
            => _db.CxcConceptos_Guardar(codEmpresa, param);

        public ErrorDto<bool> CxcConceptos_Eliminar(int codEmpresa, CxcConceptoDeleteParams param)
            => _db.CxcConceptos_Eliminar(codEmpresa, param);

        public ErrorDto<List<DropDownListaGenericaModel>> CxcConceptos_ListaGenerica(int codEmpresa)
            => _db.CxcConceptos_ListaGenerica(codEmpresa);

        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_ContratosAsignados(int codEmpresa, string codConcepto)
            => _db.CxcConceptos_ContratosAsignados(codEmpresa, codConcepto);

        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_FacturaEstadosAsignados(int codEmpresa, string codConcepto)
            => _db.CxcConceptos_FacturaEstadosAsignados(codEmpresa, codConcepto);

        public ErrorDto<bool> CxcConceptos_Contrato_Insertar(int codEmpresa, CxcConceptoContratoParams param)
            => _db.CxcConceptos_Contrato_Insertar(codEmpresa, param);

        public ErrorDto<bool> CxcConceptos_Contrato_Eliminar(int codEmpresa, CxcConceptoContratoParams param)
            => _db.CxcConceptos_Contrato_Eliminar(codEmpresa, param);

        public ErrorDto<bool> CxcConceptos_FacturaEstado_Insertar(int codEmpresa, CxcConceptoFacturaEstadoParams param)
            => _db.CxcConceptos_FacturaEstado_Insertar(codEmpresa, param);

        public ErrorDto<bool> CxcConceptos_FacturaEstado_Eliminar(int codEmpresa, CxcConceptoFacturaEstadoParams param)
            => _db.CxcConceptos_FacturaEstado_Eliminar(codEmpresa, param);

        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Pagadores(int codEmpresa)
            => _db.CxcPersonas_Pagadores(codEmpresa);

        public ErrorDto<bool> CxcConceptos_ActualizarPagadorDefault(int codEmpresa, CxcConceptoPagadorDefaultParams param)
            => _db.CxcConceptos_ActualizarPagadorDefault(codEmpresa, param);

        public ErrorDto<List<UnidadDto>> Unidades_ListaPorContabilidad(int codEmpresa, string codContabilidad)
            => _db.Unidades_ListaPorContabilidad(codEmpresa, codContabilidad);

        public ErrorDto<List<CentrosCostoDto>> CentrosCosto_ListaPorContabilidad(int codEmpresa, string codContabilidad)
            => _db.CentrosCosto_ListaPorContabilidad(codEmpresa, codContabilidad);

        public ErrorDto<bool> CxcConceptos_Incobrable(int codEmpresa, CxcConceptoIncobrableParams param)
            => _db.CxcConceptos_Incobrable(codEmpresa, param);
    }
}
