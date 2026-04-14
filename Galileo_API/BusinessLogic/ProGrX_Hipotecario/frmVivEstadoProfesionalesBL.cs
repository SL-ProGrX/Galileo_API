using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivEstadoProfesionalesBl
    {
        private readonly FrmVivEstadoProfesionalesDb _db;

        public FrmVivEstadoProfesionalesBl(IConfiguration config)
            => _db = new FrmVivEstadoProfesionalesDb(config);

        public ErrorDto<List<ViviendaContactosData>> ViviendaContactos_Lista_Obtener(int codEmpresa)
        {
            return _db.ViviendaContactos_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_Obtener(int codEmpresa, int idContacto)
        {
            return _db.VivEstadoProfesionales_Obtener(codEmpresa, idContacto);
        }

        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_ConsultaExterna_Obtener(int codEmpresa, string cedula)
        {
            return _db.VivEstadoProfesionales_ConsultaExterna_Obtener(codEmpresa, cedula);
        }

        public ErrorDto VivEstadoProfesionales_Suspender(int codEmpresa, string usuario, ViviendaContactosData request)
        {
            return _db.VivEstadoProfesionales_Suspender(codEmpresa, usuario, request);
        }
    }
}
