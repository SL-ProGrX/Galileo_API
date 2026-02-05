using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosBL
    {
        private readonly FrmCxCContratosDB _db;

        public FrmCxCContratosBL(IConfiguration config)
        {
            _db = new FrmCxCContratosDB(config);
        }

        public ErrorDto<List<ContratoBusquedaDto>> Contratos_Busqueda_Lista(int codEmpresa)
            => _db.Contratos_Busqueda_Lista(codEmpresa);

        public ErrorDto<ContratoDetalleDto?> Contrato_ObtenerPorCodigo(int codEmpresa, string codContrato)
            => _db.Contrato_ObtenerPorCodigo(codEmpresa, codContrato);

        public ErrorDto<List<ContratoPersonaDto>> Contrato_PersonasPorContrato(int codEmpresa, string codContrato)
            => _db.Contrato_PersonasPorContrato(codEmpresa, codContrato);

        public ErrorDto<bool> Contrato_PersonaPagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
            => _db.Contrato_PersonaPagador_Eliminar(codEmpresa, param);

        public ErrorDto<bool> Contrato_PersonaSuscripcion_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
            => _db.Contrato_PersonaSuscripcion_Eliminar(codEmpresa, param);

        public ErrorDto<bool> Contrato_Persona_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
            => _db.Contrato_Persona_Eliminar(codEmpresa, param);

        public ErrorDto<List<ContratoPagadorDto>> Contrato_PagadoresPorContrato(int codEmpresa, string codContrato)
            => _db.Contrato_PagadoresPorContrato(codEmpresa, codContrato);

        public ErrorDto<bool> Contrato_Pagador_Eliminar(int codEmpresa, ContratoPersonaDeleteParams param)
            => _db.Contrato_Pagador_Eliminar(codEmpresa, param);

        public ErrorDto<List<ContratoCargoDto>> Contrato_CargosPorContrato(int codEmpresa, string codContrato)
            => _db.Contrato_CargosPorContrato(codEmpresa, codContrato);

        public ErrorDto<bool> Contrato_Cargo_Eliminar(int codEmpresa, ContratoCargoDeleteParams param)
            => _db.Contrato_Cargo_Eliminar(codEmpresa, param);

        public ErrorDto<List<ContratoConceptoDto>> Contrato_ConceptosPorContrato(int codEmpresa, string codContrato)
           => _db.Contrato_ConceptosPorContrato(codEmpresa, codContrato);

        public ErrorDto<bool> Contrato_Concepto_Insertar(int codEmpresa, ContratoConceptoParams param)
            => _db.Contrato_Concepto_Insertar(codEmpresa, param);

        public ErrorDto<bool> Contrato_Concepto_Eliminar(int codEmpresa, ContratoConceptoParams param)
            => _db.Contrato_Concepto_Eliminar(codEmpresa, param);
    }
}
