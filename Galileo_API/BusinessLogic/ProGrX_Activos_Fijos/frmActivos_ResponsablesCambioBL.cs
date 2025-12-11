using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosResponsablesCambioBL
    {
        private readonly FrmActivosResponsablesCambioDB _db;

        public FrmActivosResponsablesCambioBL(IConfiguration config)
        {
            _db = new FrmActivosResponsablesCambioDB(config);
        }
        public ErrorDto<ActivosResponsablesCambioBoletaLista> Activos_ResponsablesCambio_Boletas_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_ResponsablesCambio_Boletas_Lista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Export(
            int CodEmpresa, string cod_traslado, string identificacion, string usuario)
        {
            return _db.Activos_ResponsablesCambio_Placas_Export(CodEmpresa, cod_traslado, identificacion, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Motivos_Obtener(int CodEmpresa)
        {
            return _db.Activos_ResponsablesCambio_Motivos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Personas_Buscar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_ResponsablesCambio_Personas_Buscar(CodEmpresa, filtros);
        }
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Obtener(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Obtener(CodEmpresa, cod_traslado, usuario);
        }
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Obtener(int CodEmpresa,string? cod_traslado,string identificacion,string usuario)
        {
            return _db.Activos_ResponsablesCambio_Placas_Obtener(CodEmpresa, cod_traslado, identificacion, usuario);
        }
        public ErrorDto Activos_ResponsablesCambio_Boleta_Existe_Obtener(int CodEmpresa, string cod_traslado)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Existe_Obtener(CodEmpresa, cod_traslado);
        }
        public ErrorDto<ActivosResponsablesCambioBoletaResult> Activos_ResponsablesCambio_Boleta_Guardar(int CodEmpresa, ActivosResponsablesCambioBoleta boleta)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Guardar(CodEmpresa, boleta);
        }

        public ErrorDto Activos_ResponsablesCambio_Boleta_Placa_Guardar(int CodEmpresa, ActivosResponsablesCambioPlacaGuardarRequest data)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Placa_Guardar(CodEmpresa, data);
        }
        public ErrorDto Activos_ResponsablesCambio_Boleta_Procesar(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Procesar(CodEmpresa, cod_traslado, usuario);
        }
        public ErrorDto Activos_ResponsablesCambio_Boleta_Descartar(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Descartar(CodEmpresa, cod_traslado, usuario);
        }
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Scroll(int CodEmpresa, int scroll, string? cod_traslado, string usuario)
        {
            return _db.Activos_ResponsablesCambio_Boleta_Scroll(CodEmpresa, scroll, cod_traslado, usuario);
        }
        public ErrorDto<ActivosResponsablesPersona> Activos_ResponsablesCambio_Persona_Obtener(int CodEmpresa, string identificacion)
        {
            return _db.Activos_ResponsablesCambio_Persona_Obtener(CodEmpresa, identificacion);
        }
    }
}
