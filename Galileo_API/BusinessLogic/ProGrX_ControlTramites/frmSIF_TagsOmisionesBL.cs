namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
    using Galileo_API.Models.ProGrX_ControlTramites;

    public class FrmSifTagsOmisionesBL
    {
        private readonly FrmSifTagsOmisionesDB _db;

        public FrmSifTagsOmisionesBL(IConfiguration config)
        {
            _db = new FrmSifTagsOmisionesDB(config);
        }

        public ErrorDto<List<SifTagsOmisionesModel>> SifTagsOmisiones_Obtener(int CodEmpresa)
        {
            return _db.SifTagsOmisiones_Obtener(CodEmpresa);
        }

        public ErrorDto SifTagsOmisiones_Guardar(int CodEmpresa, SifTagsOmisionesGuardarRequest request)
        {
            return _db.SifTagsOmisiones_Guardar(CodEmpresa, request);
        }

        public ErrorDto SifTagsOmisiones_Eliminar(int CodEmpresa, SifTagsOmisionesEliminarRequest request)
        {
            return _db.SifTagsOmisiones_Eliminar(CodEmpresa, request);
        }

        public ErrorDto<List<SifTagsOmisionesModuloOpcion>> SifTagsOmisiones_Modulos_Obtener(int CodEmpresa)
        {
            return _db.SifTagsOmisiones_Modulos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SifTagsOmisionesAsignacionModel>> SifTagsOmisiones_Asignacion_Obtener(
            int CodEmpresa,
            string Cod_Modulo)
        {
            return _db.SifTagsOmisiones_Asignacion_Obtener(CodEmpresa, Cod_Modulo);
        }

        public ErrorDto SifTagsOmisiones_Asignacion_Guardar(
            int CodEmpresa,
            SifTagsOmisionesAsignacionRequest request)
        {
            return _db.SifTagsOmisiones_Asignacion_Guardar(CodEmpresa, request);
        }
    }
}
