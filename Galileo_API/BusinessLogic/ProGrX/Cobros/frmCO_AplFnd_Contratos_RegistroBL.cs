using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplFndContratosRegistroBL
    {
        private readonly FrmCOAplFndContratosRegistroDB _db;

        public FrmCOAplFndContratosRegistroBL(IConfiguration config)
        {
            _db = new FrmCOAplFndContratosRegistroDB(config);
        }

        public ErrorDto<List<CoAplFndContratosRegistroListaRow>> CO_AplFnd_Contratos_Registro_Lista_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplFndContratosRegistroListaRequest>(request)
                          ?? new CoAplFndContratosRegistroListaRequest();

            return _db.CO_AplFnd_Contratos_Registro_Lista_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CoAplFndContratosRegistroData> CO_AplFnd_Contratos_Registro_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplFndContratosRegistroConsultaRequest>(request)
                          ?? new CoAplFndContratosRegistroConsultaRequest();

            return _db.CO_AplFnd_Contratos_Registro_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<List<CoAplFndContratosRegistroCreditoRow>> CO_AplFnd_Contratos_Registro_Creditos_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplFndContratosRegistroCreditosRequest>(request)
                          ?? new CoAplFndContratosRegistroCreditosRequest();

            return _db.CO_AplFnd_Contratos_Registro_Creditos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CoAplFndContratosRegistroGuardarResponse> CO_AplFnd_Contratos_Registro_Guardar(
            int codEmpresa,
            CoAplFndContratosRegistroGuardarRequest request)
        {
            request ??= new CoAplFndContratosRegistroGuardarRequest();
            return _db.CO_AplFnd_Contratos_Registro_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<CoAplFndContratosRegistroPersonaF4Row>> CO_AplFnd_Contratos_Registro_Personas_F4_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplFndContratosRegistroPersonaF4Request>(request)
                          ?? new CoAplFndContratosRegistroPersonaF4Request();

            return _db.CO_AplFnd_Contratos_Registro_Personas_F4_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CoAplFndContratosRegistroCargaLoteResponse> CO_AplFnd_Contratos_Registro_Carga_Lote(
    int codEmpresa,
    CoAplFndContratosRegistroCargaLoteRequest request)
        {
            request ??= new CoAplFndContratosRegistroCargaLoteRequest();
            return _db.CO_AplFnd_Contratos_Registro_Carga_Lote(codEmpresa, request);
        }
    }
}
