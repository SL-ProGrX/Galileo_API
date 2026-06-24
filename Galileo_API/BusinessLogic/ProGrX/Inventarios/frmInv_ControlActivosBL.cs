using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvControlActivosBL
    {
        private readonly FrmInvControlActivosDB _db;

        public FrmInvControlActivosBL(IConfiguration config)
        {
            _db = new FrmInvControlActivosDB(config);
        }

        public ErrorDto<InvControlActivosLista> InvControlActivosLista_Obtener(int CodEmpresa, string usuario, string filtros)
        {
            return _db.InvControlActivosLista_Obtener(CodEmpresa, usuario, filtros);
        }

        public ErrorDto InvControlActivos_Actualizar(int CodEmpresa, InvControlActivosDto activo)
        {
            return _db.InvControlActivos_Actualizar(CodEmpresa, activo);
        }

        public ErrorDto InvNumeroPlacaId_Obtener(int CodEmpresa)
        {
            return _db.InvNumeroPlacaId_Obtener(CodEmpresa);
        }

        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosDepartamentos_Obtener(int CodEmpresa)
        {
            return _db.InvActivosDepartamentos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosSeccion_Obtener(int CodEmpresa, string? departamento)
        {
            return _db.InvActivosSeccion_Obtener(CodEmpresa, departamento);
        }

        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosResponsable_Obtener(int CodEmpresa, string? departamento, string? seccion)
        {
            return _db.InvActivosResponsable_Obtener(CodEmpresa, departamento, seccion);
        }

        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosLocalizaciones_Obtener(int CodEmpresa)
        {
            return _db.InvActivosLocalizaciones_Obtener(CodEmpresa);
        }
    }
}