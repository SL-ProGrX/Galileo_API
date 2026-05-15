using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndGarantiasBl
    {
        private readonly FrmFndGrantiasDb _db;

        public FrmFndGarantiasBl(IConfiguration config)
        {
            _db = new FrmFndGrantiasDb(config);
        }

        public ErrorDto<FndGarantiasLista> Fnd_GarantiasLista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<Models.FiltrosLazyLoadData>(jfiltros) ?? new Models.FiltrosLazyLoadData();
            return _db.Fnd_GarantiasLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<FndGarantiaModel>> Fnd_Garantias_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Garantias_Obtener(CodEmpresa);
        }

        public ErrorDto<FndGarantiaValidaResult> Fnd_Garantias_Valida(int CodEmpresa, string garantiaFND)
        {
            return _db.Fnd_Garantias_Valida(CodEmpresa, garantiaFND);
        }

        public ErrorDto Fnd_Garantias_Guardar(int CodEmpresa, FndGarantiaModel garantia)
        {
            return _db.Fnd_Garantias_Guardar(CodEmpresa, garantia);
        }

        public ErrorDto Fnd_Garantias_Eliminar(int CodEmpresa, string garantiaFND, string usuario)
        {
            return _db.Fnd_Garantias_Eliminar(CodEmpresa, garantiaFND, usuario);
        }

        public ErrorDto<List<FndGarantiaAhorrosConsultaResult>> Fnd_Garantia_Ahorros_Consulta(int CodEmpresa, FndGarantiaAhorrosConsultaRequest request)
        {
            return _db.Fnd_Garantia_Ahorros_Consulta(CodEmpresa, request);
        }

        public ErrorDto Fnd_Garantia_Ahorros_Registro(int CodEmpresa, FndGarantiaAhorrosRegistroRequest request)
        {
            return _db.Fnd_Garantia_Ahorros_Registro(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Garantias_Lista_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Garantias_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Lista_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Operadoras_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_EstadosPersona_Lista_Obtener(int CodEmpresa)
        {
            return _db.Fnd_EstadosPersona_Lista_Obtener(CodEmpresa);
        }
    }
}