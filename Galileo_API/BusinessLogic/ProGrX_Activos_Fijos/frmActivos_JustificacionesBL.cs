using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;


namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosJustificacionesBl
    {
        FrmActivosJustificacionesDb _db;

        public FrmActivosJustificacionesBl(IConfiguration config)
        {
            _db = new FrmActivosJustificacionesDb(config);
        }

        public ErrorDto<ActivosJustificacionesLista> Activos_JustificacionesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.Activos_JustificacionesLista_Obtener(CodEmpresa, filtros);
        }


        public ErrorDto Activos_JustificacionesExiste_Obtener(int CodEmpresa, string cod_justificacion)
        {
            return _db.Activos_JustificacionesExiste_Obtener(CodEmpresa, cod_justificacion);
        }

        public ErrorDto<ActivosJustificacionesData> Activos_Justificaciones_Obtener(int CodEmpresa, string cod_justificacion)
        {
            return _db.Activos_Justificaciones_Obtener(CodEmpresa, cod_justificacion);
        }


        public ErrorDto<ActivosJustificacionesData> Activos_Justificacion_Scroll(int CodEmpresa, int scroll, string? cod_justificacion)
        {
            return _db.Activos_Justificacion_Scroll(CodEmpresa, scroll, cod_justificacion);
        }

        public ErrorDto Activos_Justificaciones_Guardar(int CodEmpresa, ActivosJustificacionesData justificacionesData)
        {
            return _db.Activos_Justificaciones_Guardar(CodEmpresa, justificacionesData);
        }

        public ErrorDto Activos_Justificaciones_Eliminar(int CodEmpresa, string usuario, string cod_justificacion)
        {
            return _db.Activos_Justificaciones_Eliminar(CodEmpresa, usuario, cod_justificacion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTipos_Obtener(int CodEmpresa)
        {
            return _db.Activos_JustificacionesTipos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_JustificacionesTiposAsientos_Obtener(CodEmpresa, contabilidad);
        }

    }
}    
