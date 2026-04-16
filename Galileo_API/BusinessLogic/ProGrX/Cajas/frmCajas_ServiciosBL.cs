using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasServiciosBL
    {
        private readonly FrmCajasServiciosDB _db;

        public FrmCajasServiciosBL(IConfiguration config)
        {
            _db = new FrmCajasServiciosDB(config);
        }
        public ErrorDto<CajasServiciosConceptosLista>Cajas_Servicios_Conceptos_Lista_Obtener(int CodEmpresa, string cod_recaudador, string jfiltros)
        {
            FiltrosLazyLoadData filtros = string.IsNullOrEmpty(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)!;
            return _db.Cajas_Servicios_Conceptos_Lista_Obtener(CodEmpresa, cod_recaudador, filtros);
        }

        public ErrorDto<CajasServiciosConceptosData>Cajas_Servicios_Conceptos_Scroll(int CodEmpresa, string cod_recaudador, int scroll, string cod_servicio)
        {
            return _db.Cajas_Servicios_Conceptos_Scroll(CodEmpresa, cod_recaudador, scroll, cod_servicio);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_Servicios_Conceptos_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_Servicios_Conceptos_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Servicios_Recaudadores_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_Servicios_Recaudadores_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<CajasServiciosConceptosData>Cajas_Servicios_Conceptos_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _db.Cajas_Servicios_Conceptos_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        public ErrorDto<CajasServiciosCabysLista> Cajas_Servicios_Cabys_Lista_Obtener(int CodEmpresa, string? jfiltros)
        {
            FiltrosLazyLoadData filtros = string.IsNullOrEmpty(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)!;
            return _db.Cajas_Servicios_Cabys_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Cajas_Servicios_Conceptos_Existe_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _db.Cajas_Servicios_Conceptos_Existe_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        public ErrorDto Cajas_Servicios_Conceptos_Guardar(int CodEmpresa, string usuario, CajasServiciosConceptosData servicio)
        {
            return _db.Cajas_Servicios_Conceptos_Guardar(CodEmpresa, usuario, servicio);
        }
        public ErrorDto<List<CajasServiciosComisionesData>>Cajas_Servicios_Comisiones_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _db.Cajas_Servicios_Comisiones_Lista_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        public ErrorDto Cajas_Servicios_Comisiones_Guardar(int CodEmpresa, string usuario, CajasServiciosComisionesData rango)
        {
            return _db.Cajas_Servicios_Comisiones_Guardar(CodEmpresa, usuario, rango);
        }

        public ErrorDto Cajas_Servicios_Comisiones_Eliminar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, int linea)
        {
            return _db.Cajas_Servicios_Comisiones_Eliminar(CodEmpresa, usuario, cod_recaudador, cod_servicio, linea);
        }

        public ErrorDto<List<CajasServiciosCajasVinculadasData>>Cajas_Servicios_CajasVinculadas_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            return _db.Cajas_Servicios_CajasVinculadas_Lista_Obtener(CodEmpresa, cod_recaudador, cod_servicio);
        }

        public ErrorDto Cajas_Servicios_CajasVinculadas_Guardar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, string cod_caja, short asignada)
        {
            return _db.Cajas_Servicios_CajasVinculadas_Guardar(CodEmpresa, usuario, cod_recaudador, cod_servicio, cod_caja, asignada);
        }
    }
}