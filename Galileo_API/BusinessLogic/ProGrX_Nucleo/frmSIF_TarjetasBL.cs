using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifTarjetasBL(IConfiguration config)
    {
        private readonly FrmSifTarjetasDB _db = new FrmSifTarjetasDB(config);

        public ErrorDto<SifTarjetasLista> SIF_TarjetasLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_TarjetasLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SifTarjetasData>> SIF_Tarjetas_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_Tarjetas_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto SIF_Tarjetas_Guardar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            return _db.SIF_Tarjetas_Guardar(CodEmpresa, usuario, tarjeta);
        }

        public ErrorDto SIF_Tarjetas_Eliminar(int CodEmpresa, string usuario, string cod_tarjeta)
        {
            return _db.SIF_Tarjetas_Eliminar(CodEmpresa, usuario, cod_tarjeta);
        }

        public ErrorDto SIF_Tarjetas_Valida(int CodEmpresa, SifTarjetasData tarjeta)
        {
            return _db.SIF_Tarjetas_Valida(CodEmpresa, tarjeta);
        }

        public ErrorDto<List<SifEmisoresAsignadosData>> SIF_TarjetasEmisores_Obtener(int CodEmpresa, string cod_tarjeta)
        {
            return _db.SIF_TarjetasEmisores_Obtener(CodEmpresa, cod_tarjeta);
        }
    }
}