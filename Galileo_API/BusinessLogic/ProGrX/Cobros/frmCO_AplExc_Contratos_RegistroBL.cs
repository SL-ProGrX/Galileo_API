using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros.Galileo_API.Models.ProGrX.Cobros;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplExcContratosRegistroBL
    {
        private readonly FrmCOAplExcContratosRegistroDB _db;

        public FrmCOAplExcContratosRegistroBL(IConfiguration config)
        {
            _db = new FrmCOAplExcContratosRegistroDB(config);
        }

        public ErrorDto<List<CoAplExcContratosRegistroListaRow>> CO_AplExc_Contratos_Registro_Lista_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplExcContratosRegistroListaRequest>(request)
                          ?? new CoAplExcContratosRegistroListaRequest();

            return _db.CO_AplExc_Contratos_Registro_Lista_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CoAplExcContratosRegistroData> CO_AplExc_Contratos_Registro_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplExcContratosRegistroConsultaRequest>(request)
                          ?? new CoAplExcContratosRegistroConsultaRequest();

            return _db.CO_AplExc_Contratos_Registro_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<List<CoAplExcContratosRegistroCreditoRow>> CO_AplExc_Contratos_Registro_Creditos_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoAplExcContratosRegistroCreditosRequest>(request)
                          ?? new CoAplExcContratosRegistroCreditosRequest();

            return _db.CO_AplExc_Contratos_Registro_Creditos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CoAplExcContratosRegistroGuardarResponse> CO_AplExc_Contratos_Registro_Guardar(
            int codEmpresa,
            CoAplExcContratosRegistroGuardarRequest request)
        {
            request ??= new CoAplExcContratosRegistroGuardarRequest();
            return _db.CO_AplExc_Contratos_Registro_Guardar(codEmpresa, request);
        }

    }
}
