using Newtonsoft.Json;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndSeguridadNivelesBl
    {
        private readonly FrmFndSeguridadNivelesDb _db;

        public FrmFndSeguridadNivelesBl(IConfiguration config) => _db = new FrmFndSeguridadNivelesDb(config);

        public ErrorDto<TablasListaGenericaModel> Fnd_SegNiveles_Grupos_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Filtros) ?? new FiltrosLazyLoadData();
            return _db.Fnd_SegNiveles_Grupos_Obtener(CodEmpresa, Exporta, filtros);
        }

        public ErrorDto<List<FndSegNivelesPlanesData>> Fnd_SegNiveles_Planes_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            return _db.Fnd_SegNiveles_Planes_Obtener(CodEmpresa, CodGrupo, Filtro);
        }

        public ErrorDto<List<FndSegNivelesUsuariosData>> Fnd_SegNiveles_Usuarios_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            return _db.Fnd_SegNiveles_Usuarios_Obtener(CodEmpresa, CodGrupo, Filtro);
        }

        public ErrorDto Fnd_SegNiveles_Grupos_Guardar(int CodEmpresa, FndSegNivelesGrupoDto Data)
        {
            return _db.Fnd_SegNiveles_Grupos_Guardar(CodEmpresa, Data);
        }

        public ErrorDto Fnd_SegNiveles_Grupos_Eliminar(int CodEmpresa, string CodGrupo, string Usuario)
        {
            return _db.Fnd_SegNiveles_Grupos_Eliminar(CodEmpresa, CodGrupo, Usuario);
        }

        public ErrorDto Fnd_SegNiveles_Planes_Actualizar(int CodEmpresa, FndSegNivelesPlanesDto Data)
        {
            return _db.Fnd_SegNiveles_Planes_Actualizar(CodEmpresa, Data);
        }

        public ErrorDto Fnd_SegNiveles_Usuarios_Actualizar(int CodEmpresa, FndSegNivelesUsuariosDto Data)
        {
            return _db.Fnd_SegNiveles_Usuarios_Actualizar(CodEmpresa, Data);
        }
    }
}