using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizaConsultaBL
    {
        private readonly FrmCRPolizaConsultaDB _db;

        public FrmCRPolizaConsultaBL(IConfiguration config)
        {
            _db = new FrmCRPolizaConsultaDB(config);
        }

        public ErrorDto<List<PolizaPersonaFiltroDto>> Poliza_Persona_Filtros_Lista(int codEmpresa, PolizaPersonaFiltroParams param)
            => _db.Poliza_Persona_Filtros_Lista(codEmpresa, param);

        public ErrorDto<List<PolizaPersonaCreditoDto>> Poliza_Persona_Creditos(int codEmpresa, PolizaPersonaCreditoParams param)
            => _db.Poliza_Persona_Creditos(codEmpresa, param);

        public ErrorDto<List<PolizaPersonaOperacionPolizaDto>> Poliza_Persona_Operaciones_Polizas(int codEmpresa, PolizaPersonaOperacionPolizaParams param)
           => _db.Poliza_Persona_Operaciones_Polizas(codEmpresa, param);

        public ErrorDto<List<PolizaPersonaReclamoDto>> Poliza_Persona_Reclamos(int codEmpresa, PolizaPersonaReclamoParams param)
            => _db.Poliza_Persona_Reclamos(codEmpresa, param);
    }
}
