using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasRecaudadoresBL
    {
        private readonly FrmCajasRecaudadoresDB _db;

        public FrmCajasRecaudadoresBL(IConfiguration config)
        {
            _db = new FrmCajasRecaudadoresDB(config);
        }

        public ErrorDto<CajasRecaudadoresLista> Cajas_Recaudadores_Lista_Obtener(int CodEmpresa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Cajas_Recaudadores_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Scroll(int CodEmpresa,int cod_contabilidad,int scroll,string? cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Scroll(CodEmpresa, cod_contabilidad, scroll, cod_recaudador);
        }

        public ErrorDto<CajasRecaudadorData> Cajas_Recaudadores_Obtener(int CodEmpresa,int cod_contabilidad,string cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Obtener(CodEmpresa, cod_contabilidad, cod_recaudador);
        }

        public ErrorDto Cajas_Recaudadores_Existe_Obtener( int CodEmpresa,string cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Existe_Obtener(CodEmpresa, cod_recaudador);
        }

        public ErrorDto Cajas_Recaudadores_Guardar(int CodEmpresa,string usuario,CajasRecaudadorData recaudador)
        {
            return _db.Cajas_Recaudadores_Guardar(CodEmpresa, usuario, recaudador);
        }

        public ErrorDto Cajas_Recaudadores_Eliminar(int CodEmpresa,string usuario,string cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Eliminar(CodEmpresa, usuario, cod_recaudador);
        }
        public ErrorDto<List<CajasRecaudadorContactoData>> Cajas_Recaudadores_Contactos_Lista_Obtener(int CodEmpresa,string cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Contactos_Lista_Obtener(CodEmpresa, cod_recaudador);
        }

        public ErrorDto Cajas_Recaudadores_Contactos_Guardar(int CodEmpresa,string usuario,CajasRecaudadorContactoData contacto)
        {
            return _db.Cajas_Recaudadores_Contactos_Guardar(CodEmpresa, usuario, contacto);
        }

        public ErrorDto Cajas_Recaudadores_Contactos_Eliminar(int CodEmpresa,string usuario, string cod_recaudador,int linea)
        {
            return _db.Cajas_Recaudadores_Contactos_Eliminar(CodEmpresa, usuario, cod_recaudador, linea);
        }

        public ErrorDto<List<CajasRecaudadorServicioItem>> Cajas_Recaudadores_Servicios_Lista_Obtener( int CodEmpresa,string cod_recaudador)
        {
            return _db.Cajas_Recaudadores_Servicios_Lista_Obtener(CodEmpresa, cod_recaudador);
        }

        public ErrorDto<List<CajasServiciosCajasVinculadasData>> Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener( int CodEmpresa,string cod_recaudador,string cod_servicio)
        {
            return _db.Cajas_Recaudadores_Servicios_CajasVinculadas_Lista_Obtener(CodEmpresa,cod_recaudador,cod_servicio);
        }

        public ErrorDto Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar(int CodEmpresa,string usuario,string cod_recaudador,string cod_servicio,string cod_caja,short asignada)
        {
            return _db.Cajas_Recaudadores_Servicios_CajasVinculadas_Guardar(CodEmpresa,usuario, cod_recaudador,cod_servicio,cod_caja,asignada);
        }
    }
}