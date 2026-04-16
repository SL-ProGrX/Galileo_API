using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasEfectivoDenominacionesBL
    {
        private readonly FrmCajasEfectivosDenominacionesDB _db;
        public FrmCajasEfectivoDenominacionesBL(IConfiguration config)
        {
            _db = new FrmCajasEfectivosDenominacionesDB(config);
        }

        public ErrorDto<List<CajasEfectivosDenominacionesData>> Cajas_EfectivosDenominaciones_Obtener(int CodEmpresa, string cod_divisa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Cajas_EfectivosDenominaciones_Obtener(CodEmpresa, cod_divisa, filtros);
        }

        public ErrorDto Cajas_EfectivosDenominaciones_Guardar(int CodEmpresa, string usuario, CajasEfectivosDenominacionesData denominacion)
        {
            return _db.Cajas_EfectivosDenominaciones_Guardar(CodEmpresa, usuario, denominacion);
        }

        public ErrorDto Cajas_EfectivosDenominaciones_Eliminar(int CodEmpresa, string usuario, string cod_divisa, decimal denominacion)
        {
            return _db.Cajas_EfectivosDenominaciones_Eliminar(CodEmpresa, usuario, cod_divisa, denominacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EfectivosDenominaciones_Divisas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _db.Cajas_EfectivosDenominaciones_Divisas_Obtener(CodEmpresa, codContabilidad);
        }
    }
}