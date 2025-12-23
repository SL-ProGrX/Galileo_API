using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosReasignacionesBL
    {
        private readonly FrmActivosReasignacionesDB _db;

        public FrmActivosReasignacionesBL(IConfiguration config)
        {
            _db = new FrmActivosReasignacionesDB(config);
        }
        public ErrorDto<string> Activos_Reasignacion_SiguienteBoleta_Obtener(int CodEmpresa)
        {
            return _db.Activos_Reasignacion_SiguienteBoleta_Obtener(CodEmpresa);
        }
        public ErrorDto<ActivosReasignacionesActivosLista> Activos_Reasignacion_Activos_Lista_Obtener(int CodEmpresa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Reasignacion_Activos_Lista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<ActivosReasignacionesActivo> Activos_Reasignacion_Activo_Obtener(int CodEmpresa,string numPlaca)
        {
            return _db.Activos_Reasignacion_Activo_Obtener(CodEmpresa, numPlaca);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Personas_Buscar(int CodEmpresa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Reasignacion_Personas_Buscar(CodEmpresa, filtros);
        }
        public ErrorDto<ActivosReasignacionesPersona> Activos_Reasignacion_Persona_Obtener(int CodEmpresa,string identificacion)
        {
            return _db.Activos_Reasignacion_Persona_Obtener(CodEmpresa, identificacion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Motivos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Reasignacion_Motivos_Obtener(CodEmpresa);
        }
        public ErrorDto<ActivosReasignacionesBoletaResult> Activos_Reasignacion_CambioResponsable(int CodEmpresa,ActivosReasignacionesCambioRequest data)
        {
            return _db.Activos_Reasignacion_CambioResponsable(CodEmpresa, data);
        }
        public ErrorDto<ActivosReasignacionesBoletaHistorialLista> Activos_Reasignacion_BoletasLista_Obtener(int CodEmpresa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<ActivosReasignacionesBoletasFiltros>(jfiltros) ?? new ActivosReasignacionesBoletasFiltros();
            return _db.Activos_Reasignacion_BoletasLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>> Activos_Reasignacion_Boletas_Export(int CodEmpresa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<ActivosReasignacionesBoletasFiltros>(jfiltros) ?? new ActivosReasignacionesBoletasFiltros();
            return _db.Activos_Reasignacion_Boletas_Export(CodEmpresa, filtros);
        }
        public ErrorDto<ActivosReasignacionesBoleta> Activos_Reasignacion_Obtener(int CodEmpresa,string cod_traslado)
        {
            return _db.Activos_Reasignacion_Obtener(CodEmpresa, cod_traslado);
        }
        public ErrorDto<object> Activos_Reasignacion_Boletas_Lote(int codEmpresa,ActivosReasignacionesBoletasLoteRequest request)
        {
            return _db.Activos_Reasignacion_Boletas_Lote(codEmpresa, request);
        }

    }
}
