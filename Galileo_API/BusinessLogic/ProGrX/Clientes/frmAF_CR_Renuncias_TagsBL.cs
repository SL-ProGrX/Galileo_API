using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfCrRenunciasTagsBL
    {
        private readonly FrmAfCrRenunciasTagsDB _db;

        public FrmAfCrRenunciasTagsBL(IConfiguration config)
        {
            _db = new FrmAfCrRenunciasTagsDB(config);
        }

        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Tags_Obtener(int CodEmpresa, string Estado, string Filtro)
        {
            return _db.AF_CR_Renuncias_Tags_Obtener(CodEmpresa, Estado, Filtro);
        }

        public ErrorDto AF_CR_Renuncia_Recepcion_Aplica(int CodEmpresa, AfCrRenunciaRecepcionAplica recepcionDatos)
        {
            return _db.AF_CR_Renuncia_Recepcion_Aplica(CodEmpresa, recepcionDatos);
        }

        public ErrorDto AF_CR_Renuncia_Revision_Aplica(int CodEmpresa, AfCrRenunciaRevisionAplica revisionDatos)
        {
            return _db.AF_CR_Renuncia_Revision_Aplica(CodEmpresa, revisionDatos);
        }

        public ErrorDto<List<AfCrRenunciaEtiquetas>> AF_CR_Renuncia_Etiquetas_Consulta(int CodEmpresa, int RenunciaId)
        {
            return _db.AF_CR_Renuncia_Etiquetas_Consulta(CodEmpresa, RenunciaId);
        }

        public ErrorDto<int> AF_CR_Renuncia_Revision_Reversar_Valida(int CodEmpresa, int RenunciaId)
        {
            return _db.AF_CR_Renuncia_Revision_Reversar_Valida(CodEmpresa, RenunciaId);
        }

        public ErrorDto AF_CR_Renuncia_Revision_Reversar(int CodEmpresa, AfCrRenunciaReversa dto)
        {
            return _db.AF_CR_Renuncia_Revision_Reversar(CodEmpresa, dto);
        }

        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Pendientes_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_Renuncias_Pendientes_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_Renuncias_Obtener(CodEmpresa);
        }
    }
}