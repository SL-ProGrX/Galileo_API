using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndGruposOperativosBl
    {
        private readonly FrmFndGruposOperativosDb _db;

        public FrmFndGruposOperativosBl(IConfiguration config)
        {
            _db = new FrmFndGruposOperativosDb(config);
        }

        public ErrorDto<FndGruposOperativosLista> Fnd_GruposOperativos_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<Models.FiltrosLazyLoadData>(jfiltros) ?? new Models.FiltrosLazyLoadData();
            return _db.Fnd_GruposOperativos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<FndGrupoOperativoModel>> Fnd_GruposOperativos_Obtener(int CodEmpresa)
        {
            return _db.Fnd_GruposOperativos_Obtener(CodEmpresa);
        }

        public ErrorDto<FndGrupoOperativoValidaResult> Fnd_GruposOperativos_Valida(int CodEmpresa, string grupoCodigo)
        {
            return _db.Fnd_GruposOperativos_Valida(CodEmpresa, grupoCodigo);
        }

        public ErrorDto Fnd_GruposOperativos_Guardar(int CodEmpresa, FndGrupoOperativoModel grupo)
        {
            return _db.Fnd_GruposOperativos_Guardar(CodEmpresa, grupo);
        }

        public ErrorDto Fnd_GruposOperativos_Eliminar(int CodEmpresa, string grupoCodigo, string usuario)
        {
            return _db.Fnd_GruposOperativos_Eliminar(CodEmpresa, grupoCodigo, usuario);
        }

        public ErrorDto<List<FndGrupoOperativoPlanResult>> Fnd_GruposOperativos_Planes_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            return _db.Fnd_GruposOperativos_Planes_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<FndGrupoOperativoUsuarioResult>> Fnd_GruposOperativos_Usuarios_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            return _db.Fnd_GruposOperativos_Usuarios_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<FndGrupoOperativoConceptoResult>> Fnd_GruposOperativos_Conceptos_Obtener(int CodEmpresa, FndGrupoOperativoFiltroRequest request)
        {
            return _db.Fnd_GruposOperativos_Conceptos_Obtener(CodEmpresa, request);
        }

        public ErrorDto Fnd_GruposOperativos_AsignarPlan(int CodEmpresa, FndGrupoOperativoAsignarPlanRequest request)
        {
            return _db.Fnd_GruposOperativos_AsignarPlan(CodEmpresa, request);
        }

        public ErrorDto Fnd_GruposOperativos_AsignarUsuario(int CodEmpresa, FndGrupoOperativoAsignarUsuarioRequest request)
        {
            return _db.Fnd_GruposOperativos_AsignarUsuario(CodEmpresa, request);
        }

        public ErrorDto Fnd_GruposOperativos_AsignarConcepto(int CodEmpresa, FndGrupoOperativoAsignarConceptoRequest request)
        {
            return _db.Fnd_GruposOperativos_AsignarConcepto(CodEmpresa, request);
        }
    }
}