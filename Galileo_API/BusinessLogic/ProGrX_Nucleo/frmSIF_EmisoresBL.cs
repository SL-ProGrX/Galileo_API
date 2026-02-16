using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.DataBaseTier.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifEmisoresBL(IConfiguration config)
    {
        private readonly FrmSifEmisoresDB _db = new FrmSifEmisoresDB(config);

        public ErrorDto<SifEmisoresLista> SIF_EmisoresLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_EmisoresLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SifEmisoresData>> SIF_Emisores_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_Emisores_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto SIF_Emisores_Guardar(int CodEmpresa, string usuario, SifEmisoresData emisor)
        {
            return _db.SIF_Emisores_Guardar(CodEmpresa, usuario, emisor);
        }

        public ErrorDto SIF_Emisores_Valida(int CodEmpresa, SifEmisoresData emisor)
        {
            return _db.SIF_Emisores_Valida(CodEmpresa, emisor);
        }

        public ErrorDto SIF_Emisores_Eliminar(int CodEmpresa, string usuario, string cod_emisor)
        {
            return _db.SIF_Emisores_Eliminar(CodEmpresa, usuario, cod_emisor);
        }

        public ErrorDto<List<SifTarjetasAsignadasData>> SIF_EmisoresTarjetas_Obtener(int CodEmpresa, string cod_emisor)
        {
            return _db.SIF_EmisoresTarjetas_Obtener(CodEmpresa, cod_emisor);
        }

        public ErrorDto SIF_EmisorTarjeta_Asignar(int CodEmpresa, string usuario, SifEmisorTarjetaData asignacion)
        {
            return _db.SIF_EmisorTarjeta_Asignar(CodEmpresa, usuario, asignacion);
        }

        public ErrorDto SIF_EmisorTarjeta_Desasignar(int CodEmpresa, string usuario, SifEmisorTarjetaData asignacion)
        {
            return _db.SIF_EmisorTarjeta_Desasignar(CodEmpresa, usuario, asignacion);
        }
    }
}