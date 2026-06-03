using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAfNoticaNoCotizantesBl
    {
        private readonly FrmAfNoticaNoCotizantesDb _db;

        public FrmAfNoticaNoCotizantesBl(IConfiguration config)
        {
            _db = new FrmAfNoticaNoCotizantesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Instituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_NoticaNoCotizantes_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Rangos_Obtener(int CodEmpresa)
        {
            return _db.AF_NoticaNoCotizantes_Rangos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfAsociadosSinAportesDto>> AF_NoticaNoCotizantes_Consulta_Obtener(int CodEmpresa, string Filtro)
        {
            AfNoticaNoCotizantesFiltros filtros = System.Text.Json.JsonSerializer.Deserialize<AfNoticaNoCotizantesFiltros>(Filtro) ?? new AfNoticaNoCotizantesFiltros();
            return _db.AF_NoticaNoCotizantes_Consulta_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_NoticaNoCotizantes_Estadistica_Actualizar(int CodEmpresa)
        {
            return _db.AF_NoticaNoCotizantes_Estadistica_Actualizar(CodEmpresa);
        }

        public ErrorDto AF_NoticaNoCotizantes_Asociados_Notificar(int CodEmpresa, List<AfAsociadosSinAportesDto> Lista, int Aviso, string Usuario)
        {
            return _db.AF_NoticaNoCotizantes_Asociados_Notificar(CodEmpresa, Lista, Aviso, Usuario);
        }
    }
}
